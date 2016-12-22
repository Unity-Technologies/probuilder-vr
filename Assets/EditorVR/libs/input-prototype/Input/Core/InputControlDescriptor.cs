using System;
using System.Collections.Generic;

namespace UnityEngine.InputNew
{
	[Serializable]
	public class InputControlDescriptor
	{
		public int controlIndex;
		public int deviceKey;
		
		public virtual InputControlDescriptor Clone()
		{
			var clone = (InputControlDescriptor)Activator.CreateInstance(GetType());
			clone.controlIndex = controlIndex;
			clone.deviceKey = deviceKey;
			return clone;
		}
		
		public override string ToString()
		{
			return string.Format( "(device:{0}, control:{1})", deviceKey, controlIndex );
		}
		
		public void ExtractDeviceTypeAndControlIndex(Dictionary<int, List<int>> controlIndicesPerDeviceType)
		{
			List<int> entries;
			if (!controlIndicesPerDeviceType.TryGetValue(deviceKey, out entries))
			{
				entries = new List<int>();
				controlIndicesPerDeviceType[deviceKey] = entries;
			}
			
			entries.Add(controlIndex);
		}
	}
}
