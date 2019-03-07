using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationController : MonoBehaviour {

	[System.Serializable]
	public class MutationSettings
	{
		[Range(0, 10)]
		public float mutationPercentage;
		[Range(0, 30)]
		public float fitPopulationPercentage = 10;
		[Range(0, 50)]
		public float percentageOfUnfitToReplace = 20;

		public bool mutateGenesUsingGeneSets = true;
		public bool spliceGenesUsingSegments = false;
		
		public GameObject carPrefab;
	}
	
	[SerializeField] private MutationSettings[] mutationSettings;
	
	[SerializeField] private GameObject spawnPoint;
	[SerializeField] private GameObject carPrefab;
	[SerializeField] private int numberOfCarsToSpawn;
	[Range(0.5f, 3)]
	[SerializeField] private float timeScale =1;
	private List<GameObject> cars = new List<GameObject>();
	private GenerationalMutator generationalMutator;

	private int numberOfCarsCrashed = 0;
	private int successfulSimulations = 0;
	private int numberOfGenerations = 1;
	private int simulationTimeTaken = 0;
	private bool endPointHit = false;

	public delegate void SimulationRestarted();
	public static SimulationRestarted SimulationRestartedHandler;
	// Use this for initialization
	void Start ()
	{
		Time.timeScale = timeScale;
		generationalMutator = FindObjectOfType<GenerationalMutator>();
		SpawnCars();
		generationalMutator.SetSettings(mutationSettings[0]);
		StartCoroutine(generationalMutator.Evolve(firstGeneration: true));
		CarMovement.CrashedWallHandler += OnCarCrash;
		Checkpoint.EndPointHitHandler += () => endPointHit = true;
		StartCoroutine(KeepTime());
		WriteToFile.ClearTextFile();
		WriteToFile.ClearCSVFile();
		WriteToFile.CreateCSV();
	}

	private void OnEndPointHit()
	{
		string output = String.Format("Simulation complete! Time taken: {0}. Generations used: {1}. Settings used: {2}", simulationTimeTaken, numberOfGenerations, successfulSimulations);
		Debug.LogFormat(output);
		WriteToFile.WriteString(output);
		WriteToFile.WriteToCSV(simulationTimeTaken, numberOfGenerations, successfulSimulations);
		successfulSimulations++;
		if (successfulSimulations >= mutationSettings.Length) successfulSimulations = 0;
		foreach (var car in cars)
		{
			Destroy(car);
		}
		cars.Clear();
		carPrefab = mutationSettings[successfulSimulations].carPrefab;
		generationalMutator.SetSettings(mutationSettings[successfulSimulations]);
		SpawnCars();
		StartCoroutine(generationalMutator.Evolve(firstGeneration: true));
		ResetVariables();
		
	}

	private void ResetVariables()
	{
		numberOfCarsCrashed = 0;
		numberOfGenerations = 1;
		simulationTimeTaken = 0;
		endPointHit = false;
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
		if (endPointHit)
		{
			OnEndPointHit();
		}
		else
		{
			numberOfGenerations++;
			Time.timeScale = timeScale;
			numberOfCarsCrashed = 0;
			ResetCars();
			StartCoroutine(generationalMutator.Evolve());
			if (SimulationRestartedHandler != null) SimulationRestartedHandler.Invoke();
		}
	}

	private void ResetCars()
	{
		foreach (var car in cars)
		{
			car.transform.position = spawnPoint.transform.position;
			car.transform.rotation = spawnPoint.transform.rotation;
		}
	}

	private IEnumerator KeepTime()
	{
		while (true)
		{
			yield return new WaitForSeconds(1);
			simulationTimeTaken++;
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
