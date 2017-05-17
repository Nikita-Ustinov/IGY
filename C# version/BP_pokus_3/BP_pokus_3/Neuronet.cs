using System;
using System.Collections.Generic;
//using System.IO;

namespace BP_pokus_3
{	/// <summary> /// Description of Neuronet. /// </summary>
	[Serializable]
	public class Neuronet
	{	
		public List l0, l1, l2;
		public static int inputLength = 250;		//pro fully connection neuronet
		public static int prvniVrstva = 70;		
		public static int druhaVrstva = 60;		
		public static int tretiVrstva = 10;
		public double speedLFCN = 0.01;				//rychlost uceni pro "fully connection neuronet"
		public double speedL1CL = 0.05;				//rychlost uceni pro prvni "convolution layer" 10 az 14
		public double speedL2CL = 0.05;				//rychlost uceni pro druhy "convolution layer"  5 az 9
		public double speedL3CL = 0.05;				//rychlost uceni pro treti "convolution layer"  0 az 4
		
		public int inputLengthOwn;
		public int prvniVrstvaOwn;
		public int druhaVrstvaOwn;
		public int tretiVrstvaOwn;
		
		public LinkedList<Convolution> convolutions = new LinkedList<Convolution>();
		
		public Neuronet()
		{
			addFilter(11);					//pridani konvoluci 11x11  
			addFilter(11);					//pridani konvoluci 11x11  
			addFilter(11);					//pridani konvoluci 11x11  
			addFilter(11);					//pridani konvoluci 11x11  
			addFilter(11);					//pridani konvoluci 11x11 
			
			addFilter(5);					//pridani konvoluci 5x5   
			addFilter(5);					//pridani konvoluci 5x5
			addFilter(5);					//pridani konvoluci 5x5
			addFilter(5);					//pridani konvoluci 5x5
			addFilter(5);					//pridani konvoluci 5x5
			
			addFilter(3);					//pridani konvoluci 3x3  
			addFilter(3);					//pridani konvoluci 3x3  
			addFilter(3);					//pridani konvoluci 3x3  
			addFilter(3);					//pridani konvoluci 3x3  
			addFilter(3);					//pridani konvoluci 3x3  
			
			l0= new List(0);				//create first fully connected layer  - prvni vrstva
			l1= new List(1);				//create second fully connected layer - druha vrstva
			l2= new List(2);				//create output fully connected layer - vystupni vrstva
			
			inputLengthOwn = inputLength;
			prvniVrstvaOwn = prvniVrstva;
			druhaVrstvaOwn = druhaVrstva;
			tretiVrstvaOwn = tretiVrstva;
		}
		
		void addFilter(int size) {
			int cisloFiltra = 0;
			if(convolutions.First==null) {
				convolutions.AddLast(new Convolution(size,cisloFiltra));
			} 
			else {
				LinkedListNode<Convolution> templ = convolutions.First;
				while(templ!=null) {
					templ = templ.Next;
					cisloFiltra++;
				}
				convolutions.AddLast(new Convolution(size, cisloFiltra));
			}
				
		}
	}
}
