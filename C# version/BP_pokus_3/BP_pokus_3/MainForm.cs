using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Diagnostics;

namespace BP_pokus_3
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	
	partial class MainForm : Form
	{
		static String progressInfo;
		static String _info;
		static String _convolutionsInfo;
		int Answer;
		bool [] UzByli = new bool[] {false, false, false, false, false, false , false, false, false, false};				
		Neuronet net ;
		double Max;
		public int counterAddNeuron=0;
		public int iterationWithoutNewNeuron=0;
		bool newNeuron = false;
		static int countException = 0;
		public  void newEpoch() {
			for (int i=0; i<10; i++) {
				UzByli[i] = false;
			}
		}
		
		public MainForm()
		{
			InitializeComponent();
		}
		
		public MainForm(int i) {}
		
//		public Picture getPicture() {
//			Random rand = new Random();
//			int a;
//			do {
//				a = rand.Next(0, 10);
//			} while (UzByli[a] == true);
//			String fileName = a.ToString();
//			UzByli[a] = true;
//			Answer = a;
//			return ImgToRightPicture(fileName); 				
//		}
		
		public Picture getPicture(bool ifColor) {
			Random rand = new Random();
			int a;
			do {
				a = rand.Next(0, 10);
			} while (UzByli[a] == true);
			String fileName = a.ToString();
			UzByli[a] = true;
			Answer = a;
			return ImgToRightPicture(fileName, ifColor); 							
		}
		
//		double[,] ImgToRightPicture(String file) {
//			file += ".jpeg";
//			Image img = Image.FromFile(file);
//			Bitmap bm = new Bitmap(img);
//			Color color =new Color();
//			double[,] vysledek = new double[39,30];
//			for (int i=0;i<39; i++) {
//				for (int j=0; j<30; j++) {
//					color = bm.GetPixel(j,i);
//					vysledek[i,j] = color.GetBrightness();
//				}
//			}
//			return vysledek;
//		}
		
		Picture ImgToRightPicture(String file, bool isColor) {
			Color color =new Color();
			if(isColor) {
				file += ".jpg";
				Image img = Image.FromFile(file);
				Bitmap bm = new Bitmap(img);
				Picture vysledek = new Picture(39,30,3);
				for (int i=0;i<39; i++) {
					for (int j=0; j<30; j++) {
						color = bm.GetPixel(j,i);
						for(int k=0; k<3; k++) {
							if (k==0)
								vysledek.map3D[i,j,k] = normalization(color.R);
							if (k==1)
								vysledek.map3D[i,j,k] = normalization(color.G);
							if (k==2)
								vysledek.map3D[i,j,k] = normalization(color.B);
						}
					}
				}
				return vysledek;
			}
			else {
				file += ".jpeg";
				Image img = Image.FromFile(file);
				Bitmap bm = new Bitmap(img);
				Picture vysledek = new Picture(39,30);
				for (int i=0;i<39; i++) {
					for (int j=0; j<30; j++) {
						color = bm.GetPixel(j,i);
						vysledek.map2D[i,j] = color.GetBrightness();
					}
				}
				return vysledek;
			}
		}
		
		double normalization(byte color) {
			double vysledek = ((double)color)/256;
			return vysledek;
		}
		
		
		int calculateResult(Picture picture) {
			Picture[] firstConvolution = new Picture[net.convolutions.Count/3];			//prvni vrstva, convolution 11x11
			for(int i=0; i<net.convolutions.Count/3; i++) {
				firstConvolution[i] = new Picture(applyConvolution(i, picture));							//prvni konvoluce
				firstConvolution[i].map2D = function(firstConvolution[i].map2D, "Tanh");				// prvni funkce aktivace (Tanh)
				firstConvolution[i].map2D = addX(firstConvolution[i]);				
				firstConvolution[i].map2D = pooling(2, firstConvolution[i].map2D);						//prvni pooling
				
			}
			Picture[] secondConvolution = new Picture[(int)Math.Pow(net.convolutions.Count/3, 2)];
			int  cisloFiltra = 5;
			int  cisloPolozky = 0;
			for(int j=0; j<net.convolutions.Count/3; j++) {
				for(int i=0; i<net.convolutions.Count/3; i++) {
					secondConvolution[cisloPolozky] = new Picture(applyConvolution(cisloFiltra, firstConvolution[j]));
					secondConvolution[cisloPolozky].map2D = function(secondConvolution[cisloPolozky].map2D, "Tanh");
					secondConvolution[cisloPolozky].map2D = pooling(2, secondConvolution[cisloPolozky].map2D);
					cisloFiltra++;
					cisloPolozky++;
				}
				cisloFiltra = net.convolutions.Count/3;
			}
			double[][,] thirdConvolution = new double[(int)Math.Pow(net.convolutions.Count/3, 3)][,];
			cisloFiltra = net.convolutions.Count/3*2;
			cisloPolozky = 0;
			for(int j=0; j<Math.Pow(net.convolutions.Count/3, 2); j++) {
				for(int i=0; i<net.convolutions.Count/3; i++) {
					thirdConvolution[cisloPolozky] = applyConvolution(cisloFiltra, secondConvolution[j]);
					thirdConvolution[cisloPolozky] = addY(thirdConvolution[cisloPolozky]);
					thirdConvolution[cisloPolozky] = function(thirdConvolution[cisloPolozky], "Tanh");
					thirdConvolution[cisloPolozky] = pooling(3, thirdConvolution[cisloPolozky]);
					cisloFiltra++;
					cisloPolozky++;
				}
				cisloFiltra = net.convolutions.Count/3*2;
			}
			double[] inputFullyConnectionNet = doOneArray(thirdConvolution);
			net.l0.writeInput(inputFullyConnectionNet);									//zapisuje vstupni vektor v "fully connected" neuronovou sit
			net.l0.countOutputs();
			Neuron templ1 = net.l1.head;
			for (int i=0; i<Neuronet.druhaVrstva; i++){									//zapisuje do druhe vrstvy FC neuronove siti vstupni signaly
				for (int j=0; j<Neuronet.prvniVrstva; j++){
					templ1.input[j]=net.l0.outputs[j];
				}
				templ1=templ1.next;
			}
			net.l1.countOutputs();
			templ1=net.l2.head;
			for (int i=0; i<Neuronet.tretiVrstva; i++){									// zapisuje do treti vrstvy FC neuronove siti vstupni signaly
				for (int j=0; j<Neuronet.druhaVrstva; j++){
					templ1.input[j]=net.l1.outputs[j];
				}
				templ1=templ1.next;
			}
			net.l2.countOutputs();
			int index = 0;																//cislo neuronu ktery vyhral
			Max = net.l2.outputs[0];
			for (int i=0; i<net.l2.outputs.Length; i++) {
				if (Max<net.l2.outputs[i])
				{
					Max = net.l2.outputs[i];
					index = i;
				}
			}
//			LinkedListNode<Convolution> templ = net.convolutions.First;
//			for (int i=0; i<net.convolutions.Count; i++) {
//				templ.Value.clearInputMass();
//				templ.Value.clearOutput();
//				templ = templ.Next;
//			}
			
			return index;
		}
		
		double[,] function(double[,] picture, String nazevFunkce) {
			double[,] result = picture;
			for(int i=0; i<picture.GetUpperBound(0)+1; i++) {
				for(int j=0; j<picture.GetUpperBound(1)+1; j++) {
					if(nazevFunkce == "Tanh") {
						result[i,j] = 1.7159*Math.Tanh(0.66*picture[i,j]);
					}
					else if(nazevFunkce == "ReLu") {
					   	if(result[i,j] > 0){
							result[i,j] = result[i,j];
						}
						else {
							result[i,j] = 0;
						}
					}
					
				}
			}
			return result;
		}
		
		void study() {
			Stopwatch timer = new Stopwatch();
			timer.Start();
			double [] err = new double[Neuronet.tretiVrstva];
			int iteration = 1;
			double lokError = 0;
			double lokResult = 0;
			double errorMin = 100;
			int testValue = 0;
			int bestTestValue = 0;
			int gradNull = 0; 
			
			
			while(testValue < 100) {
				lokResult = calculateResult(getPicture(true));						//getPicture if color picture -true, black and white - false
				Neuron templ3 = net.l2.head;
				for  (int i=0; i<Neuronet.tretiVrstva; i++) {
					if (Answer == i)
						err[i] = Max - templ3.output ;									//zapisuje signal chyby vystupni vrstvy
					else
						err[i] = 0 - templ3.output;
					templ3 = templ3.next;
				}
				Neuron templ2;
				templ3 = net.l2.head;
				for (int i=0; i<Neuronet.tretiVrstva; i++) {
					templ2 = net.l1.head;
					templ3.grad = 0.388*(1.7159 - templ3.output)*(1.7159 + templ3.output)*err[i];				//pocita gradient pro vystupni vyrstvu
					for (int j=0; j<Neuronet.druhaVrstva; j++) {
						templ3.weights[j]+=net.speedLFCN*templ2.output*templ3.grad;								//pocita vahy pro vystupni vrstvu
						templ2 = templ2.next;
					}
					templ3 = templ3.next;
				}
				
				double grad = 0;
				Neuron templ1;
				templ2 = net.l1.head;
				for (int i=0; i<Neuronet.druhaVrstva; i++) {
					grad = 0;
					templ3 = net.l2.head;
					for(int u=0; u<Neuronet.tretiVrstva; u++) {						//sumarizuje gradient predhozi vrstvy (delta pravidlo pro druhou vrstvu)
						grad+=templ3.grad*templ3.weights[i];
						templ3 = templ3.next;
					}
					templ2.grad = grad*0.388*(1.7159-templ2.output)*(1.7159+ templ2.output);
					templ1 = net.l0.head;
					for (int j=0; j<Neuronet.prvniVrstva; j++) {
						templ2.weights[j]+=net.speedLFCN*templ1.output*templ2.grad;
						templ1 = templ1.next;
					}
					templ2 = templ2.next;
				}
				
				templ1 = net.l0.head;
				for (int i=0; i<Neuronet.prvniVrstva; i++) {
					grad = 0;
					templ2 = net.l1.head;
					for(int u=0; u<Neuronet.druhaVrstva; u++) {							//sumarizuje gradient predhozi vrstvy (delta pravidlo pro prvni vrstvu)
						grad+=templ2.grad*templ2.weights[i];
						templ2 = templ2.next;
					}
					templ1.grad = grad*0.388*(1.7159-templ1.output)*(1.7159+ templ1.output);										
					for (int j=0; j<Neuronet.inputLength; j++) {
						templ1.weights[j]+=net.speedLFCN*templ1.input[j]*grad;
					}
					templ1 = templ1.next;
				}
				
				LinkedListNode<Convolution> templ = net.convolutions.First;
				for (int i=0; i<net.convolutions.Count; i++) {
					templ.Value.countAverageInput();
//					templ.Value.countAverageInput3D();
					templ.Value.countAverageOutput();
					templ = templ.Next;
				}
				
				//pro filtry 10 az 14
				templ = net.convolutions.First;
				while(templ.Value.cisloFiltra!=net.convolutions.Count/3*2) {
					templ = templ.Next;
				}
				for(int i=0; i<net.convolutions.Count/3; i++) {
					grad = 0;
					templ1 = net.l0.head;												//vrstva se ktere scita gradienty
					for(int j=0; j<net.l0.length; j++) {
						grad+=templ1.grad;												//sumarizuje gradient predhozi vrstvy
						templ1 = templ1.next;
					}
					if (iteration==7) {
						
					}
					templ.Value.grad = grad*0.388*(1.7159 - templ.Value.averageOutput)*(1.7159 + templ.Value.averageOutput)/net.l0.length;
					if (grad == 0) {
						gradNull++;
					}
					for(int k=0; k<templ.Value.weights2D.GetUpperBound(0)+1; k++) {
						for(int q=0; q<templ.Value.weights2D.GetUpperBound(1)+1; q++) {
							templ.Value.weights2D[k,q] += net.speedL1CL*templ.Value.grad*templ.Value.avInput[k,q];
						}
					}
					templ = templ.Next;
				}
				
				//pro filtry 5 az 9
				templ = net.convolutions.First;
				while(templ.Value.cisloFiltra!=net.convolutions.Count/3) {
					templ = templ.Next;
				}
				for(int i=0; i<net.convolutions.Count/3; i++) {
					grad = 0;
					LinkedListNode<Convolution> templLast = net.convolutions.First;					//vrstva se ktere scita gradienty
					while(templLast.Value.cisloFiltra != net.convolutions.Count/3*2) {
						templLast = templLast.Next;
					}
					for(int j=0; j<net.convolutions.Count/3; j++) {
						grad += templLast.Value.grad;												//sumarizuje gradient predhozi vrstvy
						templLast = templLast.Next;
					}
					templ.Value.grad = grad*0.388*(1.7159 - templ.Value.averageOutput)*(1.7159 + templ.Value.averageOutput)/Neuronet.prvniVrstva;
					if (grad == 0) {
						gradNull++;
					}
					for(int k=0; k<templ.Value.weights2D.GetUpperBound(0)+1; k++) {
						for(int q=0; q<templ.Value.weights2D.GetUpperBound(1)+1; q++) {
							templ.Value.weights2D[k,q] += net.speedL2CL*templ.Value.grad*templ.Value.avInput[k,q];
						}
					}
					templ = templ.Next;
				}	
				
				//pro filtry 0 az 4
				templ = net.convolutions.First;
				for(int i=0; i<net.convolutions.Count/3; i++) {
					grad = 0;
					LinkedListNode<Convolution> templLast = net.convolutions.First;					//vrstva se ktere scita gradienty
					while(templLast.Value.cisloFiltra != net.convolutions.Count/3) {
						templLast = templLast.Next;
					}
					for(int j=0; j<net.convolutions.Count/3; j++) {
						grad += templLast.Value.grad;												//sumarizuje gradient predhozi vrstvy
						templLast = templLast.Next;
					}
					templ.Value.grad = grad*0.388*(1.7159 - templ.Value.averageOutput)*(1.7159 + templ.Value.averageOutput)/Neuronet.druhaVrstva;
					if (grad == 0) {
						gradNull++;
					}
					if(templ.Value.weights2D == null) {
						for(int k=0; k<templ.Value.weights3D.GetUpperBound(0)+1; k++) {
							for(int q=0; q<templ.Value.weights3D.GetUpperBound(1)+1; q++) {
								for(int z=0; z<templ.Value.weights3D.GetUpperBound(2)+1; z++) {
									templ.Value.weights3D[k,q,z] += net.speedL3CL*templ.Value.grad*templ.Value.avInput3D[k,q,z];
								}
							}
						}
						templ = templ.Next;
					}
					else {
						MessageBox.Show("Unexpected 2-d convolution in a study method ");
					}
				}
				
				templ = net.convolutions.First;
				for (int i=0; i<net.convolutions.Count; i++) {
					templ.Value.clearInputMass();
					templ.Value.clearInputMass3D();
					templ.Value.clearOutput();
					templ = templ.Next;
				}
				writeAllConvolution(iteration);
				
				if (Answer != lokResult) {
					lokError++;
				}
				if ((lokError/iteration*100)<errorMin) {
					errorMin = lokError/iteration*100;
				}
				if (iteration % 10 == 0) {
					testValue = test();
					if (testValue > bestTestValue) {
						bestTestValue = testValue;
						serializace("BestValue");
					}
					writeProgressInfo(iteration, errorMin, testValue, timer, gradNull);
					gradNull = 0;
					newEpoch(); 			
				}
				if(iteration % 100000 == 0) {
					_convolutionsInfo=null;
				}
				if(iteration % 1000000 == 0) {
					progressInfo = null;
				}
				iteration++;
			}
			serializace("normal");
		}
		
		double[,] pooling(int size, double[,] picture) {             			// size treba 2x2 => size=2
			double[,] result = new double[(picture.GetUpperBound(0)+1)/size,(picture.GetUpperBound(1)+1)/size];
			int x0, x1, y0, y1;
			y0 = 0;
			y1 = size;
			for(int i=0; i<result.GetUpperBound(0)+1; i++) {
				x0 = 0;
				x1 = size;
				for(int j=0; j<result.GetUpperBound(1)+1; j++) {
					result[i,j] = max(picture, x0, x1, y0, y1);
					x0+=size;
					x1+=size;
				}
				x0 = 0;
				x1 = size;
				y0+=size;
				y1+=size;
			}
			return result;
		}
		
		double max(double[,] picture, int x0, int x1, int y0, int y1) {
			double result = picture[y0,x0];
			for(int i=y0; i<y1; i++) {
				for(int j=x0; j< x1; j++) {
					if(picture[i,j]>result)
						result = picture[i,j];
				}
			}
			return result;
		}
		
		double[,] applyConvolution(int cisloFiltra, Picture picture){
			LinkedListNode<Convolution> templ = net.convolutions.First;
			while(cisloFiltra != 0 ) {
				templ = templ.Next;
				cisloFiltra--;
			}
			int x;
			int y;
			if (picture.map3D == null) {
				x = picture.map2D.GetUpperBound(1) - templ.Value.weights2D.GetUpperBound(1)+2;		// rozmer vysledne matici - x a y
				y = picture.map2D.GetUpperBound(0) - templ.Value.weights2D.GetUpperBound(0)+2;
			}
			else {
				x = picture.map3D.GetUpperBound(1) - templ.Value.weights3D.GetUpperBound(0)+2;		// rozmer vysledne matici - x a y
				y = picture.map3D.GetUpperBound(0) - templ.Value.weights3D.GetUpperBound(1)+2;
			}
			double[,] result = new double[y,x];
			int x0, y0;
			for(int i=0; i<y; i++) {
				x0 = 0;
				y0 = 0;
				for(int j=0; j<x; j++) {
					result[i,j] = sum(picture,templ.Value, x0, y0);
					x0++;
				}
				y0++;
			}
			return result;
		}

		double sum(Picture picture, Convolution templ , int x0, int y0) {
			double result = 0;
			int y = 0;			// kountery pro konvoluce
			int x = 0;
			if(picture.map3D == null) {
				for(int i=y0; i<y0+templ.weights2D.GetUpperBound(0)+1; i++) {
					for(int j=x0; j<x0+templ.weights2D.GetUpperBound(1)+1; j++) {
						if(j==picture.map2D.GetUpperBound(1)+1) {
							result += picture.map2D[i,j-1]*templ.weights2D[y,x];
							templ.addInput(picture.map2D[i,j - 1], y, x); 
						}
						else {
							result += picture.map2D[i,j]*templ.weights2D[y,x];
							templ.addInput(picture.map2D[i,j], y, x);
						}
						x++;
					}
					x=0;
					y++;
				}
				templ.addOutput(result);
				return result;
			}
			else {
				for(int i=y0; i<y0+templ.size2; i++) {
					for(int j=x0; j<x0+templ.size1; j++) {
						for(int k=0; k<3; k++) {
//							try{
//								result += picture.map3D[i,j,k]*templ.weights3D[y,x,k];
//							}
//							catch(Exception e) {
//								countException ++;
//							}
							if(j==picture.map3D.GetUpperBound(1)+1) {
								result += picture.map3D[i,j-1,k]*templ.weights3D[y,x,k];
								templ.addInput(picture.map3D[i,j - 1, k], y, x, k); 
							}
							else {
								result += picture.map3D[i,j,k]*templ.weights3D[y,x,k];
								templ.addInput(picture.map3D[i,j,k], y, x, k);
							}
//							x++;
//							if(j==picture.size2) {
//								result += picture.map2D[i,j-1]*templ.weights2D[y,x];
//								templ.addInput(picture.map2D[i,j - 1], y, x); 
//							}
//							else {
//								result += picture.map2D[i,j]*templ.weights2D[y,x];
//								templ.addInput(picture.map2D[i,j], y, x);
//							}
//							x++;
						}
						x++;
					}
					x=0;
					y++;
				}
				templ.addOutput(result);
				return result;
			}
		}
			
		double[] doOneArray(double[][,] thirdConvolution) {
		int length = thirdConvolution.Length*thirdConvolution[10].Length;
		double[] result = new double[length];
		int counter = 0;
			for (int i = 0; i<thirdConvolution.Length; i++) {
				for(int j=0; j<thirdConvolution[i].GetUpperBound(0)+1; j++) {
					for(int k=0; k< thirdConvolution[i].GetUpperBound(1)+1; k++) {
						result[counter] = thirdConvolution[i][j,k];
						counter++;
					}
				}
			}
			return result;
		}
		
//		public static void writeInfo(double[,] picture, String typeOfTransformation)	//zobrazuje zmeny primo v konvolucich
//		{
//			_info +=typeOfTransformation+" -> "+"\r\n"+"x- "+ (picture.GetUpperBound(1)+1).ToString() +"\r\n"+ "y -"+(picture.GetUpperBound(0)+1).ToString()+"\r\n";
//			File.WriteAllText("Date.txt", _info);
//			writeConvolution(picture, typeOfTransformation);
//		}

		static void writeProgressInfo(int iteration, double errorMin, int testValue, Stopwatch timer, int gradNull) {
			progressInfo += "epoch = "+iteration/10+" error min. = "+errorMin+" test= "+testValue+"     h:"+timer.Elapsed.Hours.ToString()+" m:"+timer.Elapsed.Minutes.ToString()+" s:"+timer.Elapsed.Seconds.ToString() +" counter null grad-"+gradNull+"\r\n";                                      			
			File.WriteAllText("Progress info.txt", progressInfo);
		}
		
		static void writeProgressInfo(int iteration, double testValue) {
			progressInfo += "epoch = "+iteration/10+" test value = "+testValue+"\r\n";
			File.WriteAllText("Short progress info.txt", progressInfo);
		}

		void writeAllConvolution(int iteration) {
			LinkedListNode<Convolution> templ = net.convolutions.First;
			for(int i=0; i<net.convolutions.Count; i++) {
				writeConvolution(templ.Value, "Conv. № "+i+" iteration: "+ iteration);
				templ = templ.Next;
			}
		}
		
		static void writeConvolution(Convolution picture, string typeOfTransformation) {	
			_convolutionsInfo += typeOfTransformation+"\r\n";
			if(picture.weights3D==null) {
				for (int i=0; i<picture.weights2D.GetUpperBound(0)+1; i++) {
					for (int j=0; j<picture.weights2D.GetUpperBound(1)+1; j++) {
						_convolutionsInfo += picture.weights2D[i,j].ToString()+"  ";
					}
					_convolutionsInfo += "\r\n";
				}
				_convolutionsInfo += "\r\n"+"\r\n";
				File.WriteAllText("Convolutions.txt", _convolutionsInfo);
			}
			else {
				for (int i=0; i<picture.weights3D.GetUpperBound(0)+1; i++) {
					for (int j=0; j<picture.weights3D.GetUpperBound(1)+1; j++) {
						_convolutionsInfo += "[";
						for (int k=0; k<picture.weights3D.GetUpperBound(2)+1; k++) {
							_convolutionsInfo += picture.weights3D[i,j,k].ToString()+",";
						}
						_convolutionsInfo += "] ";
					}
					_convolutionsInfo += "\r\n";
				}
				_convolutionsInfo += "\r\n"+"\r\n";
				File.WriteAllText("Convolutions.txt", _convolutionsInfo);
			}
		}
		
		public	int test() {
			int vysledek=0;
			for (int j=0; j<1; j++) {
				newEpoch();
				for (int i=0; i<10; i++) {
					int vysOperace = calculateResult(getPicture(true));
					if (Answer == vysOperace) {
						vysledek += 1;
					}
					else
						vysledek += 0;
				}
			}
			return vysledek*10;
		}

		void serializace(String wayOfSaving) {
			BinaryFormatter formatter = new BinaryFormatter();
			if (wayOfSaving == "normal") {
				using ( var fSream = new FileStream("weights.dat", FileMode.Create, FileAccess.Write, FileShare.None)) {
				formatter.Serialize(fSream, net);
				}
			}
			else {
				using ( var fSream = new FileStream("BestWeights.dat", FileMode.Create, FileAccess.Write, FileShare.None)) {
				formatter.Serialize(fSream, net);
				}
			}
			
		}

		Neuronet deseralizace(String way) {
			try {
				using (var fStream = File.OpenRead(way+".dat")) {
					BinaryFormatter formatter = new BinaryFormatter();
					return  (Neuronet)formatter.Deserialize(fStream);
				}
			} catch (Exception e) {
				return new Neuronet();
			}
		}
		
		double[,] addX(Picture picture) {								//pridani sloupce '0' k polu
			if(!picture.isColor) {
				double[,] result = new double[picture.map2D.GetUpperBound(0)+1, picture.map2D.GetUpperBound(1)+2 ];
				for(int i=0; i<picture.map2D.GetUpperBound(0)+1; i++) {
					for(int j=0; j<picture.map2D.GetUpperBound(1)+2; j++) {
						if(j==picture.map2D.GetUpperBound(1)+1) {
							result[i,j] = -100;
						}
						else {
							result[i,j] = picture.map2D[i,j];
						}
					}
				}
				return result;
			}
			else {
				MessageBox.Show("В addX зашел 3d picture");
				return null;
			}
		}

		double[,] addY(double[,] picture) {									//pridani radka '0' k polu
			double[,] result = new double[picture.GetUpperBound(0)+2, picture.GetUpperBound(1)+1 ];
			for(int i=0; i<picture.GetUpperBound(0)+2; i++) {
				for(int j=0; j<picture.GetUpperBound(1)+1; j++) {
					if(i==picture.GetUpperBound(0)+1) {
						result[i,j] = -100;
					}
					else {
						result[i,j] = picture[i,j];
					}
				}
			}
			return result;
		}
		
		void Button1Click(object sender, EventArgs e)
		{
//			net = deseralizace("weights");
//			Neuronet.inputLength = net.inputLengthOwn;
//			Neuronet.prvniVrstva = net.prvniVrstvaOwn;
//			Neuronet.druhaVrstva = net.druhaVrstvaOwn;
//			Neuronet.tretiVrstva = net.tretiVrstvaOwn;
//			writeProgressInfo(0,test());					//vysledek ve fajlu "short progress info"
			net = new Neuronet();
			study();
			button1.BackColor = Color.Green;
		}
	}
}
