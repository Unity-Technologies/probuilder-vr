using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	public enum TouchControl
	{
		Touch0Position = PointerControlConstants.ControlCount,
		Touch0PositionX,
		Touch0PositionY,

		Touch0Delta,
		Touch0DeltaX,
		Touch0DeltaY,

		Touch0Phase,
	}

	public static class TouchControlConstants
	{
		public const int ControlCount = (int)TouchControl.Touch0Phase + 1;
		public const int ControlsPerTouch = TouchControl.Touch0Phase - TouchControl.Touch0Position;
	}
}