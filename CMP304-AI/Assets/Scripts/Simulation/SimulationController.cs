using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationController : MonoBehaviour {

	[SerializeField] private GameObject spawnPoint;
	[SerializeField] private GameObject carPrefab;
	[SerializeField] private int numberOfCarsToSpawn;
	[Range(0.5f, 3)]
	[SerializeField] private float timeScale =1;
	private List<GameObject> cars = new List<GameObject>();
	private GenerationalMutator generationalMutator;

	private int numberOfCarsCrashed = 0;

	public delegate void SimulationRestarted();
	public static SimulationRestarted SimulationRestartedHandler;
	// Use this for initialization
	void Start ()
	{
		Time.timeScale = timeScale;
		generationalMutator = FindObjectOfType<GenerationalMutator>();
		SpawnCars();
		StartCoroutine(generationalMutator.Evolve(firstGeneration: true));
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
		Time.timeScale = timeScale;
		//Time.fixedDeltaTime = 0.0167f * Time.timeScale;
		numberOfCarsCrashed = 0;
		ResetCars();
		StartCoroutine(generationalMutator.Evolve());
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
			cars.Add(Instantiate(carPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation));
			cars[cars.Count - 1].GetComponent<CarMovement>().id = i;
		}
	}
}
