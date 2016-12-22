using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputNew;
using UnityEngine.VR;

public class VRInputToEvents
	: MonoBehaviour
{
#if USE_NATIVE_INPUT
    public void Update()
	{
		SendButtonEvents();
		SendAxisEvents();
		SendTrackingEvents();
	}

	public const int controllerCount = 10;
	public const int buttonCount = 64;
	public const int axisCount = 10;
	private float [,] m_LastAxisValues = new float[controllerCount,axisCount];
	private Vector3[] m_LastPositionValues = new Vector3[controllerCount];
	private Quaternion[] m_LastRotationValues = new Quaternion[controllerCount];

	private void SendAxisEvents()
	{
		for (int device = 0; device < controllerCount; ++device)
		{
			for (int axis = 0; axis < axisCount; ++axis)
			{
				var inputEvent = InputSystem.CreateEvent<GenericControlEvent>();
				inputEvent.deviceType = typeof (VRInputDevice);
				inputEvent.deviceIndex = device;
				inputEvent.controlIndex = axis;
				inputEvent.value = UnityEngine.VR.InputTracking.GetAxis(device, axis);

				if (Mathf.Approximately(m_LastAxisValues[device, axis], inputEvent.value))
					continue;
				m_LastAxisValues[device, axis] = inputEvent.value;

				Debug.Log("Axis event: " + inputEvent);

				InputSystem.QueueEvent(inputEvent);
			}
		}
	}

	private void SendButtonEvents()
	{
		for (int device = 0; device < controllerCount; ++device)
		{
			for (int btn = 0; btn < buttonCount; ++btn)
			{
				bool keyDown = UnityEngine.VR.InputTracking.GetKeyDown(device, btn);
				bool keyUp = UnityEngine.VR.InputTracking.GetKeyUp(device, btn);

				if (keyDown || keyUp)
				{
					var inputEvent = InputSystem.CreateEvent<GenericControlEvent>();
					inputEvent.deviceType = typeof(VRInputDevice);
					inputEvent.deviceIndex = device;
					inputEvent.controlIndex = axisCount + btn;
					inputEvent.value = keyDown ? 1.0f : 0.0f;

					InputSystem.QueueEvent(inputEvent);
				}

				//bool keyDown = UnityEngine.VR.InputTracking.GetKeyDown(device, btn);
				//bool keyUp = UnityEngine.VR.InputTracking.GetKeyUp(device, btn);
				//if (keyDown || keyUp)
				//{
				//	var inputEvent = InputSystem.CreateEvent<KeyboardEvent>();
				//	inputEvent.deviceType = typeof(VRInputDevice);
				//	inputEvent.deviceIndex = device;
				//	inputEvent.key = (UnityEngine.KeyCode)btn;
				//	inputEvent.isDown = keyDown;

				//	InputSystem.QueueEvent(inputEvent);
				//}
			}
		}
	}

	private void SendTrackingEvents()
	{
		for (int device = 0; device < controllerCount; ++device)
		{
			var inputEvent = InputSystem.CreateEvent<VREvent>();
			inputEvent.deviceType = typeof (VRInputDevice);
			inputEvent.deviceIndex = device;
			inputEvent.localPosition = UnityEngine.VR.InputTracking.GetLocalPosition((VRNode) device);
			inputEvent.localRotation = UnityEngine.VR.InputTracking.GetLocalRotation((VRNode) device);

			if (inputEvent.localPosition == m_LastPositionValues[device] &&
				inputEvent.localRotation == m_LastRotationValues[device])
				continue;

			m_LastPositionValues[device] = inputEvent.localPosition;
			m_LastRotationValues[device] = inputEvent.localRotation;

			InputSystem.QueueEvent(inputEvent);
		}
	}
#endif
}