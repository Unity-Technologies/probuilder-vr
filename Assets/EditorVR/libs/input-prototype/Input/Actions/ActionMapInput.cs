using System;
using System.Linq;
using System.Collections.Generic;

namespace UnityEngine.InputNew
{
	/*
	Things to test for action map / control schemes.

	- When pressing e.g. mouse button or gamepad trigger in one action map creates a new action map
	  based on the same device, the new action map should not immediately have wasJustPressed be true.
	  Hence the state in the newly created control scheme should be initialized to the state
	  of the devices it's based on.
	
	- When pressing e.g. mouse button or gamepad trigger and it causes a switch in control scheme
	  within an existing action map, the new control scheme *should* immediately have wasJustPressed be true.
	  Hence the state in the newly created control scheme should not be initialized to the state
	  of the devices it's based on.

	*/
	public class ActionMapInput : InputControlProvider
	{
		private ActionMap m_ActionMap;
		public ActionMap actionMap { get { return m_ActionMap; } }

		private ControlScheme m_ControlScheme;
		public ControlScheme controlScheme { get { return m_ControlScheme; } }

		private List<InputState> m_DeviceStates = new List<InputState>();
		private List<InputState> deviceStates { get { return m_DeviceStates; } }

		bool m_Active;
		public override bool active {
			get {
				return m_Active;
			}
			set {
				if (m_Active == value)
					return;

				m_Active = value;

				if (onStatusChange != null)
					onStatusChange.Invoke();
			}
		}

		/// <summary>
		/// Control whether this ActionMapInput will attempt to reinitialize with applicable devices in order to process events
		/// </summary>
		public bool autoReinitialize { get; set; }

		public bool blockSubsequent { get; set; }

		public delegate void ChangeEvent();
		public static ChangeEvent onStatusChange;

		public static ActionMapInput Create(ActionMap actionMap)
		{
			ActionMapInput map =
				(ActionMapInput)Activator.CreateInstance(actionMap.customActionMapType, new object[] { actionMap });
			map.autoReinitialize = true;
			return map;
		}

		protected ActionMapInput(ActionMap actionMap)
		{
			autoReinitialize = true;
			m_ActionMap = actionMap;

			// Create list of controls from ActionMap.
			////REVIEW: doesn't handle compounds
			var controls = new List<InputControlData>();
			foreach (var entry in actionMap.actions)
				controls.Add(entry.controlData);
			SetControls(controls);
		}

		/// <summary>
		/// Find the best control scheme for the available devices and initialize the action map input.
		/// 
		/// It's important to note that an action map that can use either of two devices should have two of the same device
		/// listed in the control scheme. Otherwise, if the ActionMapInput is initialized with those two devices, the first
		/// device state found will override the other's device state. This becomes apparent when GetDeviceStateForDeviceSlot
		/// is called. 
		///
		/// Ex. Having a left and right VR controller where the action map can accept either controller's trigger button 
		/// would cause issues if only one device was listed in the action map. Usually, this shows up as a ping-ponging
		/// issue where the ActionMapInput keeps getting re-initialized and binds different devices.
		/// </summary>
		/// <param name="availableDevices">Available devices in the system</param>
		/// <param name="requiredDevice">Required device for scheme</param>
		/// <returns></returns>
		public bool TryInitializeWithDevices(IEnumerable<InputDevice> availableDevices, IEnumerable<InputDevice> requiredDevices = null)
		{
			int bestScheme = -1;
			List<InputDevice> bestFoundDevices = null;
			float mostRecentTime = -1;

			List<InputDevice> foundDevices = new List<InputDevice>();
			for (int scheme = 0; scheme < actionMap.controlSchemes.Count; scheme++)
			{
				float timeForScheme = -1;
				foundDevices.Clear();
				var deviceSlots = actionMap.controlSchemes[scheme].deviceSlots;
				bool matchesAll = true;
				foreach (var deviceSlot in deviceSlots)
				{
					InputDevice foundDevice = null;
					float foundDeviceTime = -1;
					foreach (var device in availableDevices)
					{
						if (deviceSlot.type.value.IsInstanceOfType(device) && device.lastEventTime > foundDeviceTime
							&& (deviceSlot.tagIndex == -1 || deviceSlot.tagIndex == device.tagIndex)
							)
						{
							foundDevice = device;
							foundDeviceTime = device.lastEventTime;
						}
					}
					if (foundDevice != null)
					{
						foundDevices.Add(foundDevice);
						timeForScheme = Mathf.Max(timeForScheme, foundDeviceTime);
					}
					else
					{
						matchesAll = false;
						break;
					}
				}

				// Don't switch schemes in the case where we require a specific device for an event that is getting processed.
				if (matchesAll && requiredDevices != null && requiredDevices.Any())
				{
					foreach (var device in requiredDevices)
					{
						if (!foundDevices.Contains(device))
						{
							matchesAll = false;
							break;
						}
					}
				}

				if (!matchesAll)
					continue;

				// If we reach this point we know that control scheme both matches required and matches all.
				if (timeForScheme > mostRecentTime)
				{
					bestScheme = scheme;
					bestFoundDevices = new List<InputDevice>(foundDevices);
					mostRecentTime = timeForScheme;
				}
			}

			if (bestScheme == -1)
				return false;
			
			ControlScheme matchingControlScheme = actionMap.controlSchemes[bestScheme];
			Assign(matchingControlScheme, bestFoundDevices);
			return true;
		}

