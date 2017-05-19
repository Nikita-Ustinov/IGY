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
		public double[,,] weights3D;
		public double[,] weights2D;
		public double [,] avInput;					// pro pocitani stredni hodnoty vstupu kazde vahy
		public double [,,] avInput3D;					// pro pocitani stredni hodnoty vstupu kazde vahy
		public int[,,] inputSize3D;
		public int cisloFiltra;
		public int size1;
		public int size2;
		public int size3 = 0;
		public double grad;
		public double averageOutput;
		public int[,] inputSize;
		public int outputSize = 0;
		private static LinkedList<Convolution> Filters = new LinkedList<Convolution>();
		
		
		
		public Convolution(int size1, int size2, int cisloFiltra)
		{
			this.cisloFiltra = cisloFiltra;
			if(cisloFiltra == 0) {
				weights2D = doFirstRandomWeights(new double[size1,size2]);
			}
			else {
				weights2D = doRandomWeights(new double[size1,size2]);
			}
			avInput = new double[size1,size2];
			inputSize = new int[size1,size2];
			this.size1 = size1;
			this.size2 = size2;
			Filters.AddLast(this);
		}
		
		public Convolution(int size1, int size2, int size3, int cisloFiltra)
		{
			this.cisloFiltra = cisloFiltra;
			if(cisloFiltra == 0) {
				weights3D = doFirstRandomWeights(new double[size1, size2, size3]);
			}
			else {
				weights3D = doRandomWeights(new double[size1,size2,size3]);
			}
			avInput3D = new double[size1, size2, size3];
			inputSize3D = new int[size1, size2, size3];
			this.size1 = size1;
			this.size2 = size2;
			this.size3 = size3;
			Filters.AddLast(this);
		}
		
//		[Serializable]
//		public struct AverageInput {
//			public double average;
//			public LinkedList<Double> inputList;
//		}
		
		public void addInput(double input, int y, int x) {
			inputSize[y,x]++;
			avInput[y,x] += input;
		}
		
		public void addInput(double input, int y, int x, int k) {
			inputSize3D[y,x,k]++;
			avInput3D[y,x,k] += input;
		}
		
		public void addOutput(double output) {
			outputSize++;
			averageOutput += output;
		}
		
		public void clearInputMass() {
			avInput = new Double[size1,size2];
			inputSize = new int[size1,size2];
		}
		
		public void clearInputMass3D() {
			avInput3D = new Double[size1,size2,size3];
			inputSize3D = new int[size1,size2, size3];
		}
		
		public void clearOutput() {
			averageOutput = 0;
			outputSize = 0;
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
		
		double[,,] doFirstRandomWeights(double[,,] newFilter) {
			Random rand = new Random();
			for(int i=0; i<newFilter.GetUpperBound(0)+1; i++) {
				for(int j=0; j<newFilter.GetUpperBound(1)+1; j++) {
					for(int k=0; k<newFilter.GetUpperBound(2)+1; k++) {
						do{
							newFilter[i,j,k] = rand.Next(-6,5)*0.1 + 0.1;
						} while (newFilter[i,j,k] == 0 );
					}
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
					if((cisloFiltra != 5)&&(cisloFiltra != 10)){
						do{
							newFilter[i,j] = rand.Next(-6,5)*0.1 + 0.1;
						} while (newFilter[i,j] == templ.Value.weights2D[i,j]);
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
		
		double[,,] doRandomWeights(double[,,] newFilter) {
			Random rand = new Random();
			LinkedListNode<Convolution> templ = Filters.First;
			while(templ.Value.cisloFiltra != cisloFiltra-1) {
				templ = templ.Next;
			}
			for(int i=0; i<newFilter.GetUpperBound(0)+1; i++) {
				for(int j=0; j<newFilter.GetUpperBound(1)+1; j++) {
					for(int k=0; k<newFilter.GetUpperBound(2)+1; k++) {
						if((cisloFiltra != 5)||(cisloFiltra != 10)){
							do{
								newFilter[i,j,k] = rand.Next(-6,5)*0.1 + 0.1;
							} while (newFilter[i,j,k] == templ.Value.weights3D[i,j,k]);
						}
						else {
							do{
								newFilter[i,j,k] = rand.Next(-6,5)*0.1 + 0.1;
							} while (newFilter[i,j,k] == 0);
						}
					}
				}
			}
			return newFilter;
		}
		
		public void countAverageOutput() {
			averageOutput = averageOutput / outputSize ;
		}
		
		public void countAverageInput() {
			if(weights3D == null) {
				for (int i=0; i<size1; i++) {
					for (int j=0; j<size2; j++) {
						avInput[i,j] = avInput[i,j]/inputSize[i,j];
					}
				}
			}
			else 
				countAverageInput3D();
		}
		
		void countAverageInput3D() {
			for (int i=0; i<size1; i++) {
				for (int j=0; j<size2; j++) {
					for(int k=0; k<size3; k++) {
						avInput3D[i,j,k] = avInput3D[i,j,k]/inputSize3D[i,j,k];
					}
				}
			}
		}
	}
}
