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
	
	[Range(0, 10)]
	[SerializeField] private float mutationPercentage;
	[Range(0, 30)]
	[SerializeField] private float fitPopulationPercentage = 10;
	[Range(0, 50)]
	[SerializeField] private float percentageOfUnfitToReplace = 20;
	[SerializeField] private bool geneSetMutation;

	[SerializeField] private bool spliceGenesUsingSegments = false;
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

	private int NumberOfGenes
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
	private int FitCount
	{
		get
		{ 
			int value = (int)(CurrentGenomeGeneration.Count * (fitPopulationPercentage/100));
			return value;
		}
	}
	
	private int UnfitCount
	{
		get
		{ 
			int value = (int)(CurrentGenomeGeneration.Count * ((100-fitPopulationPercentage-percentageOfUnfitToReplace)/100));
			return value;
		}
	}

	private int BottomOfGenePoolCount
	{
		get
		{ 
			int value = (int)(CurrentGenomeGeneration.Count * (percentageOfUnfitToReplace/100));
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
		List<NeuralNet> bottomOfGenePoolGenomes = SelectBottomOfGenePool();
		List<NeuralNet> unfitGenomes = SelectUnfitIndividuals(fittestGenomes, bottomOfGenePoolGenomes);
		List<Queue<float>> childChromosomes = new List<Queue<float>>();
		List<Queue<float>> newGeneration = new List<Queue<float>>();
		//Cut the fit genes up and cross them over creating new children
		SpliceUnfitWithFittest(fittestGenomes, unfitGenomes, childChromosomes);
		SpliceFittestWithFittest(fittestGenomes, childChromosomes);

		newGeneration.Add(fittestGenomes[0].ExtractChromosome());
		//Re-apply the mutated parent chromosomes
		for (int i = 1; i < FitCount; i++)
		{
			var mutatedChromosome = MutateChromosome(fittestGenomes[i].ExtractChromosome());
			newGeneration.Add(mutatedChromosome);
		}
		
		//Replace the least fit individuals with the child chromosomes of the fittest genes
		for (int i = 0; i < childChromosomes.Count; i++)
		{
			var mutatedChromosome = MutateChromosome(childChromosomes[i]);
			newGeneration.Add(mutatedChromosome);
		}
		
		//Re-apply the mutated parent chromosomes
		for (int i = 0; i < newGeneration.Count-1; i++)
		{
			currentGenomeGeneration[i].ApplyChromosome(newGeneration[i]);
		}
	}
	
	private void CreateNewGenerationWithGeneSets()
	{
		SetIndividuals();
		List<NeuralNet> fittestGenomes = SelectFittestIndividuals();
		List<NeuralNet> bottomOfGenePoolGenomes = SelectBottomOfGenePool();
		List<NeuralNet> unfitGenomes = SelectUnfitIndividuals(fittestGenomes, bottomOfGenePoolGenomes);
		List<Queue<float>> childChromosomes = new List<Queue<float>>();
		List<Queue<float>> newGeneration = new List<Queue<float>>();
		//Cut the fit genes up and cross them over creating new children
		SpliceUnfitWithFittest(fittestGenomes, unfitGenomes, childChromosomes);
		SpliceFittestWithFittest(fittestGenomes, childChromosomes);

		newGeneration.Add(fittestGenomes[0].ExtractChromosome());
		//Re-apply the mutated parent chromosomes
		for (int i = 1; i < FitCount; i++)
		{
			newGeneration.Add(fittestGenomes[i].ExtractChromosome());
		}
		
		//Replace the least fit individuals with the child chromosomes of the fittest genes
		for (int i = 0; i < childChromosomes.Count; i++)
		{
			newGeneration.Add(childChromosomes[i]);
		}
		
		//Re-apply the mutated parent chromosomes
		for (int i = 0; i < newGeneration.Count-1; i++)
		{
			currentGenomeGeneration[i].ApplyChromosome(newGeneration[i]);
		}
	}

	private void SpliceUnfitWithFittest(List<NeuralNet> fittestGenomes, List<NeuralNet> leastFitGenomes, List<Queue<float>> childChromosomes)
	{
		int j = 0;
		//We want to increase the counter by two, to allow for each parent
		for (int i = 0; i < UnfitCount; i ++)
		{
			//Get the chromosomes of the 1st and 2nd point of the array, the next two fittest individuals
			var motherChromosome = fittestGenomes[j].ExtractChromosome().ToArray();
			var fatherChromosome = leastFitGenomes[i].ExtractChromosome().ToArray();

			var newChromosome = GetChild(motherChromosome, fatherChromosome);

			childChromosomes.Add(new Queue<float>(newChromosome));
			j++;
			if (j >= FitCount - 1) j = 0;
		}
	}

	private void SpliceFittestWithFittest(List<NeuralNet> fittestGenomes, List<Queue<float>> childChromosomes)
	{
		int j = 0;
		for (int i = 0; i < BottomOfGenePoolCount; i ++)
		{
			//Get the chromosomes of the 1st and 2nd point of the array, the next two fittest individuals
			var motherChromosome = fittestGenomes[j].ExtractChromosome().ToArray();
			var fatherChromosome = fittestGenomes[j+1].ExtractChromosome().ToArray();

			var newChromosome = GetChild(motherChromosome, fatherChromosome);
			childChromosomes.Add(new Queue<float>(newChromosome));
			j+=2;
			if (j >= FitCount - 1) j = 0;
		}
	}

	private float[] GetChild(float[] motherChromosome, float[] fatherChromosome)
	{
		var newChromosome = new float[NumberOfGenes];
		if (spliceGenesUsingSegments)
		{
			//Create splices from the mother and father chromosome
			List<float> fatherSegment;
			List<float> motherSegment;
			SpliceGenes(motherChromosome, fatherChromosome, out fatherSegment,
				out motherSegment);
			newChromosome = CreateChild(fatherSegment, motherSegment);
		}
		else
		{
			newChromosome = CreateUnsegmentedChild(fatherChromosome, motherChromosome);
		}

		return newChromosome;
	}

	private void SpliceGenes( float[] motherChromosome,
		float[] fatherChromosome, out List<float> fatherSegment, out List<float> motherSegment)
	{
		//Get a random point to splice the genes
		int slicePoint = Random.Range(1, NumberOfGenes - 1);

		
		//Segment the array based on the splice point
		//Take the father's genes up to the splice point
		fatherSegment = new List<float>(fatherChromosome);
		fatherSegment.RemoveRange(0, slicePoint);
		//And the mother's genes after the splice point
		motherSegment = new List<float>(motherChromosome);
		motherSegment.RemoveRange(slicePoint, NumberOfGenes-slicePoint);
	}
	
	private float[] CreateUnsegmentedChild( float[] motherChromosome, float[] fatherChromosome)
	{
		float[] newChromosome = new float[NumberOfGenes];
		for (int i = 0; i < NumberOfGenes; i++)
		{
			if (Random.Range(0, 1) == 0)
			{
				newChromosome[i] = motherChromosome[i];
			}
			else
			{
				newChromosome[i] = fatherChromosome[i];
			}
		}
		
		return newChromosome;
	}

	//Takes two array segments and combines them into a new genome

	private float[] CreateChild( List<float> fatherSegment, List<float> motherSegment)
	{
		float[] newChromosome = new float[NumberOfGenes];
		//Add the father segment to the start of the array, since this is where we took it from earlier
		fatherSegment.CopyTo(newChromosome, 0);
		//Add the mother's genes onto the end of the fathers
		motherSegment.CopyTo(newChromosome, fatherSegment.Count);
		return (float[]) DeepCopy.Copy(newChromosome);
	}

	private Queue<float> MutateChromosome(Queue<float> newChromosome)
	{
		Queue<float> mutatedChromosome = new Queue<float>();
		for (int i = 0; i < NumberOfGenes; i++)
		{
			//If the random lands below our percentage rate, mutate the gene
			if (Random.Range(0, 100) < mutationPercentage)
			{
				//Mutation involves randomly applying a value to the gene
				mutatedChromosome.Enqueue(Random.Range(-1f, 1f));
				newChromosome.Dequeue();
			}
			else
			{
				mutatedChromosome.Enqueue(newChromosome.Dequeue());
			}
		}

		return mutatedChromosome;
	}

	private List<NeuralNet> SelectBottomOfGenePool()
	{
		List<NeuralNet> bottomOfGenePool = new List<NeuralNet>();
		//Order the list from least fit to most
		List<NeuralNet> sortedByLeastFit = CurrentGenomeGeneration.OrderBy(o=>o.fitness).ToList();
		//Add the top X to the fittest
		for (int i = 0; i < BottomOfGenePoolCount; i ++)
		{
			bottomOfGenePool.Add(sortedByLeastFit[i]);
		}
		return new List<NeuralNet>(bottomOfGenePool);
	}

	private List<NeuralNet> SelectUnfitIndividuals(List<NeuralNet> fittestGenomes, List<NeuralNet> bottomOfGenePool)
	{
		List<NeuralNet> leastFitGenomes = new List<NeuralNet>(CurrentGenomeGeneration);
		//Remove all the fit genomes from the list
		foreach (var fittestGenome in fittestGenomes)
		{
			leastFitGenomes.Remove(fittestGenome);
		}
		
		foreach (var bottomGene in bottomOfGenePool)
		{
			leastFitGenomes.Remove(bottomGene);
		}
		//what we're left with is least fit
		return leastFitGenomes;
	}


	private List<NeuralNet> SelectFittestIndividuals()
	{
		List<NeuralNet> fittestGenomes = new List<NeuralNet>();
		//Order the list from least fit to most
		List<NeuralNet> sortedByFittest = CurrentGenomeGeneration.OrderBy(o=>o.fitness).ToList();
		sortedByFittest.Reverse();
		//Add the top X to the fittest
		for (int i = 0; i < FitCount; i ++)
		{
			fittestGenomes.Add(sortedByFittest[i]);
		}
		return new List<NeuralNet>(fittestGenomes);
	}
	
	public GameObject GetFittestCar()
	{
		if (currentGenomeGeneration.Count == 0) return null;
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
			for (int i = 0; i < NumberOfGenes; i++)
			{
				randomChromosome.Enqueue(Random.Range(-1f, 1f));
			}
			genome.ApplyChromosome(randomChromosome);
		}
	}

	public void SetSettings(SimulationController.MutationSettings mutationSetting)
	{
		mutationPercentage = mutationSetting.mutationPercentage;
		fitPopulationPercentage = mutationSetting.fitPopulationPercentage;
		percentageOfUnfitToReplace = mutationSetting.percentageOfUnfitToReplace;
	}
}
