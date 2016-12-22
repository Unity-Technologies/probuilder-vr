using System;
using System.Collections.Generic;
using UnityEngine;

////TODO: PointerClickEvent (important as unlike GenericControlEvent it tells the position of the click)

namespace UnityEngine.InputNew
{
	public class PointerMoveEvent
		: PointerEvent
	{
		#region Public Properties

		public Vector3 delta { get; set; }

		#endregion
	}
}
