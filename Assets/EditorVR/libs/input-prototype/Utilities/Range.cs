using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Utilities
{
	[Serializable]
	public struct Range
	{
		public static Range full = new Range(-1, 1);
		public static Range negative = new Range(0, -1);
		public static Range positive = new Range(0, 1);
		public static Range fullInverse = new Range(1, -1);
		
		public float min;
		public float max;

		public Range(float min, float max)
		{
			this.min = min;
			this.max = max;
		}
	}
}
