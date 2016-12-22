using System;
using System.Collections.Generic;
using Assets.Utilities;
using UnityEngine;

namespace UnityEngine.InputNew
{
	public abstract class InputDeviceProfile
	{
		#region Public Methods

		public abstract void Remap(InputEvent inputEvent);

		public void AddSupportedPlatform(string platform)
		{
			ArrayHelpers.AppendUnique(ref supportedPlatforms, platform);
		}

		public void AddDeviceName(string deviceName)
		{
			ArrayHelpers.AppendUnique(ref deviceNames, deviceName);
		}

		public void AddDeviceRegex(string regex)
		{
			ArrayHelpers.AppendUnique(ref deviceRegexes, regex);
		}
		
		public virtual string GetControlNameOverride(int controlIndex)
		{
			return null;
		}

		#endregion

		#region Public Properties

		public string[] supportedPlatforms;
		public string[] deviceNames;
		public string[] deviceRegexes;
		public string lastResortDeviceRegex;
		public Version minUnityVersion;
		public Version maxUnityVersion;

		#endregion
	}
}
