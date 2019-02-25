using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {
	[SerializeField] private float fitnessValue = 0.1f;

	public delegate void CheckpointHit(float carFitness);
	public static CheckpointHit CheckpointHitHandler;
	
	private List<int> carsPassed = new List<int>();
	
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Car") && !carsPassed.Contains(other.gameObject.GetComponent<CarMovement>().id))
		{
			carsPassed.Add(other.gameObject.GetComponent<CarMovement>().id);
			other.gameObject.GetComponentInChildren<CarMovement>().HitCheckpoint(fitnessValue);
		}
	}

	private void Start()
	{
		SimulationController.SimulationRestartedHandler += carsPassed.Clear;
	}

	public static void InvokeCheckpointHit(float neuralFitness)
	{
		if (CheckpointHitHandler != null) CheckpointHitHandler.Invoke(neuralFitness);
	}
}
