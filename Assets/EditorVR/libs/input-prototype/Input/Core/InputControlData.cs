using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	[Serializable]
	public struct InputControlData
	{
		public int[] componentControlIndices;
		public SerializableType controlType;
		public string name;
		public float defaultValue;
	}
}
