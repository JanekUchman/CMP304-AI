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
		NEURAL,
		CRASHED
	}
	#endregion
	#region SetInInspector
	
	[SerializeField] private ControlState controlState;
	[SerializeField] private float spinOutTime = 1.0f;
	[SerializeField] private float moveSpeed = 0.1f;
	[SerializeField] private float turnSpeed = 1;
	[SerializeField] private float accelerationSpeed = 0.01f;
	[SerializeField] private float minSpeed = 0.05f;
	[SerializeField] private float maxSpeed = 0.15f;
	[SerializeField] private Antenna[] antennas = new Antenna[5];
	#endregion

	private NeuralNet neuralNet;
	private List<float> neuralNetOutputs = new List<float>();
	public delegate void CrashedWall();
	public static CrashedWall CrashedWallHandler;
	public int id;
	public bool trackStats = false;

	private void Awake()
	{
		neuralNet = GetComponentInChildren<NeuralNet>();
		SimulationController.SimulationRestartedHandler += () => StartCoroutine(OnSimulationRestart());
		Physics.IgnoreLayerCollision(8, 8);
	}

	private IEnumerator OnSimulationRestart()
	{
		yield return new WaitForSeconds(0.1f);
		moveSpeed = minSpeed;
		controlState = ControlState.NEURAL;
		StopCoroutine("SpinOutTimer");
		StartCoroutine("SpinOutTimer");
	}

	void FixedUpdate ()
	{

		switch (controlState)
		{
				case ControlState.HUMAN:
					transform.position += transform.forward * (moveSpeed );
					HandleInput();
					break;
				case ControlState.NEURAL:
					GetNeuralValues();
					HandleNeuralInput();
					transform.position += transform.forward * (moveSpeed );
					break;
				case ControlState.CRASHED:
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
			inputs.Add(antenna.GetDistanceFromWall(trackStats));
		}
		inputs.Add(moveSpeed);
		neuralNetOutputs = neuralNet.GetNeuralOutput(inputs);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Wall"))
			CrashedIntoWall();
	}

	private void CrashedIntoWall()
	{
		if (controlState != ControlState.CRASHED)
		{
			StopCoroutine("SpinOutTimer");
			controlState = ControlState.CRASHED;
			CrashedWallHandler.Invoke();
		}
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
		}
		else if (moveSpeed + speedChange > maxSpeed)
		{
			moveSpeed = maxSpeed;
		}
		else
		{
			moveSpeed += speedChange;
		}
	}

	public void HitCheckpoint(float fitnessIncrease)
	{
		if (trackStats)
		{
			Debug.Log(neuralNet.fitness);
		}

		neuralNet.fitness += fitnessIncrease;
		Checkpoint.InvokeCheckpointHit(neuralNet.fitness, gameObject);
		StopCoroutine("SpinOutTimer");
		StartCoroutine("SpinOutTimer");
	}

	private IEnumerator SpinOutTimer()
	{
		yield return new WaitForSeconds(spinOutTime);
		neuralNet.fitness /= 2;
		CrashedIntoWall();
	}

	private void AdjustSteering(float inputValue)
	{
		transform.Rotate(0, inputValue*turnSpeed, 0);
	}
}
