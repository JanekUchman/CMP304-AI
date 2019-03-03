using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField] private float lerpSpeed = 10;
	private float fittestCarFitness;
	private Transform fittestCar;
	// Use this for initialization
	void Start ()
	{
		Checkpoint.CheckpointHitHandler += OnCheckpointHit;
		SimulationController.SimulationRestartedHandler += () => fittestCarFitness = 0;
	}

	private void OnCheckpointHit(float carFitness, GameObject car)
	{
		if (carFitness > fittestCarFitness)
		{
			fittestCarFitness = carFitness;
			fittestCar = car.transform;
		}
	}

	private void Update()
	{
		if (fittestCar != null)
		{
			transform.position = Vector3.Lerp(transform.position, new Vector3(fittestCar.position.x, transform.position.y, fittestCar.position.z), lerpSpeed*Time.deltaTime);
		}
	}
}