		private void Assign(ControlScheme controlScheme, List<InputDevice> devices)
		{
			m_ControlScheme = controlScheme;

			// Create state for every device.
			var deviceStates = new List<InputState>();
			foreach (var device in devices)
				deviceStates.Add(new InputState(device));
			m_DeviceStates = deviceStates;
			RefreshBindings();

			ResetControlsForCurrentReceivers();

			Reset();

			if (onStatusChange != null)
				onStatusChange.Invoke();
		}

		private void ResetControlsForCurrentReceivers()
		{
			// Set state to inactive temporarily, so subsequent ActionMaps receive the reset events.
			bool oldActiveState = m_Active;
			m_Active = false;

			for (int i = 0; i < m_DeviceStates.Count; i++)
			{
				var state = m_DeviceStates[i];
				for (int j = 0; j < state.count; j++)
				{
					if (blockSubsequent || state.IsControlEnabled(j))
					{
						GenericControlEvent evt = new GenericControlEvent()
						{
							device = state.controlProvider as InputDevice,
							controlIndex = j,
							value = state.controlProvider.GetControlData(j).defaultValue,
							time = Time.time
						};
						InputSystem.consumers.ProcessEvent(evt);
					}
				}
			}

			m_Active = oldActiveState;
		}

		/// <summary>
		/// Reset controls that share the same source (potentially from another AMI)
		/// </summary>
		/// <param name="control"></param>
		public void ResetControl(InputControl control)
		{
			var otherAMI = control.provider as ActionMapInput;
			var otherControlScheme = otherAMI.controlScheme;
			var otherBinding = otherControlScheme.bindings[control.index];
			foreach (var otherSource in otherBinding.sources)
			{
				var deviceStateIndex = otherSource.controlIndex;
				var otherDeviceState = otherAMI.GetDeviceStateForDeviceSlot(otherControlScheme.GetDeviceSlot(otherSource.deviceKey));
				if (otherDeviceState != null)
				{
					foreach (var deviceState in deviceStates)
					{
						var inputDevice = deviceState.controlProvider as InputDevice;
						if (inputDevice == otherDeviceState.controlProvider)
						{
							deviceState.ResetStateForControl(deviceStateIndex);

							var bindings = controlScheme.bindings;
							for (var bindingIndex = 0; bindingIndex < bindings.Count; bindingIndex++)
							{
								var binding = bindings[bindingIndex];

								// Bindings use local indices instead of the actual device state index, so 
								// we have to look those up in order to clear the state
								for (int index = 0; index < binding.sources.Count; index++)
								{
									var source = binding.sources[index];
									var sourceDeviceState = GetDeviceStateForDeviceSlot(controlScheme.GetDeviceSlot(source.deviceKey));
									if (sourceDeviceState == deviceState && source.controlIndex == deviceStateIndex)
										state.ResetStateForControl(bindingIndex);
								}
							}
						}
					}
				}
			}
		}

		public bool CurrentlyUsesDevice(InputDevice device)
		{
			foreach (var deviceState in deviceStates)
				if (deviceState.controlProvider == device)
					return true;
			return false;
		}

		public override bool ProcessEvent/*ForScheme*/(InputEvent inputEvent)
		{
			var consumed = false;
			
			// Update device state (if event actually goes to one of the devices we talk to).
			foreach (var deviceState in deviceStates)
			{
				////FIXME: should refer to proper type
				var device = (InputDevice)deviceState.controlProvider;
				
				// Skip state if event is not meant for device associated with it.
				if (device != inputEvent.device)
					continue;
				
				// Give device a stab at converting the event into state.
				if (device.ProcessEventIntoState(inputEvent, deviceState))
				{
					consumed = true;
					break;
				}
			}
			
			if (!consumed)
				return false;
			
			return true;
		}

		public void Reset(bool initToDeviceState = true)
		{
			if (initToDeviceState)
			{
				foreach (var deviceState in deviceStates)
					deviceState.InitToDevice();
				
				ExtractCurrentValuesFromSources();

				// Copy current values into prev values.
				state.BeginFrame();
			}
			else
			{
				foreach (var deviceState in deviceStates)
					deviceState.Reset();
				state.Reset();
			}
		}

		public List<InputDevice> GetCurrentlyUsedDevices()
		{
			List<InputDevice> list = new List<InputDevice>();
			for (int i = 0; i < deviceStates.Count; i++)
				list.Add(deviceStates[i].controlProvider as InputDevice);
			return list;
		}

