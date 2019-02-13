using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarMovement : MonoBehaviour {

	#region enum
	[Serializable]
	private enum ControlState
	{
		HUMAN,
		NEURAL
	}
	#endregion
	#region SetInInspector
	
	[SerializeField] private ControlState controlState;
	[SerializeField] private float moveSpeed = 0.1f;
	[SerializeField] private float turnSpeed = 1;
	[SerializeField] private float accelerationSpeed = 0.01f;
	[SerializeField] private float minSpeed = 0.05f;
	[SerializeField] private float maxSpeed = 0.15f;
	[SerializeField] private Antenna[] antennas = new Antenna[5];
	#endregion

	private NeuralNet neuralNet;
	private List<float> neuralNetOutputs = new List<float>();
	public delegate void CrashedWall(NeuralNet crashedCarNet);
	public static CrashedWall CrashedWallHandler;
	
	
	// Update is called once per frame
	private void Start()
	{
		neuralNet = GetComponentInChildren<NeuralNet>();
	}

	void Update ()
	{
		transform.position += transform.forward * moveSpeed;

		switch (controlState)
		{
				case ControlState.HUMAN:
					HandleInput();
					break;
				case ControlState.NEURAL:
					GetNeuralValues();
					HandleNeuralInput();
					break;
		}
	}

	private void HandleNeuralInput()
	{
		//The values here must match up with the order the output nodes were added on the last layer
		AdjustSteering(neuralNetOutputs[0]);
		AdjustSpeed(neuralNetOutputs[1]);
	}

	private void GetNeuralValues()
	{
		List<float> inputs = new List<float>();
		foreach (var antenna in antennas)
		{
			inputs.Add(antenna.GetDistanceFromWall());
		}
		inputs.Add(moveSpeed);
		neuralNetOutputs = neuralNet.GetNeuralOutput(inputs);
	}

	private void OnTriggerEnter(Collider other)
	{
		CrashedIntoWall();
	}

	private void CrashedIntoWall()
	{
		SceneManager.LoadScene(0);
		if (CrashedWallHandler != null) CrashedWallHandler.Invoke(neuralNet);
	}

	private void HandleInput()
	{
		AdjustSteering(Input.GetAxis("Horizontal"));
		AdjustSpeed(Input.GetAxis("Vertical"));
	}

	private void AdjustSpeed(float inputValue)
	{
		float speedChange = inputValue * accelerationSpeed;
		if (moveSpeed + speedChange < minSpeed)
		{
			moveSpeed = minSpeed;
			return;
		}
		else if (moveSpeed + speedChange > maxSpeed)
		{
			moveSpeed = maxSpeed;
			return;
		}
		else
		{
			moveSpeed += speedChange;
		}
	}

	private void AdjustSteering(float inputValue)
	{
		transform.Rotate(0, inputValue*turnSpeed, 0);
	}
}
