
import java.util.Random;
import java.awt.image.BufferedImage;
import java.io.*;
import javax.imageio.*;
import java.awt.Color;

public class Program implements Serializable {

    static Boolean firstEnter = true;
    static String progressInfo;
    static String _info;
    static String _convolutionsInfo;
    static int Answer;
    static boolean[] UzByli = new boolean[]{false, false, false, false, false, false, false, false, false, false};
    static Neuronet net;
    static double Max;

    private static void getAllPicture() {
        String s = null;
        for (int i=0; i<10; i++) {
            s=i+".jpeg";
            writeConvolution(ImgToRightPicture(s), "");
        }
    }
    public int counterAddNeuron = 0;
    public int iterationWithoutNewNeuron = 0;
    boolean newNeuron = false;

    public static void main(String[] args) {
//        net = deseralizace("weights");
//        Neuronet.inputLength = net.inputLengthOwn;
//        Neuronet.prvniVrstva = net.prvniVrstvaOwn;
//        Neuronet.druhaVrstva = net.druhaVrstvaOwn;
//        Neuronet.tretiVrstva = net.tretiVrstvaOwn;
//        writeProgressInfo(0,test());					//vysledek ve fajlu "short progress info"
//        getAllPicture();
        net = new Neuronet();
        study();
//        button1.BackColor = Color.Green;
    }

    public static void newEpoch() {
        for (int i = 0; i < 10; i++) {
            UzByli[i] = false;
        }
    }

    public Program(int i) {
    }

    public static double[][] getPicture() {
//        System.out.println("Get picture");
        Random rand = new Random();
        int a;;
        do {
            a = rand.nextInt(10);
        } while (UzByli[a] == true);
        String fileName = a + ".jpeg";
        UzByli[a] = true;
        Answer = a;
        return ImgToRightPicture(fileName);
    }

    static double[][] ImgToRightPicture(String file)  {
//            Image img = Image.FromFile(file);
        BufferedImage img = null;
        try {
            img = ImageIO.read(new File(file));
        } catch (IOException e) {
        }

        int color;
        double[][] vysledek = new double[39][30];

        for (int i = 0; i < 39; i++) {
            for (int j = 0; j < 30; j++) {
                color = img.getRGB(j, i); //bm.GetPixel(j,i);
                int red = (color >>> 16)& 0xFF;
                int green = (color >>> 8)& 0xFF;
                int blue = (color >>> 0)& 0xFF;
                double delta = (0.2126f * red + 0.7152f * green + 0.0722f * blue)/255;
                vysledek[i][j] = delta;
            }
        }
        return vysledek;
    }

