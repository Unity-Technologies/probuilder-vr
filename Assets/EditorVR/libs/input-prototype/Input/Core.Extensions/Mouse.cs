using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	public class Mouse
		: Pointer
	{
		#region Constructors

		public Mouse()
			: this("Mouse", null) { }

		public Mouse(string deviceName, List<InputControlData> controls)
			: base(deviceName, controls) 
		{
			cursor = new Cursor();
		}

		#endregion
		
		public new static Mouse current { get { return InputSystem.GetMostRecentlyUsedDevice<Mouse>(); } }
	}
}
