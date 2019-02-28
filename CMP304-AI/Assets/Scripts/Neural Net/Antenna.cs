using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Antenna : MonoBehaviour
{

	[SerializeField] private float detectionDistance = 3.0f;
	
	public float GetDistanceFromWall(bool showAntenna)
	{
		if (showAntenna)
		{
			Debug.DrawRay(transform.position, transform.forward*detectionDistance, Color.red, Time.deltaTime);
		}
		RaycastHit hit;
		if (Physics.Raycast(transform.position, transform.forward, out hit, detectionDistance))
		{
			return hit.distance;
		}
		 else
		{
			return detectionDistance;
		}
	}
}
