using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	public abstract class InputDevice
		: InputControlProvider
	{
		#region Constructors

		protected InputDevice(string deviceName, List<InputControlData> controls)
		{
			SetControls(controls);
			this.deviceName = deviceName;
			deviceIndex = InputSystem.GetNewDeviceIndex(this);
		}

		protected InputDevice()
		{
			this.deviceName = "Generic Input Device";
			deviceIndex = InputSystem.GetNewDeviceIndex(this);
		}

		#endregion

		#region Public Methods

		////REVIEW: right now the devices don't check whether the event was really meant for them; they go purely by the
		////  type of event they receive. should they check more closely?
		
		public override sealed bool ProcessEvent(InputEvent inputEvent)
		{
			// If event was used, set time, but never consume event.
			// Devices don't consume events.
			if (ProcessEventIntoState(inputEvent, state))
				lastEventTime = inputEvent.time;
			return false;
		}

		public virtual bool ProcessEventIntoState(InputEvent inputEvent, InputState intoState)
		{
			GenericControlEvent controlEvent = inputEvent as GenericControlEvent;
			if (controlEvent == null)
				return false;

			return intoState.SetCurrentValue(controlEvent.controlIndex, controlEvent.value);
		}

		public virtual bool RemapEvent(InputEvent inputEvent)
		{
			if (profile != null)
				profile.Remap(inputEvent);
			return false;
		}
		
		private void SetNameOverrides()
		{
			if (profile == null)
				return;
			
			// Assign control override names
			for (int i = 0; i < controlCount; i++) {
				string nameOverride = profile.GetControlNameOverride(i);
				if (nameOverride != null)
					SetControlNameOverride(i, nameOverride);
			}
		}

		#endregion

		#region Public Properties

		public bool connected { get; internal set; }

		public InputDeviceProfile profile
		{
			get { return m_Profile; } set { m_Profile = value; SetNameOverrides(); }
		}

		// Some input providers need an identifier tag when there are multiple devices of the same type (e.g. left and right hands)
		public virtual int tagIndex
		{
			get { return -1; } // -1 tag means unset or "Any"
		}

		public string deviceName { get; protected set; }
		public int deviceIndex { get; private set; }

		public PlayerDeviceAssignment assignment
		{
			get
			{
				return m_Assignment;
			}
			set
			{
				m_Assignment = value;
			}
		}

		public override string ToString ()
		{
			return (deviceName ?? GetType().Name) + " " + deviceIndex;
		}

		#endregion
		
		private InputDeviceProfile m_Profile;
		private PlayerDeviceAssignment m_Assignment = null;
	}
}
