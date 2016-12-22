using System;
using System.Collections.Generic;
using Assets.Utilities;
using UnityEngine;

namespace UnityEngine.InputNew
{
	[Serializable]
	public struct JoystickControlMapping
	{
		public int targetIndex;
		public Range fromRange;
		public Range toRange;
		public Range interDeadZoneRange;
	}
}
