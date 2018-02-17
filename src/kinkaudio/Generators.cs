using System;
namespace kinkaudio
{
	public static class Generators
	{
		// NOTE : ALL AMPLITUDES ARE SUBTRACTIVE, NOT ADDITIVE
		public static float GenSawtooth(int time, float amplitude, float period)
		{
			float tp = time / period;
			return Convert.ToSingle(((tp) - (0.5 + Math.Floor(tp))) / amplitude);
		}
		public static float GenPulse(int time, float amplitude, float period, int duty)
		{
			//duty is out of 400, 200 is 50% etc.
			return Convert.ToSingle((Math.Floor((time % period) / (duty * (period / 400))) - 0.5) / amplitude);
		}
		public static float GenTriangle(int time, float amplitude, float period)
		{
			return Convert.ToSingle((2 / period * (Math.Abs((time % period) - period / 2) - period / 4)) / amplitude);
		}
		public static float GenPCycloid(int time, float amplitude, float period)
		{
			float radius = period / 2;
			return Convert.ToSingle((Math.Abs(Math.Pow(0 - 1, Math.Floor(time / (2 * radius) + 0.5)) * Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(time - 2 * radius * Math.Floor(time / (2 * radius) + 0.5), 2)) / radius) - 0.5) / amplitude);
		}
		public static float GenSin(int time, float amplitude, float period)
		{
			return Convert.ToSingle(Math.Sin(time / period * 8) / amplitude);
		}

	}
}