    static int calculateResult(double[][] picture) {
        
        Convolution templ = net.convolutions.head;                              
        for (int i = 0; i < net.convolutions.size; i++) {
            templ.clearInputMass();                                             //mazani zbytecnych dat
            templ.clearOutput();
            templ = templ.next;
        }
        double[][][] firstConvolution = new double[net.convolutions.size / 3][][];		//массив массивов?? [][,]		//prvni vrstva, convolution 11x11
        for (int i = 0; i < net.convolutions.size / 3; i++) {
            firstConvolution[i] = applyConvolution(i, picture);			//prvni konvoluce
            firstConvolution[i] = function(firstConvolution[i], "Tanh");	// prvni funkce aktivace (Tanh)
            firstConvolution[i] = addY(firstConvolution[i]);
            firstConvolution[i] = pooling(2, firstConvolution[i]);		//prvni pooling
        }
//        writeConvolution(firstConvolution[1], "1 vrstva");
        double[][][] secondConvolution = new double[(int)Math.pow(net.convolutions.size / 3, 2)][][];
        int cisloFiltra = net.convolutions.size / 3 ;
        int cisloPolozky = 0;
        for (int j = 0; j < net.convolutions.size / 3; j++) {
            for (int i = 0; i < net.convolutions.size / 3; i++) {
                secondConvolution[cisloPolozky] = applyConvolution(cisloFiltra, firstConvolution[j]);
                secondConvolution[cisloPolozky] = function(secondConvolution[cisloPolozky], "Tanh");
                secondConvolution[cisloPolozky] = addY(secondConvolution[cisloPolozky]);
                secondConvolution[cisloPolozky] = pooling(2, secondConvolution[cisloPolozky]);
                cisloFiltra++;
                cisloPolozky++;
            }
            cisloFiltra = net.convolutions.size / 3;
//            writeConvolution(secondConvolution[1], "2 vrstva");
        }
        double[][][] thirdConvolution = new double[(int)Math.pow(net.convolutions.size / 3, 3)][][];
        cisloFiltra = net.convolutions.size / 3 * 2;
        cisloPolozky = 0;
        for (int j = 0; j < Math.pow(net.convolutions.size / 3, 2); j++) {
            for (int i = 0; i < net.convolutions.size / 3; i++) {
                thirdConvolution[cisloPolozky] = applyConvolution(cisloFiltra, secondConvolution[j]);
                thirdConvolution[cisloPolozky] = addY(thirdConvolution[cisloPolozky]);
                thirdConvolution[cisloPolozky] = function(thirdConvolution[cisloPolozky], "Tanh");
                thirdConvolution[cisloPolozky] = pooling(3, thirdConvolution[cisloPolozky]);
                cisloFiltra++;
                cisloPolozky++;
            }
            cisloFiltra = net.convolutions.size / 3 * 2;
        }
//        writeConvolution(thirdConvolution[1], "3 vrstva");
        double[] inputFullyConnectionNet = doOneArray(thirdConvolution);
        net.l0.writeInput(inputFullyConnectionNet);				//zapisuje vstupni vektor v "fully connected" neuronovou sit
        net.l0.countOutputs();
        Neuron templ1 = net.l1.head;
        for (int i = 0; i < Neuronet.druhaVrstva; i++) {			//zapisuje do druhe vrstvy FC neuronove siti vstupni signaly
            for (int j = 0; j < Neuronet.prvniVrstva; j++) {
                templ1.input[j] = net.l0.outputs[j];
            }
            templ1 = templ1.next;
        }
        net.l1.countOutputs();
        templ1 = net.l2.head;
        for (int i = 0; i < Neuronet.tretiVrstva; i++) {			// zapisuje do treti vrstvy FC neuronove siti vstupni signaly
            for (int j = 0; j < Neuronet.druhaVrstva; j++) {
                templ1.input[j] = net.l1.outputs[j];
            }
            templ1 = templ1.next;
        }
        net.l2.countOutputs();
        int index = 0;          	//cislo neuronu ktery vyhral
        Max = net.l2.outputs[0];
        for (int i = 0; i < net.l2.outputs.length; i++) {
            if (Max < net.l2.outputs[i]) {
                Max = net.l2.outputs[i];
                index = i;
            }
        }

        
        templ = net.convolutions.head;                              
        for (int i = 0; i < net.convolutions.size; i++) {
            templ.countAverageInput();                                          //spocitani prumerneho vstupu
            templ.countAverageOutput();
//            templ.clearInputMass();                                             //mazani zbytecnych dat
//            templ.clearOutput();
            templ = templ.next;
        }
        return index;
    }

    static double[][] function(double[][] picture, String nazevFunkce) {
        double[][] result = picture;
        for (int i = 0; i < picture.length; i++) {
            for (int j = 0; j < picture[1].length; j++) {
                if (nazevFunkce == "Tanh") {
                    result[i][j] = 1.7159 * Math.tanh(0.66 * picture[i][j]);
                } else if (nazevFunkce == "ReLu") {
                    if (result[i][j] > 0) {
                        result[i][j] = result[i][j];
                    } else {
                        result[i][j] = 0;
                    }
                }

            }
        }
        return result;
    }

