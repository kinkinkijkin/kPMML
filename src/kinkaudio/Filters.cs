using System;
using System.Collections.Generic;
namespace kinkaudio
{
	namespace Filters
	{
		public struct SVF
		{
			static List<float> window1 = new List<float>();
			static List<float> window2 = new List<float>();
			static float a1 = 0;
			static float a2 = 0;
			static float output1 = 0;
			static float output2 = 0;
			static float output3 = 0;
			public void RemoveIndex ( int index )
			{
				window1.RemoveAt(index);
				window2.RemoveAt(index);
			}
			public float Filter ( float input, float cutoff, int type )
			{
				output1 = input + output2 - output3;
				window1.Add(output1);
				foreach ( var item in window1 )
				{
					a1 = Math.Max(-150, Math.Min(150, a1 + item));
				}
				output2 = Convert.ToSingle((-1) 
					* (1 / (2 * Math.PI * cutoff) * a1 ));
				window2.Add(output2);
				foreach ( var item in window2 )
				{
					a2 = Math.Max(-150, Math.Min(150, a2 + item));
				}
				output3 = Convert.ToSingle((-1) 
					* (1 / (2 * Math.PI * cutoff) * a2 ));
				if ( type == 2 ) return output1;
				if ( type == 1 ) return output2;
				else return output3;
			}
		}
	}
}
