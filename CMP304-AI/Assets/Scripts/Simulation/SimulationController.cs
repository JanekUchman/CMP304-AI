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
		[Range(0, 100)]
		public float fitPopulationPercentage = 10;
		[Range(0, 100)]
		public float percentageOfUnfitToReplace = 20;

		public bool mutateGenesUsingGeneSets = true;
		public bool spliceGenesUsingSegments = false;
		
		public GameObject carPrefab;
	}

	[SerializeField] private int settingToStartOn = 0;
	[SerializeField] private MutationSettings[] mutationSettings;
	
	[SerializeField] private GameObject spawnPoint;
	[SerializeField] private GameObject carPrefab;
	[SerializeField] private int numberOfCarsToSpawn;
	[Range(0.5f, 100)]
	[SerializeField] private float timeScale =1;
	private List<GameObject> cars = new List<GameObject>();
	private GenerationalMutator generationalMutator;

	private int numberOfCarsCrashed = 0;
	private int currentSettingNumber = 0;
	private int numberOfGenerations = 1;
	private int lastSimTime = 0;
	private bool endPointHit = false;

	public delegate void SimulationRestarted();
	public static SimulationRestarted SimulationRestartedHandler;
	
	// Use this for initialization
	void Start ()
	{
		currentSettingNumber = settingToStartOn;
		Time.timeScale = timeScale;
		generationalMutator = FindObjectOfType<GenerationalMutator>();
		SpawnCars();
		generationalMutator.SetSettings(mutationSettings[currentSettingNumber]);
		StartCoroutine(generationalMutator.Evolve(firstGeneration: true));
		CarMovement.CrashedWallHandler += OnCarCrash;
		Checkpoint.EndPointHitHandler += () => endPointHit = true;
	}

	private void OnEndPointHit()
	{
		string output = String.Format("Simulation complete! Time taken: {0}. Generations used: {1}. Settings used: {2}", (int)Time.timeSinceLevelLoad - lastSimTime, numberOfGenerations, currentSettingNumber);
		Debug.LogFormat(output);
		WriteToFile.WriteString(output);
		WriteToFile.WriteToCSV((int)Time.timeSinceLevelLoad - lastSimTime, numberOfGenerations, currentSettingNumber);
		lastSimTime = (int)Time.timeSinceLevelLoad;
		currentSettingNumber++;
		if (currentSettingNumber >= mutationSettings.Length) currentSettingNumber = 0;
		foreach (var car in cars)
		{
			Destroy(car);
		}
		cars.Clear();
		carPrefab = mutationSettings[currentSettingNumber].carPrefab;
		generationalMutator.SetSettings(mutationSettings[currentSettingNumber]);
		SpawnCars();
		StartCoroutine(generationalMutator.Evolve(firstGeneration: true));
		ResetVariables();
		
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.A))
			Time.timeScale = 3;
		if (Input.GetKeyDown(KeyCode.S))
			Time.timeScale = timeScale;
	}

	private void ResetVariables()
	{
		numberOfCarsCrashed = 0;
		numberOfGenerations = 1;
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

	private void SpawnCars()
	{
		for (int i = 0; i < numberOfCarsToSpawn; i++)
		{
			cars.Add(Instantiate(carPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation));
			cars[cars.Count - 1].GetComponent<CarMovement>().id = i;
		}
	}
}
