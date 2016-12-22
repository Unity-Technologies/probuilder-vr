using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	internal class InputConsumerCallback : IInputConsumer
	{
		public delegate bool ProcessInputDelegate(InputEvent inputEvent);
		public delegate void FrameDelegate();

		public ProcessInputDelegate processEvent { get; set; }
		public FrameDelegate beginFrame { get; set; }
		public FrameDelegate endFrame { get; set; }

		public bool ProcessEvent(InputEvent inputEvent)
		{
			if (processEvent != null)
				return processEvent(inputEvent);
			return false;
		}

		public void BeginFrame()
		{
			if (beginFrame != null)
				beginFrame();
		}

		public void EndFrame()
		{
			if (endFrame != null)
				endFrame();
		}
	}
}
