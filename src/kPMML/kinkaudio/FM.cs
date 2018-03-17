using System;
namespace kinkaudio
{
	public class FM
	{
		public static float FM2opMergeTrunc(float inOp, float amplitude, float carrierPeriod, int time, int truncMod, int truncCar)
		{
			if (truncMod == 0 && truncCar == 0) return Convert.ToSingle(Math.Sin(time / carrierPeriod * 8 + inOp * 2000) / amplitude);
			else
			{
				if (truncMod == 1) inOp = Math.Max(0, inOp);
				else if (truncMod == 2) inOp = Math.Min(0, inOp);
				if (truncCar == 1) return Convert.ToSingle(Math.Max(0, Math.Sin(time / carrierPeriod * 8 + inOp * 2000)) / amplitude);
				else if (truncCar == 2) return Convert.ToSingle(Math.Min(0, Math.Sin(time / carrierPeriod * 8 + inOp * 2000)) / amplitude);
				else return Convert.ToSingle(Math.Sin(time / carrierPeriod * 8 + inOp * 2000) / amplitude);
			}
		}
		public static float FM3opMerge(float inOp, float car1Amp, float car1Per, float car2Amp, float car2Per, int time)
		{
			float firstPass = Convert.ToSingle(Math.Sin(time / car1Per + inOp) / car1Amp);
			return Convert.ToSingle(Math.Sin(time / car2Per + firstPass) / car2Amp);
		}
	}
}
