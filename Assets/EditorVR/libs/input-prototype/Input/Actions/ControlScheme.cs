using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEngine.InputNew
{
	[Serializable]
	public class ControlScheme
	{
		[SerializeField]
		private string m_Name;
		public string name { get { return m_Name; } set { m_Name = value; } }

		[SerializeField]
		private List<DeviceSlot> m_DeviceSlots = new List<DeviceSlot>();
		public List<DeviceSlot> deviceSlots { get { return m_DeviceSlots; } set { m_DeviceSlots = value; } }
		
		[SerializeField]
		private ActionMap m_ActionMap;
		public ActionMap actionMap { get { return m_ActionMap; } }
		
		[SerializeField]
		private List<ControlBinding> m_Bindings = new List<ControlBinding> ();
		public List<ControlBinding> bindings { get { return m_Bindings; } set { m_Bindings = value; } }

		public bool customized { get; internal set; }

		public ControlScheme()
		{
		}
		
		public ControlScheme(string name, ActionMap actionMap)
		{
			m_Name = name;
			m_ActionMap = actionMap;
		}

		public virtual ControlScheme Clone()
		{
			var clone = (ControlScheme) Activator.CreateInstance(GetType());
			clone.m_Name = m_Name;
			clone.m_DeviceSlots = m_DeviceSlots.Select(x => x.Clone()).ToList();            
			clone.m_ActionMap = m_ActionMap;
			clone.m_Bindings = m_Bindings.Select(x => x.Clone()).ToList();
			// Don't clone customized flag.
			return clone;
		}

		public int GetDeviceKey(InputDevice device)
		{
			foreach (var deviceSlot in m_DeviceSlots)
			{
				if (device.GetType().IsInstanceOfType(deviceSlot.type.value) &&
					(device.tagIndex == -1 || device.tagIndex == deviceSlot.tagIndex))
					return deviceSlot.key;
			}

			return DeviceSlot.kInvalidKey;
		}

		public DeviceSlot GetDeviceSlot(int key)
		{
			foreach (var deviceSlot in m_DeviceSlots)
			{
				if (deviceSlot.key == key)
					return deviceSlot;
			}

			return null;
		}

		public void ExtractDeviceTypesAndControlIndices (Dictionary<int, List<int>> controlIndicesPerDeviceType)
		{
			foreach (var binding in bindings)
				binding.ExtractDeviceTypesAndControlIndices(controlIndicesPerDeviceType);
		}
	}
}
