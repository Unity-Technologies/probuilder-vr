using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Utilities;
using UnityEngine;

namespace UnityEngine.InputNew
{
	public class Keyboard
		: InputDevice
	{
		#region Constructors

		public Keyboard()
			: this("Keyboard", null) { }

		public Keyboard(string deviceName, List<InputControlData> additionalControls)
		{
			this.deviceName = deviceName;
			var controlCount = EnumHelpers.GetValueCount<KeyCode>();
			var controls = Enumerable.Repeat(new InputControlData(), controlCount).ToList();

			for (var i = 0; i < controlCount; ++ i)
			{
				InitKey(controls, (KeyCode)i);
			}

			if (additionalControls != null)
				controls.AddRange(additionalControls);

			SetControls(controls);
		}

		#endregion

		#region Non-Public Methods

		static void InitKey(List<InputControlData> controls, KeyCode key)
		{
			controls[(int)key] = new InputControlData
			{
				name = key.ToString()
				, controlType = typeof(ButtonInputControl)
			};
		}

		#endregion

		#region Public Methods
		
		public static Keyboard current { get { return InputSystem.GetMostRecentlyUsedDevice<Keyboard>(); } }

		public override bool ProcessEventIntoState(InputEvent inputEvent, InputState intoState)
		{
			if (base.ProcessEventIntoState(inputEvent, intoState))
				return true;

			var consumed = false;

			var keyEvent = inputEvent as KeyboardEvent;
			if (keyEvent != null)
				consumed |= intoState.SetCurrentValue((int)keyEvent.key, keyEvent.isDown);

			return consumed;
		}

		#endregion
	}
}
