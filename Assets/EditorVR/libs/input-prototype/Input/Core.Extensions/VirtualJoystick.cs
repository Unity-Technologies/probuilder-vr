using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Utilities;
using UnityEngine;

namespace UnityEngine.InputNew
{
	public class VirtualJoystick
		: Joystick
	{
		public enum VirtualJoystickControl
		{
			// Standardized.

			LeftStickX,
			LeftStickY,

			RightStickX,
			RightStickY,

			Action1,
			Action2,
			Action3,
			Action4,

			// Compound controls.

			LeftStick,
			RightStick,

			// Not standardized, but provided for convenience.

			Back,
			Start,
			Select,
			Pause,
			Menu,
			Share,
			View,
			Options
		}

		#region Constructors

		public VirtualJoystick()
			: this("Virtual Joystick", null) { }

		public VirtualJoystick(string deviceName, List<InputControlData> additionalControls)
		{
			this.deviceName = deviceName;
			var controlCount = EnumHelpers.GetValueCount<VirtualJoystickControl>();
			var controls = new List<InputControlData>(controlCount);
			for (int i = 0; i < controlCount; i++)
				controls.Add(new InputControlData());
			
			// Compounds.
			controls[(int)VirtualJoystickControl.LeftStick] = new InputControlData
			{
				name = "Left Stick"
				, controlType = typeof(Vector2InputControl)
				, componentControlIndices = new[] { (int)VirtualJoystickControl.LeftStickX, (int)VirtualJoystickControl.LeftStickY }
			};
			controls[(int)VirtualJoystickControl.RightStick] = new InputControlData
			{
				name = "Right Stick"
				, controlType = typeof(Vector2InputControl)
				, componentControlIndices = new[] { (int)VirtualJoystickControl.RightStickX, (int)VirtualJoystickControl.RightStickY }
			};
			
			// Axes.
			controls[(int)VirtualJoystickControl.LeftStickX] = new InputControlData { name = "Left Stick X", controlType = typeof(AxisInputControl) };
			controls[(int)VirtualJoystickControl.LeftStickY] = new InputControlData { name = "Left Stick Y", controlType = typeof(AxisInputControl) };
			controls[(int)VirtualJoystickControl.RightStickX] = new InputControlData { name = "Right Stick X", controlType = typeof(AxisInputControl) };
			controls[(int)VirtualJoystickControl.RightStickY] = new InputControlData { name = "Right Stick Y", controlType = typeof(AxisInputControl) };
			
			// Buttons.
			controls[(int)VirtualJoystickControl.Action1] = new InputControlData { name = "Action 1", controlType = typeof(ButtonInputControl) };
			controls[(int)VirtualJoystickControl.Action2] = new InputControlData { name = "Action 2", controlType = typeof(ButtonInputControl) };
			controls[(int)VirtualJoystickControl.Action3] = new InputControlData { name = "Action 3", controlType = typeof(ButtonInputControl) };
			controls[(int)VirtualJoystickControl.Action4] = new InputControlData { name = "Action 4", controlType = typeof(ButtonInputControl) };
			
			controls[(int)VirtualJoystickControl.Back] = new InputControlData { name = "Back", controlType = typeof(ButtonInputControl) };
			controls[(int)VirtualJoystickControl.Start] = new InputControlData { name = "Start", controlType = typeof(ButtonInputControl) };
			controls[(int)VirtualJoystickControl.Select] = new InputControlData { name = "Select", controlType = typeof(ButtonInputControl) };
			controls[(int)VirtualJoystickControl.Pause] = new InputControlData { name = "Pause", controlType = typeof(ButtonInputControl) };
			controls[(int)VirtualJoystickControl.Menu] = new InputControlData { name = "Menu", controlType = typeof(ButtonInputControl) };
			controls[(int)VirtualJoystickControl.Share] = new InputControlData { name = "Share", controlType = typeof(ButtonInputControl) };
			controls[(int)VirtualJoystickControl.View] = new InputControlData { name = "View", controlType = typeof(ButtonInputControl) };
			controls[(int)VirtualJoystickControl.Options] = new InputControlData { name = "Options", controlType = typeof(ButtonInputControl) };
			
			if (additionalControls != null)
				controls.AddRange(additionalControls);
			
			SetControls(controls);
		}

		#endregion

		public static VirtualJoystick current { get { return InputSystem.GetMostRecentlyUsedDevice<VirtualJoystick>(); } }
		
		public void SetAxisValue(int controlIndex, float value)
		{
			var control = this[controlIndex] as InputControl;
			if (control == null)
				return;
			float currentValue = control.rawValue;
			if (value == currentValue)
				return;
			
			var inputEvent = InputSystem.CreateEvent<GenericControlEvent>();
			inputEvent.deviceType = typeof(VirtualJoystick);
			inputEvent.deviceIndex = 0; // TODO: Use index of device itself, but that's not currently stored on device.
			inputEvent.controlIndex = controlIndex;
			inputEvent.value = value;
			InputSystem.QueueEvent(inputEvent);
		}
		
		public void SetButtonValue(int controlIndex, bool state)
		{
			SetAxisValue(controlIndex, state ? 1 : 0);
		}

		public InputControl leftStickX { get { return this[(int)VirtualJoystickControl.LeftStickX]; } }
		public InputControl leftStickY { get { return this[(int)VirtualJoystickControl.LeftStickY]; } }

		public InputControl rightStickX { get { return this[(int)VirtualJoystickControl.RightStickX]; } }
		public InputControl rightStickY { get { return this[(int)VirtualJoystickControl.RightStickY]; } }

		public InputControl action1 { get { return this[(int)VirtualJoystickControl.Action1]; } }
		public InputControl action2 { get { return this[(int)VirtualJoystickControl.Action2]; } }
		public InputControl action3 { get { return this[(int)VirtualJoystickControl.Action3]; } }
		public InputControl action4 { get { return this[(int)VirtualJoystickControl.Action4]; } }

		// Compound controls.

		public InputControl leftStick { get { return this[(int)VirtualJoystickControl.LeftStick]; } }
		public InputControl rightStick { get { return this[(int)VirtualJoystickControl.RightStick]; } }

		// Not standardized, but provided for convenience.

		public InputControl back { get { return this[(int)VirtualJoystickControl.Back]; } }
		public InputControl start { get { return this[(int)VirtualJoystickControl.Start]; } }
		public InputControl select { get { return this[(int)VirtualJoystickControl.Select]; } }
		public InputControl pause { get { return this[(int)VirtualJoystickControl.Pause]; } }
		public InputControl menu { get { return this[(int)VirtualJoystickControl.Menu]; } }
		public InputControl share { get { return this[(int)VirtualJoystickControl.Share]; } }
		public InputControl view { get { return this[(int)VirtualJoystickControl.View]; } }
		public InputControl options { get { return this[(int)VirtualJoystickControl.Options]; } }
	}
}
