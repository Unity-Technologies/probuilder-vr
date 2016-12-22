using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Utilities
{
	public static class ArrayHelpers
	{
		public static void Resize<T>(ref T[] array, int newSize)
		{
			if (array == null)
				array = new T[1];
			else
			{
				var newArray = new T[newSize];
				Array.Copy(array, newArray, array.Length);
				array = newArray;
			}
		}

		public static void AppendUnique<T>(ref T[] array, T value)
		{
			if (array == null)
			{
				array = new T[1];
				array[0] = value;
			}
			else
			{
				if (array.Contains(value))
					return;

				Resize(ref array, array.Length + 1);
				array[array.Length - 1] = value;
			}
		}
	}
}
