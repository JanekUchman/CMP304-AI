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

	//Weighting should be set in mutations and only adjusted before the start of the simulation
	//Input nodes should be set in the inspector
	public NeuralNodeConnection[] inputConnections;

	//Bias is set from the mutator
	[HideInInspector]
	public float bias;
	
	//The final summed product after processing inputs and weightings
	private float output;
	public float Output
	{
		get { return output; }
	}
	
	//Called from the neural net, sets the outputs based on node's connections
	//Used for the hidden layers and final output layer
	public void SetOutput(ActivationFunctionTypes activationFunctionUsed)
	{
		//Cycle through all the inputs into this node
		foreach (var nodeConnection in inputConnections)
		{
			//Add them together, including the weighting for each individual node
			SumInput(nodeConnection);
		}
		//Add the node's bias on to outputs
		output += bias;
		//Adjust the data so we receive sensible results
		ApplyActivationFunction(activationFunctionUsed);
	}

	//Used for the first layer of nodes
	public void SetInputNodeValue(float output)
	{
		this.output = output;
	}

	//Get the last nodes output and weighting, sum it to ours
	private void SumInput(NeuralNodeConnection nodeConnection)
	{
		output += nodeConnection.inputNode.output * nodeConnection.weight;
	}

	//Switch between different types of activation functions
	//As all nodes should use the same function, set in Neural Net
	private void ApplyActivationFunction(ActivationFunctionTypes activationFunctionUsed)
	{
		switch (activationFunctionUsed)
		{
			case ActivationFunctionTypes.LOGISTIC_SIGMOID:
				output= ActivationSigmoid(output);
				break;
			case ActivationFunctionTypes.HYPERBOLIC_TANGENT:
				output= ActivationTanH(output);
				break;
		}
	}

	//Activation function sigmoid
	private float ActivationSigmoid(float outputValue)
	{
		if (outputValue > 10) return 1.0f;
		else if (outputValue < -10) return 0.0f;
		else return (float)(1.0 / (1.0 + Math.Exp(-outputValue)));
	}

	private float ActivationTanH(float outputValue)
	{
		if (outputValue > 10) return 1.0f;
		else if (outputValue < -10) return 0.0f;
		else return (float)Math.Tanh(outputValue);
	}

	[Serializable]
	public class NeuralNodeConnection
	{
		[HideInInspector]
		public float weight;
		public NeuralNode inputNode;
	}
}
