using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	// ------------------------------------------------------------------------
	//	Events.
	// ------------------------------------------------------------------------

	public abstract class InputEvent
	{
		#region Public Methods

		public override string ToString()
		{
			if (deviceType == null)
				return base.ToString ();

			return string.Format
				(
					"{0} on {1}, {2}, time:{3}"
					, GetType().Name
					, deviceType.Name
					, deviceIndex
					, time.ToString("0.00")
				);
		}

		#endregion

		internal void Reset()
		{
			time = 0.0f;
			deviceType = null;
			deviceIndex = 0;
		}

		#region Public Properties

		public float time { get; set; }
		public Type deviceType
		{
			get { return m_DeviceType; }
			set { m_DeviceType = value; m_CachedDevice = null; }
		}
		public int deviceIndex
		{
			get { return m_DeviceIndex; }
			set { m_DeviceIndex = value; m_CachedDevice = null; }
		}

		public InputDevice device
		{
			get
			{
				if (m_CachedDevice == null && deviceType != null)
					m_CachedDevice = InputSystem.LookupDevice(deviceType, deviceIndex);

				return m_CachedDevice;
			}
			set
			{
				deviceIndex = value.deviceIndex;
				deviceType = value.GetType();
			}
		}

		#endregion

		private Type m_DeviceType;
		private int m_DeviceIndex;
		private InputDevice m_CachedDevice;
	}
}

// -------- from old single file thing

////REVIEW: we may want to store actual state for compounds such that we can do postprocessing on them (like normalize vectors, for example)

// ------------------------------------------------------------------------
//	Devices.
// ------------------------------------------------------------------------

////TODO: how deal with compound devices (e.g. gamepads that also have a touchscreen)?
////	create a true CompoundDevice class that is a collection of InputDevices?

////FIXME: currently compounds go in the same array as primitives and thus lead to allocation of state which is useless for them

////REVIEW: have a single Pointer class representing the union of all types of pointer devices or have multiple specific subclasses?
////	also: where to keep the state for "the one" pointer

// ------------------------------------------------------------------------
//	Bindings.
// ------------------------------------------------------------------------

// Three different naming approaches:
// 1. ActionMap, ActionMapEntry
// 2. InputActionMap, InputAction
// 3. InputActivityMap, InputActivity

////NOTE: this needs to be proper asset stuff; can't be done in script code only

// ------------------------------------------------------------------------
//	System.
// ------------------------------------------------------------------------
