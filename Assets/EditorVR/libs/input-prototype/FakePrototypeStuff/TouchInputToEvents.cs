using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputNew;
using Touch = UnityEngine.Touch;

public class TouchInputToEvents : MonoBehaviour
{
	void Awake()
	{
		Input.simulateMouseWithTouches = false;

		var touchCount = Input.touchCount;
		for (var i = 0; i < m_LastTouches.Length && i < touchCount; ++i)
			m_LastTouches[i] = Input.GetTouch(i);
	}

	void Update()
	{
		var touchCount = Input.touchCount;
		for (var i = 0; i < m_LastTouches.Length && i < touchCount; ++i)
		{
			var touch = Input.GetTouch(i);

			if (touch.position == m_LastTouches[i].position
				&& touch.phase == m_LastTouches[i].phase)
				continue;

			var newTouch = new UnityEngine.InputNew.Touch
			{
				fingerId = touch.fingerId
				, position = touch.position
				, delta = touch.deltaPosition
				, deltaTime = touch.deltaTime
				, phase = touch.phase
				, time = Time.time
			};

			SendTouchEvent(newTouch);
		}
	}

	private void SendTouchEvent(UnityEngine.InputNew.Touch touch)
	{
		var inputEvent = InputSystem.CreateEvent<TouchEvent>();
		inputEvent.deviceType = typeof(Touchscreen);
		inputEvent.deviceIndex = 0;
		inputEvent.touch = touch;

		InputSystem.QueueEvent(inputEvent);
	}

	private Touch[] m_LastTouches = new Touch[Touchscreen.MaxConcurrentTouches];
}