    static void study() {
        double[] err = new double[Neuronet.tretiVrstva];
        int iteration = 1;
        double lokError = 0;
        int lokResult = 0;
        double errorMin = 100;
        int testValue = 0;
        int bestTestValue = 0;
        int gradNull = 0;
        writeAllConvolution(0);

        while (testValue < 100) {
            lokResult = calculateResult(getPicture());
            Neuron templ3 = net.l2.head;
            if((Double.isNaN(templ3.weights[34])||(Double.isNaN(net.l0.head.weights[1]))||(Double.isNaN(net.l1.head.weights[1])))) {
                System.out.println("NAN NAN NAN iteration "+ iteration);
            }
            if(iteration == 12 ) {
                    int asd = 2;
            }
            for (int i = 0; i < Neuronet.tretiVrstva; i++) {
                if (Answer == i) {
                    err[i] = Max - templ3.output;				//zapisuje signal chyby vystupni vrstvy
                } else {
                    err[i] = 0 - templ3.output;
                }
                templ3 = templ3.next;
            }
            Neuron templ2;
            templ3 = net.l2.head;
            for (int i = 0; i < Neuronet.tretiVrstva; i++) {
                templ2 = net.l1.head;
                templ3.grad = 0.388 * (1.716 - templ3.output) * (1.716 + templ3.output) * err[i];   //pocita gradient pro vystupni vyrstvu
                for (int j = 0; j < Neuronet.druhaVrstva; j++) {
                    templ3.weights[j] += net.speedLFCN * templ2.output * templ3.grad;     //pocita vahy pro vystupni vrstvu
                    templ2 = templ2.next;
                }
                templ3 = templ3.next;
            }

            double grad = 0;
            Neuron templ1;
            templ2 = net.l1.head;
            for (int i = 0; i < Neuronet.druhaVrstva; i++) {
                grad = 0;
                templ3 = net.l2.head;
                for (int u = 0; u < Neuronet.tretiVrstva; u++) {		//sumarizuje gradient predhozi vrstvy (delta pravidlo pro druhou vrstvu)
                    grad += templ3.grad * templ3.weights[i];
                    templ3 = templ3.next;
                }
                templ2.grad = grad * 0.388 * (1.716 - templ2.output) * (1.716 + templ2.output);
                templ1 = net.l0.head;
                for (int j = 0; j < Neuronet.prvniVrstva; j++) {
                    templ2.weights[j] += net.speedLFCN * templ1.output * templ2.grad;
                    templ1 = templ1.next;
                }
                templ2 = templ2.next;
            }

            templ1 = net.l0.head;
            for (int i = 0; i < Neuronet.prvniVrstva; i++) {
                grad = 0;
                templ2 = net.l1.head;
                for (int u = 0; u < Neuronet.druhaVrstva; u++) {		//sumarizuje gradient predhozi vrstvy (delta pravidlo pro prvni vrstvu)
                    grad += templ2.grad * templ2.weights[i];
                    templ2 = templ2.next;
                }
                templ1.grad = grad * 0.388 * (1.716 - templ1.output) * (1.716 + templ1.output);//1.7159
                for (int j = 0; j < Neuronet.inputLength; j++) {
                    templ1.weights[j] += net.speedLFCN * templ1.input[j] * grad;
                }
                templ1 = templ1.next;
            }

            //pro filtry 10 az 14
            Convolution templ = net.convolutions.head;
            while (templ.cisloFiltra != net.convolutions.size / 3 * 2 ) {
                templ = templ.next;
            }
            for (int i = 0; i < net.convolutions.size / 3; i++) {
                grad = 0;
                templ1 = net.l0.head;                                           //vrstva se ktere scita gradienty
                for (int j = 0; j < net.l0.length; j++) {
                    grad += templ1.grad;					//sumarizuje gradient predhozi vrstvy
                    templ1 = templ1.next;
                }
                templ.grad = grad * 0.388 * (1.7159 - templ.averageOutput) * (1.7159 + templ.averageOutput) / net.l0.length;
                if (grad == 0) {
                    gradNull++;
                }
                double delta;
                for (int k = 0; k < templ.weights.length; k++) {
                    for (int q = 0; q < templ.weights[0].length; q++) {
                        delta = net.speedL1CL * templ.grad * templ.avInput[k][q];
                        templ.weights[k][q] += delta;
                    }
                    
                }
//                writeConvolution(templ.weights, "");
//                writeAllConvolution(iteration);
                templ = templ.next;
            }

            //pro filtry 5 az 9
            templ = net.convolutions.head;
            while (templ.cisloFiltra != net.convolutions.size / 3 ) {
                templ = templ.next;
            }
//            int countTemplLast = 0;
            for (int i = 0; i < net.convolutions.size / 3; i++) {
                grad = 0;
                Convolution templLast = net.convolutions.head;          //vrstva se ktere scita gradienty
                while (templLast.cisloFiltra != net.convolutions.size / 3 * 2 ) {
                    templLast = templLast.next;
                }
                for (int j = 0; j < net.convolutions.size / 3; j++) {
                    grad += templLast.grad;				//sumarizuje gradient predhozi vrstvy
                    templLast = templLast.next;
                }
                templ.grad = grad * 0.388 * (1.7159 - templ.averageOutput) * (1.7159 + templ.averageOutput) / Neuronet.prvniVrstva;
                if (grad == 0) {
                    gradNull++;
                }
                for (int k = 0; k < templ.weights.length; k++) {
                    for (int q = 0; q < templ.weights[0].length; q++) {
                        templ.weights[k][q] += net.speedL2CL * templ.grad * templ.avInput[k][q];
                    }
                }
                templ = templ.next;
            }

            //pro filtry 0 az 4
            templ = net.convolutions.head;
            for (int i = 0; i < net.convolutions.size / 3; i++) {
                grad = 0;
//                countTemplLast = 0;
                Convolution templLast = net.convolutions.head;		//vrstva se ktere scita gradienty
                while (templLast.cisloFiltra != net.convolutions.size / 3 ) {
                    templLast = templLast.next;
                }
                for (int j = 0; j < net.convolutions.size / 3; j++) {
                    grad += templLast.grad;					//sumarizuje gradient predhozi vrstvy
                    templLast = templLast.next;
                }
                templ.grad = grad * 0.388 * (1.7159 - templ.averageOutput) * (1.7159 + templ.averageOutput) / Neuronet.druhaVrstva;
                if (grad == 0) {
                    gradNull++;
                }
                for (int k = 0; k < templ.weights.length; k++) {
                    for (int q = 0; q < templ.weights[0].length; q++) {
                        templ.weights[k][q] += net.speedL3CL * templ.grad * templ.avInput[k][q];
//                        System.out.println(templ.cisloFiltra+"  " +templ.weights[k][q]);
                    }
                }
                templ = templ.next;
            }

            if (Answer != lokResult) {
                lokError++;
            }
            if ((lokError / iteration * 100) < errorMin) {
                errorMin = lokError / iteration * 100;
            }
            if (iteration % 10 == 0) {
                testValue = test();
                if (testValue > bestTestValue) {
                    bestTestValue = testValue;
                    System.out.println("");
                    System.out.println("Better result >>>>>>>>>>>  "+bestTestValue);
                    System.out.println("");
                }
                writeProgressInfo(iteration, testValue);
                if (iteration % 100 == 0) {
                    writeAllConvolution(iteration);
                }
                gradNull = 0;
                newEpoch();
            }
            if (iteration % 1000000 == 0) {
                _convolutionsInfo = null;
            }
            if (iteration % 1000000 == 0) {
                progressInfo = null;
            }
            iteration++;
        }
//        writeAllConvolution(iteration); 
        try {
            serializace("normal");
        } catch (Exception e){
            System.out.println("Serialization error in 'normal' way");
        }
    }

