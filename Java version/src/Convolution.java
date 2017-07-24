import java.io.Serializable;
import java.util.LinkedList;    
import java.util.Random;


//	[Serializable]
public class Convolution implements Serializable
{
    public Convolution next;
    public double[][] weights;
    public Double[][] avInput;                                          	// pro pocitani stredni hodnoty vstupu kazde vahy 
   
    public int[][] inputSize;                                                  //kolik bylo vstupnych hodnot. Pro pocitani stredni hodnoty vstupu
    public int outputSize = 0;                                                 //kolik bylo vystupnich hodnot. Pro pocitani stredni hodnoty vystupu
    public int cisloFiltra;
    public int size;
    public double grad;
    public double averageOutput;
//    public LinkedList<Double> allOutputs;
    private static LinkedList<Convolution> Filters;
    
    public void addInput(Double input, int y, int x) {
        inputSize[y][x]++;  
        if(avInput[y][x] == null) 
            avInput[y][x] = new Double(input);
        else
            avInput[y][x] += input;
            
    }
    
    public void addOutput(Double output) {
        outputSize++;
        averageOutput += output;
    }
    
    public void clearInputMass() {
        avInput = new Double[size][size];
        inputSize = new int[size][size];
    }
    
    public void clearOutput() {
        averageOutput = 0;
        outputSize = 0;
    }

    public Convolution(int size, int cisloFiltra)
    {   
//        allOutputs = new LinkedList<>();
        this.cisloFiltra = cisloFiltra;
        if(cisloFiltra == 0) {
            weights = doFirstRandomWeights(new double[size][size]);
        }
        else {
            weights = doRandomWeights(new double[size][size]);
        }
        inputSize = new int[size][size];
        avInput = new Double[size][size];
        this.size = size;
        if(Filters == null) {
            Filters = new LinkedList<>();
            Filters.addFirst(this);
        }
        else {
            Filters.addLast(this);
        }  
    }

    double[][] doFirstRandomWeights(double[][] newFilter) {
        Random rand = new Random();
        for(int i=0; i<newFilter.length; i++){                             //GetUpperBound(0)+1
            for(int j=0; j<newFilter[1].length; j++) {                   //GetUpperBound(1)+1
                do{
                   newFilter[i][j] = -2 + Math.random()*4;            //rand.Next(-6,5)*0.1 + 0.1;
                } while (newFilter[i][j] == 0 );
            }
        }
        return newFilter;
    }

    double[][] doRandomWeights(double[][] newFilter) {
        int countTempl = 0;
        Random rand = new Random();
        Convolution templ = Filters.getFirst();
        while(templ.cisloFiltra != cisloFiltra-1) {
                templ = Filters.get(countTempl++);
        }
                for(int i=0; i<newFilter.length; i++) {             //  getUpperBound(0)+1
            for(int j=0; j<newFilter[1].length; j++) {              //getUpperBound(1)+1
                if((cisloFiltra != 5)||(cisloFiltra != 10)){
                    do{
                       newFilter[i][j] = -2 + Math.random()*4;   //rand.Next(-6,5)*0.1 + 0.1;
                    } while (newFilter[i][j] == templ.weights[i][j]);
                }
                else {
                    do{
                       newFilter[i][j] = -2 + Math.random()*4;   //rand.Next(-6,5)*0.1 + 0.1;
                    } while (newFilter[i][j] == 0);
                }
            }
        }
        return newFilter;
    }

    public void countAverageOutput() {
//        Double templ = allOutputs.getFirst();
//        for(int i=0; i<allOutputs.size(); i++) {
//            averageOutput += templ;
//            templ = allOutputs.get(i);
//        }
        averageOutput =   averageOutput/ outputSize ;        //averageOutput/allOutputs.size();
    }

    public void countAverageInput() {
        int countTempl = 0;
        for (int i=0; i<size; i++) {
            for (int j=0; j<size; j++) {
//                Input templ = avInput[i][j].inputList.head;
//                for(int k=0; k<avInput[i][j].inputList.size-10; k++) {
////                    if(k==580) {
////                        k=580;
////                    }
//                    avInput[i][j].average += templ.value;
//                    templ = templ.next;
//                }
                avInput[i][j] = avInput[i][j]/inputSize[i][j];
            }
        }
    }
}

