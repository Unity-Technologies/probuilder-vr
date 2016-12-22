using UnityEngine;
using UnityEngine.InputNew;

public class JoystickInputToEvents
	: MonoBehaviour
{
	#region Public Methods

	public void Update()
	{
		SendButtonEvents();
		SendAxisEvents();
	}

	#endregion

	#region Non-Public Methods

	// Fake gamepad has 10 axes (index 0 - 9) and 20 buttons (index 10 - 29).
	public const int axisCount = 10;
	public const int buttonCount = 20;
	public const int joystickCount = 10;
	private float[,] m_LastValues = new float[joystickCount, axisCount + buttonCount];
	
	private void SendAxisEvents()
	{
		int first = 1;
		int last = 10;
		for (int device = 0; device < joystickCount; device++)
		{
			for (int i = 0; i <= last - first; i++)
			{
				var value = Input.GetAxis("Analog" + (i + first) + "_Joy" + (device + 1));
				SendEvent(device, i, value);
			}
		}
	}

	private void SendButtonEvents()
	{
		
		for (int device = 0; device < joystickCount; device++)
		{
			int first = (int)KeyCode.Joystick1Button0 + device * 20;
			int last = (int)KeyCode.Joystick1Button19 + device * 20;
			
			for (int i = 0; i <= last - first; i++)
			{
				if (Input.GetKeyDown((KeyCode)(i + first)))
					SendEvent(device, axisCount + i, 1.0f);
				if (Input.GetKeyUp((KeyCode)(i + first)))
					SendEvent(device, axisCount + i, 0.0f);
			}
		}
	}

	private void SendEvent(int deviceIndex, int controlIndex, float value)
	{
		if (value == m_LastValues[deviceIndex, controlIndex])
			return;
		m_LastValues[deviceIndex, controlIndex] = value;
		
		var inputEvent = InputSystem.CreateEvent<GenericControlEvent>();
		inputEvent.deviceType = typeof(Gamepad);
		inputEvent.deviceIndex = deviceIndex;
		inputEvent.controlIndex = controlIndex;
		inputEvent.value = value;
		InputSystem.QueueEvent(inputEvent);
	}

	#endregion
}
