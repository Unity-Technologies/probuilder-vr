using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	class InputEventQueue
	{
		#region Fields

		readonly SortedList<float, InputEvent> m_List = new SortedList<float, InputEvent>(new SortInputEventsByTime());

		#endregion

		#region Inner Types

		class SortInputEventsByTime
			: IComparer<float>
		{
			public int Compare(float x, float y)
			{
				if (x < y)
					return -1;
				// Avoid duplicate keys in sorted list by always treating equality as greater-than.
				return 1;
			}
		}

		#endregion

		#region Public Methods

		public void Queue(InputEvent inputEvent)
		{
			m_List.Add(inputEvent.time, inputEvent);
		}

		public bool Dequeue(float targetTime, out InputEvent inputEvent)
		{
			if (m_List.Count == 0)
			{
				inputEvent = null;
				return false;
			}

			var nextEvent = m_List.Values[0];
			if (nextEvent.time > targetTime)
			{
				inputEvent = null;
				return false;
			}

			m_List.RemoveAt(0);
			inputEvent = nextEvent;
			return true;
		}

		#endregion
	}
}
