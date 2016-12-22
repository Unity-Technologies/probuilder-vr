using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Utilities;
using UnityEngine;

namespace UnityEngine.InputNew
{
	public class Gamepad
		: Joystick
	{
		enum GamepadControl
		{
			// Standardized.

			LeftStickX,
			LeftStickY,
			LeftStickButton,

			RightStickX,
			RightStickY,
			RightStickButton,

			DPadLeft,
			DPadRight,
			DPadUp,
			DPadDown,

			Action1,
			Action2,
			Action3,
			Action4,

			LeftTrigger,
			RightTrigger,

			LeftBumper,
			RightBumper,

			// Compound controls.

			LeftStick,
			RightStick,
			DPad,

			// Not standardized, but provided for convenience.

			Back,
			Start,
			Select,
			System,
			Pause,
			Menu,
			Share,
			View,
			Options,
			TiltX,
			TiltY,
			TiltZ,
			ScrollWheel,
			TouchPadTap,
			TouchPadXAxis,
			TouchPadYAxis,

			// Not standardized.

			Analog0,
			Analog1,
			Analog2,
			Analog3,
			Analog4,
			Analog5,
			Analog6,
			Analog7,
			Analog8,
			Analog9,
			Analog10,
			Analog11,
			Analog12,
			Analog13,
			Analog14,
			Analog15,
			Analog16,
			Analog17,
			Analog18,
			Analog19,

			Button0,
			Button1,
			Button2,
			Button3,
			Button4,
			Button5,
			Button6,
			Button7,
			Button8,
			Button9,
			Button10,
			Button11,
			Button12,
			Button13,
			Button14,
			Button15,
			Button16,
			Button17,
			Button18,
			Button19,
		}

		#region Constructors

		public Gamepad()
			: this("Gamepad", null) {}

		public Gamepad(string deviceName, List<InputControlData> additionalControls)
		{
			this.deviceName = deviceName;
			var controlCount = EnumHelpers.GetValueCount<GamepadControl>();
			var controls = Enumerable.Repeat(new InputControlData(), controlCount).ToList();

			// Compounds.
			controls[(int)GamepadControl.LeftStick] = new InputControlData
			{
				name = "Left Stick"
				, controlType = typeof(Vector2InputControl)
				, componentControlIndices = new[] { (int)GamepadControl.LeftStickX, (int)GamepadControl.LeftStickY }
			};
			controls[(int)GamepadControl.RightStick] = new InputControlData
			{
				name = "Right Stick"
				, controlType = typeof(Vector2InputControl)
				, componentControlIndices = new[] { (int)GamepadControl.RightStickX, (int)GamepadControl.RightStickY }
			};
			////TODO: dpad (more complicated as the source is buttons which need to be translated into a vector)

			// Buttons.
			controls[(int)GamepadControl.Action1] = new InputControlData { name = "Action 1", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.Action2] = new InputControlData { name = "Action 2", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.Action3] = new InputControlData { name = "Action 3", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.Action4] = new InputControlData { name = "Action 4", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.LeftStickButton] = new InputControlData { name = "Left Stick Button", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.RightStickButton] = new InputControlData { name = "Right Stick Button", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.DPadUp] = new InputControlData { name = "DPad Up", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.DPadDown] = new InputControlData { name = "DPad Down", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.DPadLeft] = new InputControlData { name = "DPad Left", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.DPadRight] = new InputControlData { name = "DPad Right", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.LeftBumper] = new InputControlData { name = "Left Bumper", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.RightBumper] = new InputControlData { name = "Right Bumper", controlType = typeof(ButtonInputControl) };

			controls[(int)GamepadControl.Start] = new InputControlData { name = "Start", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.Back] = new InputControlData { name = "Back", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.Select] = new InputControlData { name = "Select", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.System] = new InputControlData { name = "System", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.Pause] = new InputControlData { name = "Pause", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.Menu] = new InputControlData { name = "Menu", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.Share] = new InputControlData { name = "Share", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.View] = new InputControlData { name = "View", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.Options] = new InputControlData { name = "Options", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.TiltX] = new InputControlData { name = "TiltX", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.TiltY] = new InputControlData { name = "TiltY", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.TiltZ] = new InputControlData { name = "TiltZ", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.ScrollWheel] = new InputControlData { name = "ScrollWheel", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.TouchPadTap] = new InputControlData { name = "TouchPadTap", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.TouchPadXAxis] = new InputControlData { name = "TouchPadXAxis", controlType = typeof(ButtonInputControl) };
			controls[(int)GamepadControl.TouchPadYAxis] = new InputControlData { name = "TouchPadYAxis", controlType = typeof(ButtonInputControl) };

			// Axes.
			controls[(int)GamepadControl.LeftStickX] = new InputControlData { name = "Left Stick X", controlType = typeof(AxisInputControl) };
			controls[(int)GamepadControl.LeftStickY] = new InputControlData { name = "Left Stick Y", controlType = typeof(AxisInputControl) };
			controls[(int)GamepadControl.RightStickX] = new InputControlData { name = "Right Stick X", controlType = typeof(AxisInputControl) };
			controls[(int)GamepadControl.RightStickY] = new InputControlData { name = "Right Stick Y", controlType = typeof(AxisInputControl) };
			controls[(int)GamepadControl.LeftTrigger] = new InputControlData { name = "Left Trigger", controlType = typeof(AxisInputControl) };
			controls[(int)GamepadControl.RightTrigger] = new InputControlData { name = "Right Trigger", controlType = typeof(AxisInputControl) };

			if (additionalControls != null)
				controls.AddRange(additionalControls);

			SetControls(controls);
		}

		#endregion

		public AxisInputControl leftStickX { get { return (AxisInputControl)this[(int)GamepadControl.LeftStickX]; } }
		public AxisInputControl leftStickY { get { return (AxisInputControl)this[(int)GamepadControl.LeftStickY]; } }
		public ButtonInputControl leftStickButton { get { return (ButtonInputControl)this[(int)GamepadControl.LeftStickButton]; } }

		public AxisInputControl rightStickX { get { return (AxisInputControl)this[(int)GamepadControl.RightStickX]; } }
		public AxisInputControl rightStickY { get { return (AxisInputControl)this[(int)GamepadControl.RightStickY]; } }
		public ButtonInputControl rightStickButton { get { return (ButtonInputControl)this[(int)GamepadControl.RightStickButton]; } }

		public ButtonInputControl dPadLeft { get { return (ButtonInputControl)this[(int)GamepadControl.DPadLeft]; } }
		public ButtonInputControl dPadRight { get { return (ButtonInputControl)this[(int)GamepadControl.DPadRight]; } }
		public ButtonInputControl dPadUp { get { return (ButtonInputControl)this[(int)GamepadControl.DPadUp]; } }
		public ButtonInputControl dPadDown { get { return (ButtonInputControl)this[(int)GamepadControl.DPadDown]; } }

		public ButtonInputControl action1 { get { return (ButtonInputControl)this[(int)GamepadControl.Action1]; } }
		public ButtonInputControl action2 { get { return (ButtonInputControl)this[(int)GamepadControl.Action2]; } }
		public ButtonInputControl action3 { get { return (ButtonInputControl)this[(int)GamepadControl.Action3]; } }
		public ButtonInputControl action4 { get { return (ButtonInputControl)this[(int)GamepadControl.Action4]; } }

		public AxisInputControl leftTrigger { get { return (AxisInputControl)this[(int)GamepadControl.LeftTrigger]; } }
		public AxisInputControl rightTrigger { get { return (AxisInputControl)this[(int)GamepadControl.RightTrigger]; } }

		public ButtonInputControl leftBumper { get { return (ButtonInputControl)this[(int)GamepadControl.LeftBumper]; } }
		public ButtonInputControl rightBumper { get { return (ButtonInputControl)this[(int)GamepadControl.RightBumper]; } }

		// Compound controls.

		public Vector2InputControl leftStick { get { return (Vector2InputControl)this[(int)GamepadControl.LeftStick]; } }
		public Vector2InputControl rightStick { get { return (Vector2InputControl)this[(int)GamepadControl.RightStick]; } }
		public Vector2InputControl dPad { get { return (Vector2InputControl)this[(int)GamepadControl.DPad]; } }

		// Not standardized, but provided for convenience.

		public ButtonInputControl back { get { return (ButtonInputControl)this[(int)GamepadControl.Back]; } }
		public ButtonInputControl start { get { return (ButtonInputControl)this[(int)GamepadControl.Start]; } }
		public ButtonInputControl select { get { return (ButtonInputControl)this[(int)GamepadControl.Select]; } }
		public ButtonInputControl system { get { return (ButtonInputControl)this[(int)GamepadControl.System]; } }
		public ButtonInputControl pause { get { return (ButtonInputControl)this[(int)GamepadControl.Pause]; } }
		public ButtonInputControl menu { get { return (ButtonInputControl)this[(int)GamepadControl.Menu]; } }
		public ButtonInputControl share { get { return (ButtonInputControl)this[(int)GamepadControl.Share]; } }
		public ButtonInputControl view { get { return (ButtonInputControl)this[(int)GamepadControl.View]; } }
		public ButtonInputControl options { get { return (ButtonInputControl)this[(int)GamepadControl.Options]; } }
		public AxisInputControl tiltX { get { return (AxisInputControl)this[(int)GamepadControl.TiltX]; } }
		public AxisInputControl tiltY { get { return (AxisInputControl)this[(int)GamepadControl.TiltY]; } }
		public AxisInputControl tiltZ { get { return (AxisInputControl)this[(int)GamepadControl.TiltZ]; } }
		public AxisInputControl scrollWheel { get { return (AxisInputControl)this[(int)GamepadControl.ScrollWheel]; } }
		public ButtonInputControl touchPadTap { get { return (ButtonInputControl)this[(int)GamepadControl.TouchPadTap]; } }
		public AxisInputControl touchPadXAxis { get { return (AxisInputControl)this[(int)GamepadControl.TouchPadXAxis]; } }
		public AxisInputControl touchPadYAxis { get { return (AxisInputControl)this[(int)GamepadControl.TouchPadYAxis]; } }
	}
}
