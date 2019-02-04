using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour {

	private Text counter;

	// Use this for initialization
	void Start ()
	{
		counter = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
		counter.text = String.Format("FPS: {0}",1.0f / Time.deltaTime);
	}
}
