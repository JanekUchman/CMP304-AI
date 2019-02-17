using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationController : MonoBehaviour {

	[SerializeField] private GameObject spawnPoint;
	[SerializeField] private GameObject carPrefab;
	[SerializeField] private int numberOfCarsToSpawn;
	private List<GameObject> cars = new List<GameObject>();
	private GenerationalMutator generationalMutator;

	private int numberOfCarsCrashed;

	public delegate void SimulationRestarted();
	public static SimulationRestarted SimulationRestartedHandler;
	// Use this for initialization
	void Start ()
	{
		generationalMutator = FindObjectOfType<GenerationalMutator>();
		QualitySettings.vSyncCount = 0;
		SpawnCars();
		generationalMutator.Evolve(firstGeneration: true);
		CarMovement.CrashedWallHandler += OnCarCrash;
	}

	private void OnCarCrash()
	{
		numberOfCarsCrashed++;
		if (numberOfCarsCrashed == numberOfCarsToSpawn)
		{
			ResetSimulation();
		}
	}

	private void ResetSimulation()
	{
		numberOfCarsCrashed = 0;
		ResetCars();
		if (SimulationRestartedHandler != null) SimulationRestartedHandler.Invoke();
	}

	private void ResetCars()
	{
		foreach (var car in cars)
		{
			car.transform.position = spawnPoint.transform.position;
			car.transform.rotation = spawnPoint.transform.rotation;
		}
	}

	private void SpawnCars()
	{
		for (int i = 0; i < numberOfCarsToSpawn; i++)
		{
			cars.Add(Instantiate(carPrefab, spawnPoint.transform));
		}
	}
}