    static double[][] pooling(int size, double[][] picture) {             	// size treba 2x2 => size=2
        double templ = (double)picture.length / size;                                   //jenom pro to, aby Math.ceil spravne zaokrouhlil
        int massSize1 = (int)Math.ceil(templ);
        templ = (double)(picture[0].length) / size;                                     //jenom pro to, aby Math.ceil spravne zaokrouhlil
        int massSize2 = (int)Math.ceil(templ);
        if (massSize2 == 0) {
            massSize2 = 1;
        }
        double[][] result = new double[massSize1][massSize2];
        int x0, x1, y0, y1;
        y0 = 0;
        y1 = size;
        for (int i = 0; i < result.length; i++) {
            x0 = 0;
            x1 = size;
            for (int j = 0; j < result[0].length; j++) {
                result[i][j] = max(picture, x0, x1, y0, y1);
                x0 += size;
                x1 += size;
            }
            y0 += size;
            y1 += size;
        }
        return result;
    }

    static double max(double[][] picture, int x0, int x1, int y0, int y1) {
        double result = picture[y0][x0];
        for (int i = y0; i < y1; i++) {
            for (int j = x0; j < x1; j++) {
                try{
                    if (picture[i][j] > result) {
                        result = picture[i][j];
                    }
                }
                catch(Exception e){}
            }
        }
        return result;
    }

