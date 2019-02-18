﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Antenna : MonoBehaviour
{

	private float detectionDistance = 3.0f;
	[SerializeField] private bool drawAntenna = true;
	
	
	public float GetDistanceFromWall()
	{
		if (drawAntenna)
		{
			Debug.DrawRay(transform.position, transform.forward*detectionDistance, Color.red, Time.deltaTime);
		}
		RaycastHit hit;
		Physics.Raycast(transform.position, transform.forward, out hit, detectionDistance);
		return hit.distance;
	}
}