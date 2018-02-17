using System;
namespace kinkaudio
{
	public class Envelopes
	{
		public static float AHD(float atk, float hld, float dcy, int time)
		{
			float timeSingle = Convert.ToSingle(time);
			if (timeSingle < atk) return atk / timeSingle;
			else if (timeSingle >= atk && timeSingle <= (atk + hld)) return 1;
			else if (timeSingle >= (atk + hld) && timeSingle <= (atk + hld + dcy)) return (timeSingle - (atk + hld)) / dcy * 50 + 1;
			else if (timeSingle >= (atk + hld + dcy)) return 10000;
			else return 10000;
		}
		public static float AHDS(float atk, float hld, float dcy, float sus, int time)
		{
			// USE REL AFTER THIS ONE !!!!
			float timeSingle = Convert.ToSingle(time);
			if (timeSingle < atk) return atk / timeSingle;
			else if (timeSingle >= atk && timeSingle <= (atk + hld)) return 1;
			else if (timeSingle >= (atk + hld) && timeSingle <= (atk + hld + dcy))
			{
				return ((timeSingle - (atk + hld)) / dcy) * sus * 50 + 1;
			}
			else if (timeSingle >= (atk + hld + dcy)) return sus;
			else return 10000;
		}
		public static float Rel(int time, float length, float input)
		{
			float timeSingle = Convert.ToSingle(time);
			return  (timeSingle / length * 50 + input);
		}
	}
}
