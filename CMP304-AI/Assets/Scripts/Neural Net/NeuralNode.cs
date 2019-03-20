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
		output = 0;
		//Cycle through all the inputs into this node
		foreach (var nodeConnection in inputConnections)
		{
			//Add them together, including the weighting for each individual node
			SumInput(nodeConnection);
		}
		//Add the node's bias on to outputs
		output += bias;
		//Adjust the data so we receive sensible results
		ActivationTanH(output);
	}

	//Used for the first layer of nodes
	public void SetInputNodeValue(float output)
	{
		this.output = ActivationTanH(output);
	}

	//Get the last nodes output and weighting, sum it to ours
	private void SumInput(NeuralNodeConnection nodeConnection)
	{
		output += nodeConnection.inputNode.output * nodeConnection.weight;
	}

	
	private float ActivationTanH(float outputValue)
	{
		return (float)Math.Tanh(outputValue);
	}

	[Serializable]
	public class NeuralNodeConnection
	{
		[HideInInspector]
		public float weight;
		public NeuralNode inputNode;
	}
}
