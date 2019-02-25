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

	[SerializeField] private float fitPopulationPercentage = 10;
	//THe neural nets of the cars currently in simulation
	private List<NeuralNet> currentGenomeGeneration = new List<NeuralNet>();

	private List<NeuralNet> CurrentGenomeGeneration
	{
		get
		{
			if (currentGenomeGeneration.Count == 0)
			{
				SetIndividuals();
			}

			return currentGenomeGeneration;
		}
	}

	private int numberOfGenes
	{
		get
		{
			if (CurrentGenomeGeneration[0] != null)
			{
				return CurrentGenomeGeneration[0].GetChromosomeLength();
			}
			else
			{
				return 0;
			}
		}
	}
	
	//The top percentage of individuals, hard coded to 1/3rd for testing
	private int NumberOfFit
	{
		get
		{ 
			int value = (int)(CurrentGenomeGeneration.Count * (fitPopulationPercentage/100));
//			//We need at least two parents
//			if (value < 2) value = 2;
//			//We don't want an odd number of parents
//			if (value % 2 != 0) value+= 1;
			return value;
		}
	}
	
	//The top percentage of individuals, hard coded to 1/3rd for testing
	private int NumberOfUnfit
	{
		get
		{ 
			int value = (int)(CurrentGenomeGeneration.Count * ((100-fitPopulationPercentage)/100));
//			//We need at least two parents
//			if (value < 2) value = 2;
//			//We don't want an odd number of parents
//			if (value % 2 != 0) value+= 1;
			return value;
		}
	}

	
	private void SetIndividuals()
	{
		//Get all the neural nets from the scene
		currentGenomeGeneration = FindObjectsOfType<NeuralNet>().ToList();
	}

	public IEnumerator Evolve(bool firstGeneration = false)
	{
		if (firstGeneration)
		{
			//We have to wait a frame so the gameobjects are spawned
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			//If it's the first generation we don't want to mutate their genes yet, just populate them
			InitialEvolution();
		}
		else
		{
			CreateNewGeneration();
		}
	}

	private void CreateNewGeneration()
	{
		SetIndividuals();
		List<NeuralNet> fittestGenomes = SelectFittestIndividuals();
		List<NeuralNet> leastFitGenomes = SelectLeastFitIndividuals(fittestGenomes);
		List<Queue<float>> childChromosomes = new List<Queue<float>>();
		List<Queue<float>> parentChromosomes = new List<Queue<float>>();

		foreach (var gene in fittestGenomes[0].ExtractChromosome())
		{
			Debug.Log(gene);
		}
		
		//Cut the fit genes up and cross them over creating new children
		SpliceLeastFitWithFittest(fittestGenomes, leastFitGenomes, parentChromosomes, childChromosomes);

		//Replace the least fit individuals with the child chromosomes of the fittest genes
		for (int i = 0; i < childChromosomes.Count-1; i++)
		{
			currentGenomeGeneration[i].ApplyChromosome(childChromosomes[i]);
		}
		
		//Re-apply the mutated parent chromosomes
		for (int j = 0; j < NumberOfFit-1; j++)
		{
			int k = j + childChromosomes.Count -1;
			currentGenomeGeneration[k].ApplyChromosome(fittestGenomes[j].ExtractChromosome());
		}
//		//Clear the old genomes from the list NOTE may not be necessary, depends on C# handling of list references
//		currentGenomeGeneration.Clear();
//		//Add the parent genomes
//		foreach (var genome in fittestGenomes)
//		{
//			currentGenomeGeneration.Add(genome);
//		}
//		//Add the child genomes
//		foreach (var genome in leastFitGenomes)
//		{
//			currentGenomeGeneration.Add(genome);
//		}
	}

	private void SpliceLeastFitWithFittest(List<NeuralNet> fittestGenomes, List<NeuralNet> leastFitGenomes, List<Queue<float>> parentChromosomes,
		List<Queue<float>> childChromosomes)
	{
		float[] motherChromosome;
		float[] fatherChromosome;
		List<float> fatherSegment;
		List<float> motherSegment;
		//We want to increase the counter by two, to allow for each parent
		for (int i = 0; i < NumberOfUnfit; i ++)
		{
			//Get the chromosomes of the 1st and 2nd point of the array, the next two fittest individuals
			motherChromosome = fittestGenomes[Random.Range(0, NumberOfFit-1)].ExtractChromosome().ToArray();
			fatherChromosome = leastFitGenomes[i].ExtractChromosome().ToArray();

			//Create splices from the mother and father chromosome
			SpliceGenes(motherChromosome, fatherChromosome, out fatherSegment,
				out motherSegment);

			//Combine and mutate the spliced genes
			MutateGenes(numberOfGenes, parentChromosomes, childChromosomes, fatherSegment, motherSegment, fatherChromosome,
				motherChromosome, false);
		}
	}

	private void SpliceFittestWithFittest(List<NeuralNet> fittestGenomes, List<Queue<float>> parentChromosomes, List<Queue<float>> childChromosomes)
	{
		float[] motherChromosome;
		float[] fatherChromosome;
		List<float> fatherSegment;
		List<float> motherSegment;
		//We want to increase the counter by two, to allow for each parent
		for (int i = 0; i < NumberOfFit; i += 2)
		{
			//check both parents are referenced, just in case we've gone off the end of the list
			if (fittestGenomes[i + 1] == null || fittestGenomes[i] == null) continue;

			//Get the chromosomes of the 1st and 2nd point of the array, the next two fittest individuals
			motherChromosome = fittestGenomes[i].ExtractChromosome().ToArray();
			fatherChromosome = fittestGenomes[i + 1].ExtractChromosome().ToArray();

			//Create splices from the mother and father chromosome
			SpliceGenes(motherChromosome, fatherChromosome, out fatherSegment,
				out motherSegment);

			//Combine and mutate the spliced genes
			MutateGenes(numberOfGenes, parentChromosomes, childChromosomes, fatherSegment, motherSegment, fatherChromosome,
				motherChromosome, true);
		}
	}

	private void SpliceGenes( float[] motherChromosome,
		float[] fatherChromosome, out List<float> fatherSegment, out List<float> motherSegment)
	{
		//Get a random point to splice the genes
		int slicePoint = Random.Range(1, numberOfGenes - 1);

		
		//Segment the array based on the splice point
		//Take the father's genes up to the splice point
		fatherSegment = new List<float>(fatherChromosome);
		fatherSegment.RemoveRange(0, slicePoint);
		//And the mother's genes after the splice point
		motherSegment = new List<float>(motherChromosome);
		motherSegment.RemoveRange(slicePoint, numberOfGenes-slicePoint);
	}

	private void MutateGenes(int chromosomeLength, List<Queue<float>> parentChromosomes, List<Queue<float>> childChromosomes,
		List<float> fatherSegment, List<float> motherSegment, float[] fatherChromosome, float[] motherChromosome, bool reAddFatherGene)
	{
		//Get the new chromosome based on the gene slices
		var newChromosome = CreateChild(chromosomeLength, fatherSegment, motherSegment);
		//Mutate all of the chromosomes individually
		MutateChromosome(chromosomeLength, ref newChromosome);
		MutateChromosome(chromosomeLength, ref fatherChromosome);
		MutateChromosome(chromosomeLength, ref motherChromosome);

		//Re-add the chromosomes to the gene pool
//		parentChromosomes.Add(new Queue<float>(motherChromosome));
//		parentChromosomes.Add(new Queue<float>(fatherChromosome));
		childChromosomes.Add(new Queue<float>(newChromosome));
	}

	//Takes two array segments and combines them into a new genome
	private static float[] CreateChild(int chromosomeLength, List<float> fatherSegment, List<float> motherSegment)
	{
		float[] newChromosome = new float[chromosomeLength];
		//Add the father segment to the start of the array, since this is where we took it from earlier
		fatherSegment.CopyTo(newChromosome, 0);
		//Add the mother's genes onto the end of the fathers
		motherSegment.CopyTo(newChromosome, fatherSegment.Count);
		return newChromosome;
	}

	private void MutateChromosome(int chromosomeLength, ref float[] newChromosome)
	{
		for (int i = 0; i < chromosomeLength; i++)
		{
			//If the random lands below our percentage rate, mutate the gene
			if (Random.Range(0, 100) < mutationPercentage)
			{
				//Mutation involves randomly applying a value to the gene
				newChromosome[i] = Random.Range(-1f, 1f);
			}
		}
	}

	private List<NeuralNet> SelectLeastFitIndividuals(List<NeuralNet> fittestGenomes)
	{
		List<NeuralNet> leastFitGenomes = new List<NeuralNet>(CurrentGenomeGeneration);
		//Remove all the fit genomes from the list
		foreach (var fittestGenome in fittestGenomes)
		{
			leastFitGenomes.Remove(fittestGenome);
		}
		//what we're left with is the least fit
		return leastFitGenomes;
	}


	private List<NeuralNet> SelectFittestIndividuals()
	{
		List<NeuralNet> fittestGenomes = new List<NeuralNet>();
		//Order the list from least fit to most
		List<NeuralNet> sortedByFittest = CurrentGenomeGeneration.OrderBy(o=>o.fitness).ToList();
		sortedByFittest.Reverse();
		//Add the top X to the fittest
		for (int i = 0; i < NumberOfFit; i ++)
		{
			fittestGenomes.Add(sortedByFittest[i]);
		}
		return fittestGenomes;
	}
	
	public GameObject GetFittestCar()
	{
		//Set a baseline
		NeuralNet fittest = CurrentGenomeGeneration[0];
		//Check all for greatest fitness value
		foreach (var neuralNet in CurrentGenomeGeneration)
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
		Queue<float> randomChromosome = new Queue<float>();
		//Randomly add genes for the first generation
		foreach (var genome in CurrentGenomeGeneration)
		{
			for (int i = 0; i < numberOfGenes; i++)
			{
				randomChromosome.Enqueue(Random.Range(-1f, 1f));
			}
			genome.ApplyChromosome(randomChromosome);
		}
	}
}
