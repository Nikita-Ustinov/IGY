using System;
using System.Collections.Generic;
//using System.IO;

namespace BP_pokus_3
{
	/// <summary>
	/// To discripe convoluce and list of inputs with their average value.
	/// </summary>
	
	[Serializable]
	public class Convolution
	{
		public double[,] weights;
		public double [,] avInput;					// pro pocitani stredni hodnoty vstupu kazde vahy
		public int cisloFiltra;
		public int size;
		public double grad;
		public double averageOutput;
		public int[,] inputSize;
		public int outputSize = 0;
//		public LinkedList<double> allOutputs = new LinkedList<double>();
		private static LinkedList<Convolution> Filters = new LinkedList<Convolution>();
		
		[Serializable]
		public struct AverageInput {
			public double average;
			public LinkedList<Double> inputList;
		}
		
		public void addInput(double input, int y, int x) {
			inputSize[y,x]++;
			avInput[y,x] +=input;
		}
		
		public void addOutput(double output) {
			outputSize++;
			averageOutput += output;
		}
		
		public void clearInputMass() {
			avInput = new Double[size,size];
			inputSize = new int[size,size];
		}
		
		public void clearOutput() {
			averageOutput = 0;
			outputSize = 0;
		}
		
		public Convolution(int size, int cisloFiltra)
		{
			this.cisloFiltra = cisloFiltra;
			if(cisloFiltra == 0) {
				weights = doFirstRandomWeights(new double[size,size]);
			}
			else {
				weights = doRandomWeights(new double[size,size]);
			}
			avInput = new double[size,size];
			inputSize = new int[size,size];
			this.size = size;
//			for (int i=0; i<size; i++) {
//				for (int j=0; j<size; j++) {
//					avInput[i,j].inputList = new LinkedList<double>();
//				}
//			}
			Filters.AddLast(this);
		}
		
		double[,] doFirstRandomWeights(double[,] newFilter) {
			Random rand = new Random();
			
			for(int i=0; i<newFilter.GetUpperBound(0)+1; i++) {
				for(int j=0; j<newFilter.GetUpperBound(1)+1; j++) {
					do{
						newFilter[i,j] = rand.Next(-6,5)*0.1 + 0.1;
					} while (newFilter[i,j] == 0 );
				}
			}
			return newFilter;
		}
		
		double[,] doRandomWeights(double[,] newFilter) {
			Random rand = new Random();
			LinkedListNode<Convolution> templ = Filters.First;
			while(templ.Value.cisloFiltra != cisloFiltra-1) {
				templ = templ.Next;
			}
			for(int i=0; i<newFilter.GetUpperBound(0)+1; i++) {
				for(int j=0; j<newFilter.GetUpperBound(1)+1; j++) {
					if((cisloFiltra != 5)||(cisloFiltra != 10)){
						do{
							newFilter[i,j] = rand.Next(-6,5)*0.1 + 0.1;
						} while (newFilter[i,j] == templ.Value.weights[i,j]);
					}
					else {
						do{
							newFilter[i,j] = rand.Next(-6,5)*0.1 + 0.1;
						} while (newFilter[i,j] == 0);
					}
				}
			}
			return newFilter;
		}
		
		public void countAverageOutput() {
//			LinkedListNode<double> templ = allOutputs.First;
//			for(int i=0; i<allOutputs.Count; i++) {
//				averageOutput += templ.Value;
//				templ = templ.Next;
//			}
//			averageOutput = averageOutput/allOutputs.Count;
			averageOutput =   averageOutput / outputSize ;
		}
		
		public void countAverageInput() {
			for (int i=0; i<size; i++) {
				for (int j=0; j<size; j++) {
//					LinkedListNode<Double> templ = avInput[i,j].inputList.First;
//					for(int k=0; k<avInput[i,j].inputList.Count; k++) {
//						avInput[i,j].average += templ.Value;
//						templ = templ.Next;
//					}
					
//					avInput[i,j].average = avInput[i,j].average/avInput[i,j].inputList.Count;
					avInput[i,j] = avInput[i,j]/inputSize[i,j];
				}
			}
		}
	}
}
