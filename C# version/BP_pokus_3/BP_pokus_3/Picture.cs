using System;
namespace BP_pokus_3
{
	
	public class Picture
	{	
		public double[,,] map3D = null;
		public double[,] map2D = null;
		public bool isColor;
//		public int size1 ;		//Y -demention
//		public int size2 ;		//X -demention
//		public int size3 = 0;	//Z -demention (if exists)
		
		
		public Picture(int size1, int size2, int size3) {		//for 3-d picture
			map3D = new double[size1, size2, size3];
			isColor = true;
//			this.size1 = size1;
//			this.size2 = size2;
//			this.size3 = size3;						
		}
		
		public Picture(int size1, int size2) {					//for 2-d picture
			map2D = new double[size1, size2];
			isColor = false;
//			this.size1 = size1;
//			this.size2 = size2;
		}
		
		public Picture(double[,] array) {
			map2D = array;
//			size1 = map2D.GetUpperBound(0) +1;
//			size2 = map2D.GetUpperBound(1) +1;
		}
	}
	
}
