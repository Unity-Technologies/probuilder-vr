using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	public class KeyEvent
		: InputEvent
	{
		#region Public Properties

		public KeyCode rawKey { get; set; }
		public KeyCode localizedKey { get; set; }
		public bool isPress { get; private set; }
		public bool isRelease { get; private set; }
		public bool isRepeat { get; private set; }

		#endregion
	}
}
