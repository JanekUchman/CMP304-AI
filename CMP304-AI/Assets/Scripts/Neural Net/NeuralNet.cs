using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNet : MonoBehaviour {

	#region SetInInspector
	[SerializeField] private NeuralNode.ActivationFunctionTypes activationFunctionType = NeuralNode.ActivationFunctionTypes.HYPERBOLIC_TANGENT;
	[SerializeField] private NeuralLayer[] layers;
	#endregion
	
	[HideInInspector] public float fitness = 0;
	const int inputLayer = 0;

	public List<float> GetNeuralOutput(List<float> inputs)
	{
		if (inputs.Count != layers[inputLayer].nodes.Length) Debug.LogError("Neural Net: Input node count mismatch, check input layers and input values.");
		//First we set the first layer of nodes from our input values retrieved from the simulation
		for (int i = 0; i < inputs.Count-1; i++)
		{
			layers[inputLayer].nodes[i].SetInputNodeValue(inputs[i]);
		}
		
		//Then using those values we cycle down the layers, starting with the 1st hidden layer
		for (int i = 1; i < layers.Length; i ++) 
		{
			//We set the output based on the previous layers outputs
			foreach (var node in layers[i].nodes)
			{
				node.SetOutput(activationFunctionType);
			}
		}

		//The last layer's outputs are taken and returned to be fed back to the simulation
		List<float> outputValues = new List<float>();
		foreach (var outputLayerNode in layers[layers.Length-1].nodes)
		{
			outputValues.Add(outputLayerNode.Output);
		}

		return outputValues;
	}
	
	[System.Serializable]
	public class NeuralLayer
	{
		public NeuralNode[] nodes;
	}
}
