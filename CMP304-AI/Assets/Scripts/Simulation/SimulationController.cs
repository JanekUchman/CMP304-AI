using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationController : MonoBehaviour {

	public static SimulationController instance;

	[SerializeField] 
	private int simulationSpeed;

	public int SimulationSpeed
	{
		get { return simulationSpeed; }
		set
		{
			simulationSpeed = value;
			Application.targetFrameRate = value;
		}
	}

	// Use this for initialization
	void Start ()
	{
		//Set up the singleton
		if (instance == null) instance = this;
		else DestroyImmediate(this);

		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = simulationSpeed;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
