using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	public interface IInputConsumer
	{
		bool ProcessEvent(InputEvent inputEvent);
		void BeginFrame();
		void EndFrame();
	}
}

