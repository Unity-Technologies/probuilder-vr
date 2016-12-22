using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputNew;

public class MouseInputToEvents
	: MonoBehaviour
{
	bool m_Ignore = false;
	private bool m_HaveSentResetEvent;

	public void Update()
	{
#if !UNITY_IOS && !UNITY_ANDROID
		SendButtonEvents();
		SendMoveEvent();
#endif
	}

	void SendButtonEvents()
	{
		HandleMouseButton(0, PointerControl.LeftButton);
		HandleMouseButton(1, PointerControl.RightButton);
		HandleMouseButton(2, PointerControl.MiddleButton);
	}
	
	void HandleMouseButton(int buttonIndex, PointerControl buttonEnumValue)
	{
		if (Input.GetMouseButtonDown(buttonIndex))
		{
			if (UnityEngine.EventSystems.EventSystem.current != null &&
				UnityEngine.Cursor.lockState != CursorLockMode.Locked &&
				UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
				m_Ignore = true;
			else
				SendClickEvent(buttonEnumValue, true);
		}
		if (Input.GetMouseButtonUp(buttonIndex))
		{
			if (m_Ignore)
				m_Ignore = false;
			else
				SendClickEvent(buttonEnumValue, false);
		}
	}

	void SendMoveEvent()
	{
		if (m_Ignore)
			return;
		
		var deltaX = Input.GetAxis("Mouse X");
		var deltaY = Input.GetAxis("Mouse Y");

		var deltaZero = (deltaX == 0.0f && deltaY == 0.0f);
		if (deltaZero && m_HaveSentResetEvent)
			return;

		var inputEvent = InputSystem.CreateEvent<PointerMoveEvent>();
		inputEvent.deviceType = typeof(Mouse);
		inputEvent.deviceIndex = 0;
		inputEvent.delta = new Vector3(deltaX, deltaY, 0.0f);
		inputEvent.position = Input.mousePosition;

		InputSystem.QueueEvent(inputEvent);

		if (deltaZero)
			m_HaveSentResetEvent = true;
		else
			m_HaveSentResetEvent = false;
	}

	void SendClickEvent(PointerControl controlIndex, bool clicked)
	{
		////REVIEW: should this be a pointer-specific event type?
		var inputEvent = InputSystem.CreateEvent<GenericControlEvent>();
		inputEvent.deviceType = typeof(Mouse);
		inputEvent.deviceIndex = 0;
		inputEvent.controlIndex = (int)controlIndex;
		inputEvent.value = clicked ? 1.0f : 0.0f;
		InputSystem.QueueEvent(inputEvent);
	}
}
