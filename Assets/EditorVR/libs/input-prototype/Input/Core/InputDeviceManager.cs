using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	class InputDeviceManager : IInputConsumer
	{
		#region Inner Types

		public delegate void DeviceConnectDisconnectEvent(InputDevice device, bool connected);

		#endregion

		#region Public Events

		public DeviceConnectDisconnectEvent deviceConnectedDisconnected = null;

		#endregion

		#region Constructors

		public InputDeviceManager()
		{
		}
		
		public void InitAfterProfiles()
		{
			// In the prototype, just create a set of default devices. In the real thing, these would be registered
			// and configured by the platform layer according to what's really available on the system.

			var mouseDevice = new Mouse();
			RegisterDevice(mouseDevice);

			var touchscreenDevice = new Touchscreen();
			RegisterDevice(touchscreenDevice); // Register after mouse; multiplayer code will pick the first applicable device. It doesn't use MRU.

			var keyboardDevice = new Keyboard();
			RegisterDevice(keyboardDevice);

			var gamepadDevice1 = new Gamepad();
			RegisterDevice(gamepadDevice1);

			var gamepadDevice2 = new Gamepad();
			RegisterDevice(gamepadDevice2);

			var virtualJoystickDevice = new VirtualJoystick();
			RegisterDevice(virtualJoystickDevice);

			// TODO: do we want to expose HMDs like this?
			var vrDeviceHMDLeftEye = new VRInputDevice();
			RegisterDevice(vrDeviceHMDLeftEye);

			var vrDeviceHMDRightEye = new VRInputDevice();
			RegisterDevice(vrDeviceHMDRightEye);

			var vrDeviceHMDCenterEye = new VRInputDevice();
			RegisterDevice(vrDeviceHMDCenterEye);

			var vrDeviceHMDController1 = new VRInputDevice();
			RegisterDevice(vrDeviceHMDController1);
		    vrDeviceHMDController1.Hand = VRInputDevice.Handedness.Left;

			var vrDeviceHMDController2 = new VRInputDevice();
			RegisterDevice(vrDeviceHMDController2);
		    vrDeviceHMDController2.Hand = VRInputDevice.Handedness.Right;
		}

		#endregion

		#region Public Methods

		public void RegisterDevice(InputDevice device)
		{
			AssignDeviceProfile(device);
			RegisterDeviceInternal(device.GetType(), device);
			HandleDeviceConnectDisconnect(device, true);
		}

		public void RegisterProfile(InputDeviceProfile profile)
		{
			m_Profiles.Add(profile);
		}

		public InputDevice GetMostRecentlyUsedDevice(Type deviceType)
		{
			InputDevice mostRecentDevice = null;
			for (var i = m_Devices.Count - 1; i >= 0; -- i)
			{
				var device = m_Devices[i];
				if (deviceType.IsInstanceOfType(device) && (mostRecentDevice == null || device.lastEventTime > mostRecentDevice.lastEventTime))
					mostRecentDevice = device;
			}
			return mostRecentDevice;
		}

		public TDevice GetMostRecentlyUsedDevice<TDevice>()
			where TDevice : InputDevice
		{
			return (TDevice)GetMostRecentlyUsedDevice(typeof(TDevice));
		}

		internal int GetNewDeviceIndex(InputDevice device)
		{
			Type t = device.GetType();
			int count = 0;
			for (int i = 0; i < devices.Count; i++)
				if (devices[i].GetType() == t)
					count++;
			return count;
		}

		public InputDevice LookupDevice(Type deviceType, int deviceIndex)
		{
			List<InputDevice> list;
			if (!m_DevicesByType.TryGetValue(deviceType, out list) || deviceIndex >= list.Count)
				return null;

			return list[deviceIndex];
		}

		////REVIEW: an alternative to these two methods is to hook every single device into the event tree independently; may be better

		public bool ProcessEvent(InputEvent inputEvent)
		{
			// Look up device.
			var device = inputEvent.device;

			// Ignore event on device if device is absent or disconnected.
			if (device == null || !device.connected)
				return false;

			//fire event

			// Let device process event.
			return device.ProcessEvent(inputEvent);
		}

		public void BeginFrame()
		{
			foreach (var device in devices)
				device.state.BeginFrame();
		}

		public void EndFrame()
		{

		}

		public bool RemapEvent(InputEvent inputEvent)
		{
			// Look up device.
			var device = inputEvent.device;
			if (device == null)
				return false;

			// Let device remap event.
			return device.RemapEvent(inputEvent);
		}

		public void DisconnectDevice(InputDevice device)
		{
			if (!device.connected)
				return;

			HandleDeviceConnectDisconnect(device, false);
		}

		public void ReconnectDevice(InputDevice device)
		{
			if (device.connected)
				return;

			HandleDeviceConnectDisconnect(device, true);
		}

		#endregion

		#region Non-Public Methods

		void AssignDeviceProfile(InputDevice device)
		{
			device.profile = FindDeviceProfile(device);
		}

		InputDeviceProfile FindDeviceProfile(InputDevice device)
		{
			foreach (var profile in m_Profiles)
			{
				if (profile.deviceNames != null)
				{
					foreach (var deviceName in profile.deviceNames)
					{
						if (string.Compare(deviceName, device.deviceName, StringComparison.InvariantCulture) == 0 &&
							IsProfileSupportedOnThisPlatform(profile))
							return profile;
					}
				}
			}
			return null;
		}

		public bool IsProfileSupportedOnThisPlatform(InputDeviceProfile profile)
		{
			if (profile.supportedPlatforms == null || profile.supportedPlatforms.Length == 0)
				return true;
			
			foreach (var platform in profile.supportedPlatforms)
			{
				// VR devices can be hot-swapped -- this can change at any point so we should check it every frame (or provide an event).
				var vrPlatform = (UnityEngine.VR.VRSettings.loadedDeviceName + " " + UnityEngine.VR.VRDevice.model).ToUpper();

				if (m_Platform.Contains(platform.ToUpper()) || vrPlatform.Contains(platform.ToUpper()))
					return true;
			}
			
			return false;
		}

		void RegisterDeviceInternal(Type deviceType, InputDevice device)
		{
			List<InputDevice> list;
			if (!m_DevicesByType.TryGetValue(deviceType, out list))
			{
				list = new List<InputDevice>();
				m_DevicesByType[deviceType] = list;
			}

			list.Add(device);
			// Called recorsively for base types so remove first to make sure it's only added once.
			m_Devices.Remove(device);
			m_Devices.Add(device);

			var baseType = deviceType.BaseType;
			if (baseType != typeof(InputDevice))
				RegisterDeviceInternal(baseType, device);
		}

		void HandleDeviceConnectDisconnect(InputDevice device, bool connected)
		{
			// Sync state.
			device.connected = connected;

			// Fire event.
			var handler = deviceConnectedDisconnected;
			if (handler != null)
				handler(device, connected);
		}

		#endregion

		#region Public Properties
		
		public List<InputDevice> devices
		{
			get { return m_Devices; }
		}

		#endregion

		#region Fields

		readonly Dictionary<Type, List<InputDevice>> m_DevicesByType = new Dictionary<Type, List<InputDevice>>();
		readonly List<InputDevice> m_Devices = new List<InputDevice>();
		readonly List<InputDeviceProfile> m_Profiles = new List<InputDeviceProfile>();

		// PJK: SystemInfo.deviceModel returns the name of my motherboard.  Is this useful?
		readonly string m_Platform = (SystemInfo.operatingSystem + " " + SystemInfo.deviceModel).ToUpper();

		#endregion
	}
}
