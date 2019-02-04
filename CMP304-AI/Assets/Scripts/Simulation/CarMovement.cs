using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour {

	#region variables
	[Serializable]
	private enum ControlState
	{
		HUMAN,
		NEURAL
	}

	[SerializeField] private ControlState controlState;
	[SerializeField] private float moveSpeed = 0.1f;
	[SerializeField] private float turnSpeed = 1;
	[SerializeField] private float accelerationSpeed = 0.01f;
	

	#endregion
	// Update is called once per frame
	void Update ()
	{
		transform.position += transform.forward * moveSpeed;

		switch (controlState)
		{
				case ControlState.HUMAN:
					HandleInput();
					break;
				case ControlState.NEURAL:
					break;
		}
	}

	private void HandleInput()
	{
		AddRotation(Input.GetAxis("Horizontal"));
	}

	public void AddRotation(float inputValue)
	{
		transform.Rotate(0, inputValue*turnSpeed, 0);
	}
}
