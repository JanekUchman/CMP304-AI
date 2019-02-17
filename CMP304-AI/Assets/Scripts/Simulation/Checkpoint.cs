using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {
	[SerializeField] private float fitnessValue = 0.1f;

	public delegate void CheckpointHit(float carFitness);
	public static CheckpointHit CheckpointHitHandler;
	
	private void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.CompareTag("Car"))
		{
			float fitness = other.gameObject.GetComponentInChildren<NeuralNet>().fitness += fitnessValue;
			if (CheckpointHitHandler != null) CheckpointHitHandler.Invoke(fitness);
		}
	}
}
