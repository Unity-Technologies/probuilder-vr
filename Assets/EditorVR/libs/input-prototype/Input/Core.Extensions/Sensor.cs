using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	public abstract class Sensor
		: InputDevice
	{
		#region Constructors

		protected Sensor(string deviceName, List<InputControlData> controls)
			: base(deviceName, controls) { }

		#endregion
	}
}
