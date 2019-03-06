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
	private int succesfulSimulations = 0;
	private int numberOfGenerations = 0;
	private int simulationTimeTaken = 0;

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
		Checkpoint.EndPointHitHandler += OnEndPointHit;
	}

	private void OnEndPointHit()
	{
		Debug.LogFormat("Simulation complete! Time taken: {0}. Generations used: {1}.", simulationTimeTaken, numberOfGenerations);
		foreach (var car in cars)
		{
			Destroy(car);
		}
		
		carPrefab = mutationSettings[succesfulSimulations].carPrefab;
		generationalMutator.SetSettings(mutationSettings[succesfulSimulations]);
		SpawnCars();
		StartCoroutine(generationalMutator.Evolve(firstGeneration: true));
		ResetVariables();
		succesfulSimulations++;
		if (succesfulSimulations >= mutationSettings.Length) succesfulSimulations = 0;
	}

	private void ResetVariables()
	{
		numberOfCarsCrashed = 0;
		numberOfGenerations = 0;
		simulationTimeTaken = 0;
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
		numberOfGenerations++;
		Time.timeScale = timeScale;
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
