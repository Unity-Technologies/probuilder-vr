using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	public enum PointerControl
	{
		Position,
		PositionX,
		PositionY,
		PositionZ,

		Delta,
		DeltaX,
		DeltaY,
		DeltaZ,

		LockedDelta,
		LockedDeltaX,
		LockedDeltaY,
		LockedDeltaZ,

		Pressure,
		Tilt,
		Rotation,

		LeftButton,
		RightButton,
		MiddleButton,

		ScrollWheel,
		ScrollWheelX,
		ScrollWheelY,
		////REVIEW: have Z for ScrollWheel, too?

		ForwardButton,
		BackButton,
	}

	public static class PointerControlConstants
	{
		public const int ControlCount = (int)PointerControl.BackButton + 1;
	}
}
