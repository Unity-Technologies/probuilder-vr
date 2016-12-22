using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Utilities
{
	public static class EnumHelpers
	{
		public static int GetValueCount<TEnum>()
		{
			// Slow...
			var values = (int[])Enum.GetValues(typeof(TEnum));
			var set = new HashSet<int>();
			var count = 0;
			foreach(var value in values)
			{
				if(set.Add(value))
					count++;
			}
			return count;
		}
	}
}
