using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	public class GenericControlEvent
		: InputEvent
	{
		#region Public Methods

		public override string ToString()
		{
			return string.Format("({0}, index:{1}, value:{2})", base.ToString(), controlIndex, value);
		}

		#endregion

		#region Public Properties

		public int controlIndex { get; set; }
		public float value { get; set; }

		#endregion
	}
}
