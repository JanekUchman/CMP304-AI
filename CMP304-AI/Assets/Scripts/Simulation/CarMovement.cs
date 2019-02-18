﻿using System;
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

	private void Awake()
	{
		Physics.IgnoreLayerCollision(8, 8);
	}

	// Update is called once per frame
	private void Start()
	{
		neuralNet = GetComponentInChildren<NeuralNet>();
		//SimulationController.SimulationRestartedHandler += () => controlState = ControlState.NEURAL;
		
	}

	void Update ()
	{

		switch (controlState)
		{
				case ControlState.HUMAN:
					transform.position += transform.forward * moveSpeed;
					HandleInput();
					break;
				case ControlState.NEURAL:
					transform.position += transform.forward * moveSpeed;
					GetNeuralValues();
					HandleNeuralInput();
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
			inputs.Add(antenna.GetDistanceFromWall());
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
		if (controlState != ControlState.CRASHED && CrashedWallHandler != null)
		{
			CrashedWallHandler.Invoke();
			controlState = ControlState.CRASHED;
		}
	}

	private void HandleInput()
	{
		AdjustSteering(Input.GetAxis("Horizontal"));
		AdjustSpeed(Input.GetAxis("Vertical"));
	}

	private void AdjustSpeed(float inputValue)
	{
		float speedChange = inputValue * accelerationSpeed * Time.deltaTime;
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
		transform.Rotate(0, inputValue*turnSpeed*Time.deltaTime, 0);
	}
}
