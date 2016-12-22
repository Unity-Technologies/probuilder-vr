using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	public class PointerEvent
		: InputEvent
	{
		#region Public Methods

		public override string ToString()
		{
			return string.Format("({0}, pos:{1})", base.ToString(), position);
		}

		#endregion

		#region Public Properties

		public Vector3 position { get; set; }
		public float pressure { get; set; }
		public float tilt { get; set; }
		public float rotation { get; set; }
		public int displayIndex { get; set; }

		#endregion
	}
}
