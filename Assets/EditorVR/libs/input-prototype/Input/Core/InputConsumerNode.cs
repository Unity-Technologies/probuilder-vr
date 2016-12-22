using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	public class InputConsumerNode : IInputConsumer
	{
		List<IInputConsumer> m_Children = new List<IInputConsumer>();
		public List<IInputConsumer> children { get { return m_Children; } }

		public bool ProcessEvent(InputEvent inputEvent)
		{
			for (int i = 0; i < m_Children.Count; i++)
			{
				if (m_Children[i].ProcessEvent(inputEvent))
					return true;
			}
			return false;
		}

		public void BeginFrame()
		{
			for (int i = 0; i < m_Children.Count; i++)
			{
				m_Children[i].BeginFrame();
			}
		}

		public void EndFrame()
		{
			for (int i = 0; i < m_Children.Count; i++)
			{
				m_Children[i].EndFrame();
			}
		}
	}
}
