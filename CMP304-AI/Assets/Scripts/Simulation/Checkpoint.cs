using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {
	[SerializeField] private float fitnessValue = 0.1f;
	[SerializeField] private bool isEndPoint;
	public delegate void CheckpointHit(float carFitness, GameObject car);
	public static CheckpointHit CheckpointHitHandler;

	public delegate void EndPointHit();
	public static EndPointHit EndPointHitHandler;
	private List<int> carsPassed = new List<int>();
	
	private void OnTriggerEnter(Collider other)
	{
		
		if (other.gameObject.CompareTag("Car") && !carsPassed.Contains(other.gameObject.GetComponent<CarMovement>().id))
		{
			if (isEndPoint)
			{
				EndPointHitHandler.Invoke();
			}
			carsPassed.Add(other.gameObject.GetComponent<CarMovement>().id);
			other.gameObject.GetComponentInChildren<CarMovement>().HitCheckpoint(fitnessValue);
		}
	}

	private void Start()
	{
		SimulationController.SimulationRestartedHandler += carsPassed.Clear;
		EndPointHitHandler += carsPassed.Clear;
	}

	public static void InvokeCheckpointHit(float neuralFitness, GameObject car)
	{
		CheckpointHitHandler?.Invoke(neuralFitness, car);
	}
}
