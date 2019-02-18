using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

//A chromosome is a collection of weights and biases
//A genome is a neural net
//A gene is a single part of a chromosome

public class GenerationalMutator : MonoBehaviour
{
	[SerializeField] private float mutationPercentage;
	private List<NeuralNet> currentGenomeGeneration = new List<NeuralNet>();
	private Queue<float> chromosomes = new Queue<float>();
	private float MutationRate {get	{return mutationPercentage / 100;}}
	private int numberOfFitIndividuals;


	private void Start()
	{
		
	}

	private void SetIndividuals()
	{
		currentGenomeGeneration = FindObjectsOfType<NeuralNet>().ToList();
	}

	public void Evolve(bool firstGeneration = false)
	{
		if (firstGeneration)
		{
			SetIndividuals();
			InitialEvolution();
		}
		else
		{
			Mutate();
		}
	}

	private void Mutate()
	{
		List<NeuralNet> fittestGenomes = SelectFittestIndividuals();
		List<NeuralNet> leastFitGenomes = SelectLeastFitIndividuals(fittestGenomes);
		int chromosomeLength = currentGenomeGeneration[0].GetChromosomeLength();
		List<Queue<float>> childChromosomes = new List<Queue<float>>();
		List<Queue<float>> parentChromosomes = new List<Queue<float>>();
		
		SpliceAndMutate(fittestGenomes, chromosomeLength, parentChromosomes, childChromosomes);

		for (int i = 0; i < childChromosomes.Count-1; i++)
		{
			leastFitGenomes[i].ApplyChromosome(childChromosomes[i]);
		}
		
		for (int i = 0; i < parentChromosomes.Count-1; i++)
		{
			fittestGenomes[i].ApplyChromosome(parentChromosomes[i]);
		}
		currentGenomeGeneration.Clear();
		foreach (var genome in fittestGenomes)
		{
			currentGenomeGeneration.Add(genome);
		}

		foreach (var genome in leastFitGenomes)
		{
			currentGenomeGeneration.Add(genome);
		}
	}

	private void SpliceAndMutate(List<NeuralNet> fittestGenomes, int chromosomeLength, List<Queue<float>> parentChromosomes, List<Queue<float>> childChromosomes)
	{
		for (int i = 0; i < numberOfFitIndividuals; i += 2)
		{
			if (fittestGenomes[i + 1] == null || fittestGenomes[i] == null) continue;
			int slicePoint = Random.Range(1, chromosomeLength - 1);

			var motherChromosome = fittestGenomes[i].ExtractChromosome().ToArray();
			var fatherChromosome = fittestGenomes[i + 1].ExtractChromosome().ToArray();

			ArraySegment<float> fatherSegment = new ArraySegment<float>(fatherChromosome, 0, slicePoint);
			ArraySegment<float> motherSegment =
				new ArraySegment<float>(motherChromosome, slicePoint, chromosomeLength - slicePoint);

			var newChromosome = CreateChild(chromosomeLength, fatherSegment, motherSegment);
			MutateChromosome(chromosomeLength, ref newChromosome);
			MutateChromosome(chromosomeLength, ref fatherChromosome);
			MutateChromosome(chromosomeLength, ref motherChromosome);

			parentChromosomes.Add(new Queue<float>(motherChromosome));
			parentChromosomes.Add(new Queue<float>(fatherChromosome));
			childChromosomes.Add(new Queue<float>(newChromosome));
		}
	}

	private static float[] CreateChild(int chromosomeLength, ArraySegment<float> fatherSegment, ArraySegment<float> motherSegment)
	{
		float[] newChromosome = new float[chromosomeLength];
		fatherSegment.Array.CopyTo(newChromosome, 0);
		motherSegment.Array.CopyTo(newChromosome, fatherSegment.Count - 1);
		return newChromosome;
	}

	private void MutateChromosome(int chromosomeLength, ref float[] newChromosome)
	{
		for (int j = 0; j < chromosomeLength; j++)
		{
			if (Random.Range(0f, 1f) < MutationRate)
			{
				newChromosome[j] = Random.Range(-1f, 1f);
			}
		}
	}

	private List<NeuralNet> SelectLeastFitIndividuals(List<NeuralNet> fittestGenomes)
	{
		List<NeuralNet> leastFitGenomes = currentGenomeGeneration;
		foreach (var fittestGenome in fittestGenomes)
		{
			leastFitGenomes.Remove(fittestGenome);
		}

		return leastFitGenomes;
	}


	private List<NeuralNet> SelectFittestIndividuals()
	{
		List<NeuralNet> fittestGenomes = new List<NeuralNet>();
		List<NeuralNet> sortedByFittest = currentGenomeGeneration.OrderBy(o=>o.fitness).ToList();
		numberOfFitIndividuals = (int)(currentGenomeGeneration.Count / (1f/3f));
		if (numberOfFitIndividuals % 2 != 0) numberOfFitIndividuals += 1;
		if (numberOfFitIndividuals < 2) numberOfFitIndividuals = 2;
		for (int i = 0; i < numberOfFitIndividuals; i ++)
		{
			fittestGenomes.Add(sortedByFittest[i]);
		}
		return fittestGenomes;
	}
	
	public GameObject GetFittestCar()
	{
		NeuralNet fittest = currentGenomeGeneration[0];
		foreach (var neuralNet in currentGenomeGeneration)
		{
			if (neuralNet.fitness > fittest.fitness)
			{
				fittest = neuralNet;
			}
		}
		return fittest.gameObject;
	}
	
	private void InitialEvolution()
	{
		int chromosomeLength = currentGenomeGeneration[0].GetChromosomeLength();
		Queue<float> randomChromosome = new Queue<float>();
		foreach (var genome in currentGenomeGeneration)
		{
			for (int i = 0; i < chromosomeLength; i++)
			{
				randomChromosome.Enqueue(Random.Range(-1f, 1f));
			}
			genome.ApplyChromosome(randomChromosome);
		}
	}
}
