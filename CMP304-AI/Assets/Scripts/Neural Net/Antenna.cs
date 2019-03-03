using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Antenna : MonoBehaviour
{

	[SerializeField] private float detectionDistance = 3.0f;
	[SerializeField] private float distanceFromWall;
	public float GetDistanceFromWall(bool showAntenna)
	{
		var dist = 0.0f;
		RaycastHit hit;
		if (Physics.Raycast(transform.position, transform.forward, out hit, detectionDistance))
		{
			dist =  hit.distance;
		}
		 else
		{
			dist = detectionDistance;
		}
		if (showAntenna)
		{
			distanceFromWall = dist;
			Debug.DrawRay(transform.position, transform.forward*dist, Color.red, Time.fixedDeltaTime);
		}

		return dist;
	}
}