    static double[][] applyConvolution(int cisloFiltra, double[][] picture) {
        int countTempl = 0;
        Convolution templ = net.convolutions.head;
        while (cisloFiltra != 0) {
            templ = templ.next;
            cisloFiltra--;
        }
        int x = picture[0].length - templ.weights[0].length + 1;		// rozmer vysledne matici - x a y
        int y = picture.length - templ.weights.length + 1;
        double[][] result = new double[y][x];
        int x0, y0;
        for (int i = 0; i < y; i++) {
            x0 = 0;
            y0 = 0;
            for (int j = 0; j < x; j++) {
                result[i][j] = sum(picture, templ, x0, y0);
                x0++;
            }
            y0++;
        }
        return result;
    }

    static double sum(double[][] picture, Convolution templ, int x0, int y0) {
        double result = 0;
        int y = 0;			// kountery pro konvoluce
        int x = 0;

        for (int i = y0; i < y0 + templ.size; i++) {
            for (int j = x0; j < x0 + templ.size; j++) {
                if (j == (int) picture[0].length) { //length+1 ?
                    result += picture[i][j - 1] * templ.weights[y][x];
//                    templ.avInput[y][x].inputList.addInput(new Input(picture[i][j - 1]));
                    templ.addInput(picture[i][j - 1], y, x);  
                } else {
                    result += picture[i][j] * templ.weights[y][x];
                    templ.addInput(picture[i][j], y, x);
//                        if(templ.avInput[y][x].inputList.head == null){
//                             templ.avInput[y][x].inputList = new ListOfInput(new Input(picture[i][j]));
//                        }
//                    templ.avInput[y][x] += picture[i][j];
                }
                x++;
            }
            x = 0;
            y++;
        }
        templ.addOutput(result);    // += result;
        return result;
    }

    static double[] doOneArray(double[][][] thirdConvolution) {
        int length = thirdConvolution.length * thirdConvolution[10].length;
        double[] result = new double[length];
        int counter = 0;
        for (int i = 0; i < thirdConvolution.length; i++) {
            for (int j = 0; j < thirdConvolution[i].length; j++) {
                for (int k = 0; k < thirdConvolution[i][0].length; k++) {
                    result[counter] = thirdConvolution[i][j][k];
                    counter++;
                }
            }
        }
        return result;
    }

    public static void writeInfo(double[][] picture, String typeOfTransformation) //zobrazuje zmeny primo v konvolucich
    {
        _info += typeOfTransformation + " -> " + "\r\n" + "x- " + picture[0].length + "\r\n" + "y -" + picture.length + "\r\n";
        try {
            File.createTempFile("Date.txt", _info); //WriteAllText("Date.txt", _info);
        } catch (IOException e) {
        }
        writeConvolution(picture, typeOfTransformation);
    }

//        static void writeProgressInfo(int iteration, double errorMin, int testValue, Stopwatch timer, int gradNull) {
//                progressInfo += "epoch = "+iteration/10+" error min. = "+errorMin+" test= "+testValue+"     h:"+timer.Elapsed.Hours.ToString()+" m:"+timer.Elapsed.Minutes.ToString()+" s:"+timer.Elapsed.Seconds.ToString() +" counter null grad-"+gradNull+"\r\n";                                      			
//                File.WriteAllText("Progress info.txt", progressInfo);
//        }
    static void writeProgressInfo(int iteration, double testValue) {
        progressInfo += "epoch = " + iteration / 10 + " test value = " + testValue + "\r\n";
        System.out.println("epoch = " + iteration / 10 + " test value = " + testValue);
        write("Short progress info.txt", progressInfo);
    }

