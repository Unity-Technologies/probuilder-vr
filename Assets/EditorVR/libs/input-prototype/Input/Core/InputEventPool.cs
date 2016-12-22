using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	class InputEventPool
	{
		#region Public Methods

		public TEvent ReuseOrCreate<TEvent>()
			where TEvent : InputEvent, new()
		{
			////TODO
			return new TEvent();
		}

		public void Return(InputEvent inputEvent)
		{
			////TODO
			inputEvent.Reset();
		}

		#endregion
	}
}
