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
	//THe neural nets of the cars currently in simulation
	private List<NeuralNet> currentGenomeGeneration = new List<NeuralNet>();
	//The mutation percentage converted to a float
	private float MutationRate {get	{return mutationPercentage / 100;}}
	//The top percentage of individuals, hard coded to 1/3rd for testing
	private int NumberOfFitIndividuals
	{
		get
		{ 
			int value = (int)(currentGenomeGeneration.Count / (1f / 3f));
			//We need at least two parents
			if (value < 2) value = 2;
			//We don't want an odd number of parents
			if (value % 2 != 0) value+= 1;
			return value;
		}
	}

	
	private void SetIndividuals()
	{
		//Get all the neural nets from the scene
		currentGenomeGeneration = FindObjectsOfType<NeuralNet>().ToList();
	}

	public void Evolve(bool firstGeneration = false)
	{
		if (firstGeneration)
		{
			//If it's the first generation we don't want to mutate their genes yet, just populate them
			SetIndividuals();
			InitialEvolution();
		}
		else
		{
			CreateNewGeneration();
		}
	}

	private void CreateNewGeneration()
	{
		List<NeuralNet> fittestGenomes = SelectFittestIndividuals();
		List<NeuralNet> leastFitGenomes = SelectLeastFitIndividuals(fittestGenomes);
		int chromosomeLength = currentGenomeGeneration[0].GetChromosomeLength();
		List<Queue<float>> childChromosomes = new List<Queue<float>>();
		List<Queue<float>> parentChromosomes = new List<Queue<float>>();
		
		//Cut the fit genes up and cross them over creating new children
		SpliceAndMutate(fittestGenomes, chromosomeLength, ref parentChromosomes, ref childChromosomes);

		//Replace the least fit individuals with the child chromosomes of the fittest genes
		for (int i = 0; i < childChromosomes.Count-1; i++)
		{
			leastFitGenomes[i].ApplyChromosome(childChromosomes[i]);
		}
		
		//Re-apply the mutated parent chromosomes
		for (int i = 0; i < parentChromosomes.Count-1; i++)
		{
			fittestGenomes[i].ApplyChromosome(parentChromosomes[i]);
		}
		//Clear the old genomes from the list NOTE may not be necessary, depends on C# handling of list references
		currentGenomeGeneration.Clear();
		//Add the parent genomes
		foreach (var genome in fittestGenomes)
		{
			currentGenomeGeneration.Add(genome);
		}
		//Add the child genomes
		foreach (var genome in leastFitGenomes)
		{
			currentGenomeGeneration.Add(genome);
		}
	}

	private void SpliceAndMutate(List<NeuralNet> fittestGenomes, int chromosomeLength, ref List<Queue<float>> parentChromosomes, ref List<Queue<float>> childChromosomes)
	{
		float[] motherChromosome;
		float[] fatherChromosome;
		ArraySegment<float> fatherSegment;
		ArraySegment<float> motherSegment;
		//We want to increase the counter by two, to allow for each parent
		for (int i = 0; i < NumberOfFitIndividuals; i += 2)
		{
			//check both parents are referenced, just in case we've gone off the end of the list
			if (fittestGenomes[i + 1] == null || fittestGenomes[i] == null) continue;
			
			//Create splices from the mother and father chromosome
			SpliceGenes(fittestGenomes, chromosomeLength, i, out motherChromosome, out fatherChromosome, out fatherSegment, out motherSegment);

			//Combine and mutate the spliced genes
			MutateGenes(chromosomeLength, parentChromosomes, childChromosomes, fatherSegment, motherSegment, fatherChromosome, motherChromosome);
		}
	}

	private static void SpliceGenes(List<NeuralNet> fittestGenomes, int chromosomeLength, int i, out float[] motherChromosome,
		out float[] fatherChromosome, out ArraySegment<float> fatherSegment, out ArraySegment<float> motherSegment)
	{
		//Get a random point to splice the genes
		int slicePoint = Random.Range(1, chromosomeLength - 1);

		//Get the chromosomes of the 1st and 2nd point of the array, the next two fittest individuals
		motherChromosome = fittestGenomes[i].ExtractChromosome().ToArray();
		fatherChromosome = fittestGenomes[i + 1].ExtractChromosome().ToArray();

		//Segment the array based on the splice point
		//Take the father's genes up to the splice point
		fatherSegment = new ArraySegment<float>(fatherChromosome, 0, slicePoint);
		//And the mother's genes after the splice point
		motherSegment = new ArraySegment<float>(motherChromosome, slicePoint, chromosomeLength - slicePoint);
	}

	private void MutateGenes(int chromosomeLength, List<Queue<float>> parentChromosomes, List<Queue<float>> childChromosomes,
		ArraySegment<float> fatherSegment, ArraySegment<float> motherSegment, float[] fatherChromosome, float[] motherChromosome)
	{
		//Get the new chromosome based on the gene slices
		var newChromosome = CreateChild(chromosomeLength, fatherSegment, motherSegment);
		//Mutate all of the chromosomes individually
		MutateChromosome(chromosomeLength, ref newChromosome);
		MutateChromosome(chromosomeLength, ref fatherChromosome);
		MutateChromosome(chromosomeLength, ref motherChromosome);

		//Re-add the chromosomes to the gene pool
		parentChromosomes.Add(new Queue<float>(motherChromosome));
		parentChromosomes.Add(new Queue<float>(fatherChromosome));
		childChromosomes.Add(new Queue<float>(newChromosome));
	}

	//Takes two array segments and combines them into a new genome
	private static float[] CreateChild(int chromosomeLength, ArraySegment<float> fatherSegment, ArraySegment<float> motherSegment)
	{
		float[] newChromosome = new float[chromosomeLength];
		//Add the father segment to the start of the array, since this is where we took it from earlier
		fatherSegment.Array.CopyTo(newChromosome, 0);
		//Add the mother's genes onto the end of the fathers
		motherSegment.Array.CopyTo(newChromosome, fatherSegment.Count - 1);
		return newChromosome;
	}

	private void MutateChromosome(int chromosomeLength, ref float[] newChromosome)
	{
		for (int i = 0; i < chromosomeLength; i++)
		{
			//If the random lands below our percentage rate, mutate the gene
			if (Random.Range(0f, 1f) < MutationRate)
			{
				//Mutation involves randomly applying a value to the gene
				newChromosome[i] = Random.Range(-1f, 1f);
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
		for (int i = 0; i < NumberOfFitIndividuals; i ++)
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
