using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	public class TouchEvent
		: InputEvent
	{
		#region Public Methods

		public override string ToString()
		{
			return string.Format("{0} finger={1} phase={2} pos={3} delta={4}",
				base.ToString(), touch.fingerId, touch.phase, touch.position, touch.delta);
		}

		#endregion

		#region Public Properties

		public Touch touch;

		#endregion
	}
}
