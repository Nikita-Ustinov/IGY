import java.io.Serializable;
import java.util.Random;
public class Neuron implements Serializable
{
    public double [] weights, input;
    public double output, grad, sum ;
    public Neuron next;


    public Neuron(int vrstva)
    {	
        if (vrstva==0){
                weights = new double[Neuronet.inputLength];
                input = new double[Neuronet.inputLength];
        }
        if (vrstva==1){
                weights = new double[Neuronet.prvniVrstva];
                input=new double[Neuronet.prvniVrstva];
        }
        if (vrstva==2){
                weights = new double[Neuronet.druhaVrstva];
                input=new double[Neuronet.druhaVrstva];
        }
        doRandomWeights();
    }		


    public void doRandomWeights()							
    {  
            Random rand = new Random();
            for (int i=0;i<weights.length; i++){
                    do{
//                    weights[i] = -0.6+rand.nextDouble(); //-6 -> 5
                       weights[i] = -2 + Math.random()*4;
                    } while (weights[i]==0);
            }
    }		

    public double countOut()
    {
        sum=0;
        output=0;
        for (int i=0; i<input.length; i++)
        {   
            if(Double.isNaN(sum)) {
                int ads = 2;
            }
            sum+=weights[i]*input[i];
        }
        output=1.7159*Math.tanh(0.66*sum); 
        return output;
    }

    public void addOneWeight(int NeuronNumber) {
            double[] templ = weights;
            weights = new double[NeuronNumber];
            for (int i = 0; i<templ.length; i++) {
                    weights[i] = templ[i];
            }
            Random rand = new Random();
            weights[weights.length-1] = -0.5+ rand.nextDouble();
    }

}


