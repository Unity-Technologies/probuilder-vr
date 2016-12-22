using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	public class KeyboardEvent
		: InputEvent
	{
		#region Public Methods

		public override string ToString()
		{
			return string.Format("({0}, key:{1}, isDown:{2})", base.ToString(), key, isDown);
		}

		#endregion

		#region Public Properties

		public KeyCode key { get; set; }
		public bool isDown { get; set; }
		public bool isRepeat { get; set; }
		public int modifiers { get; set; }

		#endregion
	}
}
