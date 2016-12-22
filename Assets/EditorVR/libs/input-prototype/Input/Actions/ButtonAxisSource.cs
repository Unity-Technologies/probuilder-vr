using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	[Serializable]
	public class ButtonAxisSource
	{
		public InputControlDescriptor negative;
		public InputControlDescriptor positive;

		public ButtonAxisSource()
		{
		}

		public virtual ButtonAxisSource Clone()
		{
			var clone = (ButtonAxisSource) Activator.CreateInstance(GetType());
			clone.negative = negative.Clone();
			clone.positive = positive.Clone();
			return clone;
		}

		public ButtonAxisSource(InputControlDescriptor negative, InputControlDescriptor positive)
		{
			this.negative = negative;
			this.positive = positive;
		}

		public override string ToString()
		{
			return string.Format("({0}, {1})", negative, positive);
		}
	}
}
