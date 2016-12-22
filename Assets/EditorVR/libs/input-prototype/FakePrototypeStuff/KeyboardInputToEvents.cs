using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputNew;
using Assets.Utilities;

public class KeyboardInputToEvents
	: MonoBehaviour
{
	public void Update()
	{
		int controlCount = EnumHelpers.GetValueCount<KeyCode>();
		for (int i = 0; i < controlCount; i++)
			HandleKey((KeyCode)i, (KeyCode)i);
	}

	void HandleKey(KeyCode keyCode, KeyCode keyControl)
	{
		if (Input.GetKeyDown(keyCode))
			SendKeyboardEvent(keyControl, true);
		if (Input.GetKeyUp(keyCode))
			SendKeyboardEvent(keyControl, false);
	}

	void SendKeyboardEvent(KeyCode key, bool isDown)
	{
		var inputEvent = InputSystem.CreateEvent<KeyboardEvent>();

		inputEvent.deviceType = typeof(Keyboard);
		inputEvent.deviceIndex = 0;
		inputEvent.key = key;
		inputEvent.isDown = isDown;
		////TODO: modifiers

		InputSystem.QueueEvent(inputEvent);
	}
}
