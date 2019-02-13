using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNet : MonoBehaviour {

	[SerializeField] private NeuralNode.ActivationFunctionTypes activationFunctionType = NeuralNode.ActivationFunctionTypes.HYPERBOLIC_TANGENT;
	[SerializeField] private NeuralLayer[] layers;

	const int inputLayer = 0;

	public List<float> GetNeuralOutput(float[] inputs)
	{
		if (inputs.Length != layers[inputLayer].nodes.Length) Debug.LogError("Neural Net: Input node mismatch, check input layers and input values.");
		for (int i = 0; i < inputs.Length; i++)
		{
			layers[inputLayer].nodes[i].SetInputNodeValue(inputs[i]);
		}
		
		for (int i = 1; i < layers.Length; i ++) 
		{
			foreach (var node in layers[i].nodes)
			{
				node.SetOutput(activationFunctionType);
			}
		}

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