    static void writeAllConvolution(int iteration) {
        Convolution templ = net.convolutions.head;
        for (int i = 0; i < net.convolutions.size; i++) {
            writeConvolution(templ.weights, i, iteration);
            templ = templ.next;
        }
    }
    
    static void writeConvolution(double[][] convolution, int counvolutionNumber, int iteration ) {
        if(firstEnter) {
            firstEnter = false;
            _convolutionsInfo = null;
        }
        _convolutionsInfo += "iteration:"+iteration+" №:"+counvolutionNumber;
         _convolutionsInfo += "\r\n";
        for (int i = 0; i < convolution.length; i++) {
            for (int j = 0; j < convolution[0].length; j++) {
                _convolutionsInfo += convolution[i][j] + "  ";
            }
            _convolutionsInfo += "\r\n";
        }
         _convolutionsInfo += "\r\n";
        
        write("Convolutions.txt", _convolutionsInfo);
    }

    static void writeConvolution(double[][] picture, String typeOfTransformation) {
        _convolutionsInfo += typeOfTransformation + "\r\n";
        for (int i = 0; i < picture.length; i++) {
            for (int j = 0; j < picture[0].length; j++) {
                _convolutionsInfo += picture[i][j] + "  ";
            }
            _convolutionsInfo += "\r\n";
        }
        _convolutionsInfo += "\r\n" + "\r\n";
        write("Convolutions.txt", _convolutionsInfo);

    }

    public static int test() {
        int vysledek = 0;
        for (int j = 0; j < 1; j++) {
            newEpoch();
            for (int i = 0; i < 10; i++) {
                int vysOperace = calculateResult(getPicture());
                if (Answer == vysOperace) {
                    vysledek += 1;
                } else {
                    vysledek += 0;
                }
            }
        }
        return vysledek * 10;
    }

    public static void serializace(String wayOfSaving) throws Exception {
        String fileName = null;
        if (wayOfSaving == "normal") {
            fileName = "Wiaghts";
        } else {
            fileName = "BestWeights";;
        }
        FileOutputStream fos = new FileOutputStream(fileName+".out");
        ObjectOutputStream oos = new ObjectOutputStream(fos);
        oos.writeObject(net);
        oos.close();
    }

    static Neuronet deseralizace(String way) throws Exception {
        try {
            Neuronet net = null;
            ObjectInputStream in = new ObjectInputStream(new FileInputStream(way));
            net = (Neuronet) in.readObject();
            return net;
        } catch (Exception e) {
            return new Neuronet();
        }
    }

    static double[][] addX(double[][] picture) {					//pridani sloupce '0' k polu
        double[][] result = new double[picture.length][picture[0].length + 1];
        for (int i = 0; i < picture.length; i++) {
            for (int j = 0; j < picture[0].length + 1; j++) {
                if (j == picture[0].length) {
                    result[i][j] = -100;
                } else {
                    result[i][j] = picture[i][j];
                }
            }
        }
        return result;
    }

    static double[][] addY(double[][] picture) {					//pridani radka '0' k polu
        double[][] result = new double[picture.length + 1][picture[0].length];
        for (int i = 0; i < picture.length + 1; i++) {
            for (int j = 0; j < picture[0].length; j++) {
                if (i == picture.length) {
                    result[i][j] = -100;
                } else {
                    result[i][j] = picture[i][j];
                }
            }
        }
        return result;
    }

    static void write(String fileName, String text) {
        File file = new File(fileName);
        try {
            if (!file.exists()) {
                file.createNewFile();
            }
            PrintWriter out = new PrintWriter(file.getAbsoluteFile());

            try {
                out.print(text);
            } finally {
                out.close();
            }
        } catch (IOException e) {
//            throw new RuntimeException(e);
        }
    }
}
