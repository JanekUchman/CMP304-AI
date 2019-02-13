using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNode : MonoBehaviour
{
	public enum ActivationFunctionTypes
	{
		LOGISTIC_SIGMOID,
		HYPERBOLIC_TANGENT
	}

	//Set in inspector
	[SerializeField] private NeuralNodeConnection[] inputConnections;
	public float weighting;
	private List<float> outputValues = new List<float>();
	
	private float output;

	public float Output
	{
		get { return output; }
	}

	public void SetOutput(ActivationFunctionTypes activationFunctionUsed)
	{
		foreach (var nodeConnection in inputConnections)
		{
			SumInput(nodeConnection);
		}
		ApplyActivationFunction(activationFunctionUsed);
	}

	//Use for the first layer of nodes
	public void SetInputNodeValue(float output)
	{
		this.output = output;
	}

	private void SumInput(NeuralNodeConnection nodeConnection)
	{
		output += nodeConnection.inputNode.output * nodeConnection.weighting;
	}

	private void ApplyActivationFunction(ActivationFunctionTypes activationFunctionUsed)
	{
		switch (activationFunctionUsed)
		{
			case ActivationFunctionTypes.LOGISTIC_SIGMOID:
				output= Sigmoid(output);
				break;
			case ActivationFunctionTypes.HYPERBOLIC_TANGENT:
				output= TanH(output);
				break;
		}
	}

	private float Sigmoid(float outputValue)
	{
		if (outputValue > 10) return 1.0f;
		else if (outputValue < -10) return 0.0f;
		else return (float)(1.0 / (1.0 + Math.Exp(-outputValue)));
	}

	private float TanH(float outputValue)
	{
		if (outputValue > 10) return 1.0f;
		else if (outputValue < -10) return 0.0f;
		else return (float)Math.Tanh(outputValue);
	}

	[Serializable]
	public class NeuralNodeConnection
	{
		public float weighting;
		public NeuralNode inputNode;
	}
}
