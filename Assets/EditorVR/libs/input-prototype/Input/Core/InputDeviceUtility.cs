using UnityEngine;
using UnityEngine.InputNew;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

namespace UnityEngine.InputNew
{
	public static class InputDeviceUtility
	{
		static Dictionary<System.Type, InputDevice> s_DeviceInstances = new Dictionary<System.Type, InputDevice>();
		static Dictionary<System.Type, string[]> s_DeviceControlNames = new Dictionary<System.Type, string[]>();
		
		static string[] s_DeviceNames = null;
		static Type[] s_DeviceTypes = null;
		static Dictionary<Type, int> s_IndicesOfDevices = null;
		static string[] s_EmptyList = new string[0];
		
		public static InputDevice GetDevice(System.Type type)
		{
			InputDevice device = null;
			if (!s_DeviceInstances.TryGetValue(type, out device))
			{
				device = (InputDevice)System.Activator.CreateInstance(type);
				s_DeviceInstances[type] = device;
			}
			return device;
		}
		
		public static string GetDeviceName(DeviceSlot deviceSlot)
		{
			return (Type)deviceSlot.type == null ? "No-Device" : deviceSlot.type.Name;
		}

		public static string GetDeviceNameWithTag(DeviceSlot deviceSlot)
		{
			string deviceName = "No-Device";

			if (deviceSlot != null && (Type)deviceSlot.type != null)
			{
				if (deviceSlot.tagIndex == -1)
					deviceName = deviceSlot.type.Name;
				else
				{
					string[] tags = GetDeviceTags(deviceSlot.type);
					deviceName = string.Format("{0}.{1}", deviceSlot.type.Name, tags[deviceSlot.tagIndex]);
				}
			}

			return deviceName;
		}
		
		public static string GetDeviceControlName(DeviceSlot deviceSlot, InputControlDescriptor source)
		{
			if (deviceSlot != null)
			{
				string[] names = GetDeviceControlNames(deviceSlot.type);
				if (source.controlIndex >= 0 && source.controlIndex < names.Length)
					return names[source.controlIndex];
			}

			return "None";
		}
		
		static string GetDeviceControlName(System.Type type, int controlIndex)
		{
			return GetDeviceControlNames(type)[controlIndex];
		}
		
		public static string[] GetDeviceControlNames(System.Type type)
		{
			if (type == null)
				return s_EmptyList;
			string[] names = null;
			if (!s_DeviceControlNames.TryGetValue(type, out names))
			{
				InputDevice device = GetDevice(type);
				names = new string[device.controlCount];
				for (int i = 0; i < names.Length; i++)
					names[i] = device.GetControlData(i).name;
				s_DeviceControlNames[type] = names;
			}
			return names;
		}
		
		static void InitDevices()
		{
			if (s_DeviceTypes != null)
				return;
			
			s_DeviceTypes = (
				from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
				from assemblyType in domainAssembly.GetExportedTypes()
				where assemblyType.IsSubclassOf(typeof(InputDevice))
				select assemblyType
			).OrderBy(e => GetInheritancePath(e)).ToArray();
			
			s_DeviceNames = s_DeviceTypes.Select(e => string.Empty.PadLeft(GetInheritanceDepth(e) * 3) + e.Name).ToArray();
			
			s_IndicesOfDevices = new Dictionary<Type, int>();
			for (int i = 0; i < s_DeviceTypes.Length; i++)
				s_IndicesOfDevices[s_DeviceTypes[i]] = i;
		}
		
		public static string[] GetDeviceNames()
		{
			InitDevices();
			return s_DeviceNames;
		}
		
		public static int GetDeviceIndex(Type type)
		{
			InitDevices();
			return (type == null ? -1 : s_IndicesOfDevices[type]);
		}
		
		public static Type GetDeviceType(int index)
		{
			InitDevices();
			return s_DeviceTypes[index];
		}

	    public static string[] GetDeviceTags(Type type)
	    {
	        string[] tags = null;
            PropertyInfo info = type.GetProperty("Tags", BindingFlags.Static | BindingFlags.Public);
	        if (info != null)
	        {
                tags = (string[])info.GetValue(null, null);
            }
            return tags;
	    }
		
		static string GetInheritancePath(Type type)
		{
			if (type.BaseType == typeof(InputDevice))
				return type.Name;
			return GetInheritancePath(type.BaseType) + "/" + type.Name;
		}
		
		static int GetInheritanceDepth(Type type)
		{
			if (type.BaseType == typeof(InputDevice))
				return 0;
			return GetInheritanceDepth(type.BaseType) + 1;
		}
	}
}
