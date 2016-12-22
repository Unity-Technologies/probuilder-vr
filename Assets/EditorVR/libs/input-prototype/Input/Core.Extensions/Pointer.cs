using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	/// <summary>
	///     A device that can point at and click on things.
	/// </summary>
	public class Pointer
		: InputDevice
	{
		#region Constructors

		public Pointer()
			: this("Pointer", null) { }
		
		protected Pointer(string deviceName, List<InputControlData> additionalControls)
		{
			this.deviceName = deviceName;
			var controls = new List<InputControlData>();
			
			controls.Add(item: new InputControlData
			{
				name = "Position"
					, controlType = typeof(Vector3InputControl)
					, componentControlIndices = new[] { (int)PointerControl.PositionX, (int)PointerControl.PositionY, (int)PointerControl.PositionZ }
			});
			
			controls.Add(new InputControlData { name = "Position X", controlType = typeof(AxisInputControl) });
			controls.Add(new InputControlData { name = "Position Y", controlType = typeof(AxisInputControl) });
			controls.Add(new InputControlData { name = "Position Z", controlType = typeof(AxisInputControl) });
			
			controls.Add(item: new InputControlData
			{
				name = "Delta"
					, controlType = typeof(Vector3InputControl)
					, componentControlIndices = new[] { (int)PointerControl.DeltaX, (int)PointerControl.DeltaY, (int)PointerControl.DeltaZ }
			});
			
			controls.Add(new InputControlData { name = "Delta X", controlType = typeof(AxisInputControl) });
			controls.Add(new InputControlData { name = "Delta Y", controlType = typeof(AxisInputControl) });
			controls.Add(new InputControlData { name = "Delta Z", controlType = typeof(AxisInputControl) });
			controls.Add(item: new InputControlData
			{
				name = "Locked Delta"
					, controlType = typeof(Vector3InputControl)
					, componentControlIndices = new[] { (int)PointerControl.LockedDeltaX, (int)PointerControl.LockedDeltaY, (int)PointerControl.LockedDeltaZ }
			});
			controls.Add(new InputControlData { name = "Locked Delta X", controlType = typeof(AxisInputControl) });
			controls.Add(new InputControlData { name = "Locked Delta Y", controlType = typeof(AxisInputControl) });
			controls.Add(new InputControlData { name = "Locked Delta Z", controlType = typeof(AxisInputControl) });
			controls.Add(new InputControlData { name = "Pressure", controlType = typeof(AxisInputControl) });
			controls.Add(new InputControlData { name = "Tilt", controlType = typeof(AxisInputControl) });
			controls.Add(new InputControlData { name = "Rotation", controlType = typeof(AxisInputControl) });
			controls.Add(new InputControlData { name = "Left Button", controlType = typeof(ButtonInputControl) });
			controls.Add(new InputControlData { name = "Right Button", controlType = typeof(ButtonInputControl) });
			controls.Add(new InputControlData { name = "Middle Button", controlType = typeof(ButtonInputControl) });

			if (additionalControls != null)
				controls.AddRange(additionalControls);
			
			SetControls(controls);
		}

		#endregion

		#region Public Methods
		
		public static Pointer current { get { return InputSystem.GetMostRecentlyUsedDevice<Pointer>(); } }

		public override bool ProcessEventIntoState(InputEvent inputEvent, InputState intoState)
		{
			if (base.ProcessEventIntoState(inputEvent, intoState))
				return true;

			var consumed = false;

			var moveEvent = inputEvent as PointerMoveEvent;
			if (moveEvent != null)
			{
				consumed |= intoState.SetCurrentValue((int)PointerControl.PositionX, moveEvent.position.x);
				consumed |= intoState.SetCurrentValue((int)PointerControl.PositionY, moveEvent.position.y);
				consumed |= intoState.SetCurrentValue((int)PointerControl.PositionZ, moveEvent.position.z);

				consumed |= intoState.SetCurrentValue((int)PointerControl.DeltaX, moveEvent.delta.x);
				consumed |= intoState.SetCurrentValue((int)PointerControl.DeltaY, moveEvent.delta.y);
				consumed |= intoState.SetCurrentValue((int)PointerControl.DeltaZ, moveEvent.delta.z);

				if (cursor == null || cursor.isLocked)
				{
					consumed |= intoState.SetCurrentValue((int)PointerControl.LockedDeltaX, moveEvent.delta.x);
					consumed |= intoState.SetCurrentValue((int)PointerControl.LockedDeltaY, moveEvent.delta.y);
					consumed |= intoState.SetCurrentValue((int)PointerControl.LockedDeltaZ, moveEvent.delta.z);
				}
				else
				{
					intoState.SetCurrentValue((int)PointerControl.LockedDeltaX, 0.0f);
					intoState.SetCurrentValue((int)PointerControl.LockedDeltaY, 0.0f);
					intoState.SetCurrentValue((int)PointerControl.LockedDeltaZ, 0.0f);
				}

				return consumed;
			}

			var clickEvent = inputEvent as GenericControlEvent;
			if (clickEvent != null)
			{
				switch ((PointerControl)clickEvent.controlIndex)
				{
				case PointerControl.LeftButton:
					consumed |= intoState.SetCurrentValue((int)PointerControl.LeftButton, clickEvent.value);
					break;
				case PointerControl.MiddleButton:
					consumed |= intoState.SetCurrentValue((int)PointerControl.MiddleButton, clickEvent.value);
					break;
				case PointerControl.RightButton:
					consumed |= intoState.SetCurrentValue((int)PointerControl.RightButton, clickEvent.value);
					break;
				}

				return consumed;
			}

			return false;
		}

		#endregion

		#region Public Properties

		public Vector3 position
		{
			get { return ((Vector3InputControl)this[(int)PointerControl.Position]).vector3; }
		}

		public float pressure
		{
			get { return ((AxisInputControl)this[(int)PointerControl.Pressure]).value; }
		}

		////REVIEW: okay, maybe the concept of a per-pointer cursor is bogus after all...
		public Cursor cursor { get; protected set; }

		#endregion
	}

}
