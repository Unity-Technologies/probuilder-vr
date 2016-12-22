using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Utilities;

namespace UnityEngine.InputNew
{
	public class VRInputDevice
		: InputDevice
	{
		public enum VRControl
		{
			// Standardized.
			Analog0,

			LeftStickX = Analog0,
			LeftStickY,

			Trigger1,
			Trigger2,

            Analog4,
			Analog5,
			Analog6,
			Analog7,
			Analog8,
			Analog9,

			LocalPositionX,
			LocalPositionY,
			LocalPositionZ,

			LocalRotationX,
			LocalRotationY,
			LocalRotationZ,
			LocalRotationW,

			Action1,
			Action2,
			Action3,
			Action4,
            Action5,

		    LeftStickButton,

			// Compound controls.

			LocalPosition,
			LocalRotation,
			LeftStick,

			// Not standardized, but provided for convenience.

			Back,
			Start,
			//Select,
			//Pause,
			//Menu,
			//Share,
			//View,
			//Options
		}

	    private static readonly string[] kTags = { "Left", "Right"};
	    public static string[] Tags
	    {
	        get
	        {
	            return kTags;
	        }
	    }

	    public enum Handedness { Left, Right }

	    private Handedness? hand = null;
	    public Handedness? Hand
	    {
            get { return hand; }
            set { hand = value; }
	    }

		public override int tagIndex
		{
			get
			{
				if (hand.HasValue)
					return (int) hand.Value;
				return -1;
			}
		}

	    public VRInputDevice()
			: this("VRInputDevice", null) {}

		public VRInputDevice(string deviceName, List<InputControlData> additionalControls)
		{
			this.deviceName = deviceName;
			var controlCount = EnumHelpers.GetValueCount<VRControl>();
			var controls = Enumerable.Repeat(new InputControlData(), controlCount).ToList();

			// Compounds.
			controls[(int)VRControl.LeftStick] = new InputControlData
			{
				name = "Left Stick"
				, controlType = typeof(Vector2InputControl)
				, componentControlIndices = new[] { (int)VRControl.LeftStickX, (int)VRControl.LeftStickY }
			};
			controls[(int)VRControl.LocalPosition] = new InputControlData
			{
				name = "Local Position"
				, controlType = typeof(Vector3InputControl)
				, componentControlIndices = new[] { (int)VRControl.LocalPositionX, (int)VRControl.LocalPositionY, (int)VRControl.LocalPositionZ }
			};
			controls[(int)VRControl.LocalRotation] = new InputControlData
			{
				name = "Local Rotation"
				, controlType = typeof(QuaternionInputControl)
				, componentControlIndices = new[] { (int)VRControl.LocalRotationX, (int)VRControl.LocalRotationY, (int)VRControl.LocalRotationZ, (int)VRControl.LocalRotationW }
			};
			////TODO: dpad (more complicated as the source is buttons which need to be translated into a vector)

			// Buttons.
			controls[(int)VRControl.Action1] = new InputControlData { name = "Action 1", controlType = typeof(ButtonInputControl) };
			controls[(int)VRControl.Action2] = new InputControlData { name = "Action 2", controlType = typeof(ButtonInputControl) };
			controls[(int)VRControl.Action3] = new InputControlData { name = "Action 3", controlType = typeof(ButtonInputControl) };
			controls[(int)VRControl.Action4] = new InputControlData { name = "Action 4", controlType = typeof(ButtonInputControl) };
            controls[(int)VRControl.Action5] = new InputControlData { name = "Action 5", controlType = typeof(ButtonInputControl) };

            controls[(int)VRControl.Start] = new InputControlData { name = "Start", controlType = typeof(ButtonInputControl) };
			controls[(int)VRControl.Back] = new InputControlData { name = "Back", controlType = typeof(ButtonInputControl) };
			controls[(int)VRControl.LeftStickButton] = new InputControlData { name = "Left Stick Button", controlType = typeof(ButtonInputControl) };

			// Axes.
			controls[(int)VRControl.LeftStickX] = new InputControlData { name = "Left Stick X", controlType = typeof(AxisInputControl) };
			controls[(int)VRControl.LeftStickY] = new InputControlData { name = "Left Stick Y", controlType = typeof(AxisInputControl) };
			controls[(int)VRControl.LocalPositionX] = new InputControlData { name = "Local Position X", controlType = typeof(AxisInputControl) };
			controls[(int)VRControl.LocalPositionY] = new InputControlData { name = "Local Position Y", controlType = typeof(AxisInputControl) };
			controls[(int)VRControl.LocalPositionZ] = new InputControlData { name = "Local Position Z", controlType = typeof(AxisInputControl) };
			controls[(int)VRControl.LocalRotationX] = new InputControlData { name = "Local Rotation X", controlType = typeof(AxisInputControl) };
			controls[(int)VRControl.LocalRotationY] = new InputControlData { name = "Local Rotation Y", controlType = typeof(AxisInputControl) };
			controls[(int)VRControl.LocalRotationZ] = new InputControlData { name = "Local Rotation Z", controlType = typeof(AxisInputControl) };
			controls[(int)VRControl.LocalRotationW] = new InputControlData { name = "Local Rotation W", controlType = typeof(AxisInputControl) };

			controls[(int)VRControl.Trigger1] = new InputControlData() { name = "Trigger 1", controlType = typeof(AxisInputControl) };
			controls[(int)VRControl.Trigger2] = new InputControlData() { name = "Trigger 2", controlType = typeof(AxisInputControl) };

			if (additionalControls != null)
				controls.AddRange(additionalControls);

			SetControls(controls);
		}

		public AxisInputControl leftStickX { get { return (AxisInputControl)this[(int)VRControl.LeftStickX]; } }
		public AxisInputControl leftStickY { get { return (AxisInputControl)this[(int)VRControl.LeftStickY]; } }
		public ButtonInputControl leftStickButton { get { return (ButtonInputControl)this[(int)VRControl.LeftStickButton]; } }

		public ButtonInputControl action1 { get { return (ButtonInputControl)this[(int)VRControl.Action1]; } }
		public ButtonInputControl action2 { get { return (ButtonInputControl)this[(int)VRControl.Action2]; } }
		public ButtonInputControl action3 { get { return (ButtonInputControl)this[(int)VRControl.Action3]; } }
		public ButtonInputControl action4 { get { return (ButtonInputControl)this[(int)VRControl.Action4]; } }
        public ButtonInputControl action5 { get { return (ButtonInputControl)this[(int)VRControl.Action5]; } }

        public AxisInputControl trigger1 { get { return (AxisInputControl)this[(int)VRControl.Trigger1]; } }
		public AxisInputControl trigger2 { get { return (AxisInputControl)this[(int)VRControl.Trigger2]; } }

		public AxisInputControl localPositionX { get { return (AxisInputControl)this[(int)VRControl.LocalPositionX]; } }
		public AxisInputControl localPositionY { get { return (AxisInputControl)this[(int)VRControl.LocalPositionY]; } }
		public AxisInputControl localPositionZ { get { return (AxisInputControl)this[(int)VRControl.LocalPositionZ]; } }

		public AxisInputControl localRotationX { get { return (AxisInputControl)this[(int)VRControl.LocalRotationX]; } }
		public AxisInputControl localRotationY { get { return (AxisInputControl)this[(int)VRControl.LocalRotationY]; } }
		public AxisInputControl localRotationZ { get { return (AxisInputControl)this[(int)VRControl.LocalRotationZ]; } }
		public AxisInputControl localRotationW { get { return (AxisInputControl)this[(int)VRControl.LocalRotationW]; } }

		// Compound controls.

		public Vector2InputControl leftStick { get { return (Vector2InputControl)this[(int)VRControl.LeftStick]; } }
		public Vector3InputControl localPosition { get { return (Vector3InputControl)this[(int)VRControl.LocalPosition]; } }
		public QuaternionInputControl localRotation { get { return (QuaternionInputControl)this[(int)VRControl.LocalRotation]; } }

		// Not standardized, but provided for convenience.

		public ButtonInputControl back { get { return (ButtonInputControl)this[(int)VRControl.Back]; } }
		public ButtonInputControl start { get { return (ButtonInputControl)this[(int)VRControl.Start]; } }

		public override bool ProcessEventIntoState(InputEvent inputEvent, InputState intoState)
		{

			if (base.ProcessEventIntoState(inputEvent, intoState))
				return true;

			var consumed = false;

			var axisEvent = inputEvent as GenericControlEvent;
			if (axisEvent != null)
				consumed |= intoState.SetCurrentValue((int)VRControl.Analog0 + (int)axisEvent.controlIndex, axisEvent.value);
            
			var trackingEvent = inputEvent as VREvent;
			if (trackingEvent != null)
			{
				consumed |= intoState.SetCurrentValue((int)VRControl.LocalPositionX, trackingEvent.localPosition.x);
				consumed |= intoState.SetCurrentValue((int)VRControl.LocalPositionY, trackingEvent.localPosition.y);
				consumed |= intoState.SetCurrentValue((int)VRControl.LocalPositionZ, trackingEvent.localPosition.z);

				consumed |= intoState.SetCurrentValue((int)VRControl.LocalRotationX, trackingEvent.localRotation.x);
				consumed |= intoState.SetCurrentValue((int)VRControl.LocalRotationY, trackingEvent.localRotation.y);
				consumed |= intoState.SetCurrentValue((int)VRControl.LocalRotationZ, trackingEvent.localRotation.z);
				consumed |= intoState.SetCurrentValue((int)VRControl.LocalRotationW, trackingEvent.localRotation.w);
			}

			return consumed;
		}
	}
}