		private InputState GetDeviceStateForDeviceSlot(DeviceSlot deviceSlot)
		{
			foreach (var deviceState in deviceStates)
			{
				var inputDevice = deviceState.controlProvider as InputDevice;
				// If this isn't an input device, simply make sure that the types match
				if (inputDevice == null && deviceSlot.type.value.IsInstanceOfType(deviceState.controlProvider))
					return deviceState;

				if (inputDevice != null && (deviceSlot.tagIndex == -1 || inputDevice.tagIndex == deviceSlot.tagIndex))
					return deviceState;
			}
			throw new ArgumentException("deviceType");
		}

		public void BeginFrame()
		{
			state.BeginFrame();
			foreach (var deviceState in deviceStates)
				deviceState.BeginFrame();
		}
		
		public void EndFrame()
		{
			ExtractCurrentValuesFromSources();
		}

		private void ExtractCurrentValuesFromSources()
		{
			for (var entryIndex = 0; entryIndex < actionMap.actions.Count; ++ entryIndex)
			{
				var binding = controlScheme.bindings[entryIndex];
				
				var controlValue = 0.0f;
				foreach (var source in binding.sources)
				{
					var value = GetSourceValue(source);
					if (Mathf.Abs(value) > Mathf.Abs(controlValue))
						controlValue = value;
				}
				
				foreach (var axis in binding.buttonAxisSources)
				{
					var negativeValue = GetSourceValue(axis.negative);
					var positiveValue = GetSourceValue(axis.positive);
					var value = positiveValue - negativeValue;
					if (Mathf.Abs(value) > Mathf.Abs(controlValue))
						controlValue = value;
				}
				
				state.SetCurrentValue(entryIndex, controlValue);
			}
		}

		private float GetSourceValue(InputControlDescriptor source)
		{
			var deviceState = GetDeviceStateForDeviceSlot(m_ControlScheme.GetDeviceSlot(source.deviceKey));
			return deviceState.GetCurrentValue(source.controlIndex);
		}

		public override string GetPrimarySourceName(int controlIndex, string buttonAxisFormattingString = "{0} & {1}")
		{
			var binding = controlScheme.bindings[controlIndex];
			
			if (binding.primaryIsButtonAxis && binding.buttonAxisSources != null && binding.buttonAxisSources.Count > 0)
			{
				return string.Format(buttonAxisFormattingString,
					GetSourceName(binding.buttonAxisSources[0].negative),
					GetSourceName(binding.buttonAxisSources[0].positive));
			}
			else if (binding.sources != null && binding.sources.Count > 0)
			{
				return GetSourceName(binding.sources[0]);
			}
			return string.Empty;
		}

		private string GetSourceName(InputControlDescriptor source)
		{
			var deviceState = GetDeviceStateForDeviceSlot(m_ControlScheme.GetDeviceSlot(source.deviceKey));
			return deviceState.controlProvider.GetControlData(source.controlIndex).name;
		}

		////REVIEW: the descriptor may come from anywhere; method assumes we get passed some state we actually own
		////REVIEW: this mutates the state of the current instance but also mutates the shared ActionMap; that's bad
		public bool BindControl(InputControlDescriptor descriptor, InputControl control, bool restrictToExistingDevices)
		{
			bool existingDevice = false;
			for (int i = 0; i < m_DeviceStates.Count; i++)
			{
				if (control.provider == m_DeviceStates[i].controlProvider)
				{
					existingDevice = true;
					break;
				}
			}
			
			if (!existingDevice)
			{
				if (restrictToExistingDevices)
					return false;
				
				deviceStates.Add(new InputState(control.provider, new List<int>() { control.index }));
			}
			
			descriptor.controlIndex = control.index;
			var inputDevice = control.provider as InputDevice;
			if (inputDevice == null)
			{
				Debug.LogError(string.Format("The InputControlProvider must be an InputDevice, but it is a {0}", control.provider.GetType()));
				return false;
			}
			int key = m_ControlScheme.GetDeviceKey(inputDevice);
			if (key == DeviceSlot.kInvalidKey)
			{
				Debug.LogError(string.Format("Could not find key for InputDevice type {0}", inputDevice.GetType()));
				return false;
			}
			descriptor.deviceKey = key;

			m_ControlScheme.customized = true;
			
			RefreshBindings();
			
			return true;
		}


		public void RevertCustomizations()
		{
			////FIXME: doesn't properly reset control scheme
			m_ActionMap.RevertCustomizations();
			RefreshBindings();
		}
	
		private void RefreshBindings()
		{
			// Gather a mapping of device types to list of bindings that use the given type.
			var perDeviceTypeUsedControlIndices = new Dictionary<int, List<int>>();
			controlScheme.ExtractDeviceTypesAndControlIndices(perDeviceTypeUsedControlIndices);
			
			foreach (var deviceSlot in controlScheme.deviceSlots)
			{
				InputState state = GetDeviceStateForDeviceSlot(deviceSlot);
				List<int> indices;
				if (perDeviceTypeUsedControlIndices.TryGetValue(deviceSlot.key, out indices))
					state.SetUsedControls(indices);
				else
					state.SetAllControlsEnabled(false);
			}
		}
	}

	[Serializable]
	public class ActionMapSlot
	{
		public ActionMap actionMap;
		public bool active = true;
		public bool blockSubsequent;
	}
}
