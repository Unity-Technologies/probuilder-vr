using System;
using System.Collections.Generic;
using UnityEngine;

//// - solve mapping of device type names from control maps to device types at runtime

namespace UnityEngine.InputNew
{
	public static class InputSystem
	{
		// For now, initialize prototype stuff here.
		// This should not be included here in final code.
		static InputSystem()
		{
			s_Devices = new InputDeviceManager();

			GameObject go = new GameObject("Input Prototype Controller");
			go.hideFlags = HideFlags.HideAndDontSave;

			go.AddComponent<InputManager>();
			go.AddComponent<InputManagerEndFrame>();
			go.AddComponent<JoystickInputToEvents>();
			go.AddComponent<MouseInputToEvents>();
			go.AddComponent<KeyboardInputToEvents>();
			go.AddComponent<TouchInputToEvents>();
			go.AddComponent<VRInputToEvents>();
			go.AddComponent<ExecuteAllEvents>();

			InputDeviceProfile[] profiles = new InputDeviceProfile[]
			{
				new Xbox360MacProfile(),
				new Xbox360WinProfile(),
				new OpenVRProfile(), 
			};
			s_EventQueue = new InputEventQueue();
			s_EventPool = new InputEventPool();

			foreach (var profile in profiles)
			{
				RegisterProfile(profile);
			}

			s_Devices.InitAfterProfiles();

			// Set up event tree.
			s_EventTree = new InputConsumerNode();

			s_EventTree.children.Add(new InputConsumerCallback { processEvent = s_Devices.RemapEvent });

			rewriters = new InputConsumerNode();
			s_EventTree.children.Add(rewriters);

			s_EventTree.children.Add(s_Devices);

			consumers = new InputConsumerNode();
			s_EventTree.children.Add(consumers);

			assignedPlayers = new InputConsumerNode();
			consumers.children.Add(assignedPlayers);

			// Global consumers should be processed last.
			globalPlayers = new InputConsumerNode();
			consumers.children.Add(globalPlayers);

			simulateMouseWithTouches = true;
		}

		public delegate bool BindingListener(InputControl control);
		
		#region Public Methods

		public static void RegisterProfile(InputDeviceProfile profile)
		{
			s_Devices.RegisterProfile(profile);
		}

		public static InputDevice LookupDevice(Type deviceType, int deviceIndex)
		{
			return s_Devices.LookupDevice(deviceType, deviceIndex);
		}

		public static void QueueEvent(InputEvent inputEvent)
		{
			s_EventQueue.Queue(inputEvent);
		}

		public static bool ExecuteEvent(InputEvent inputEvent)
		{
			var wasConsumed = s_EventTree.ProcessEvent(inputEvent);
			s_EventPool.Return(inputEvent);
			return wasConsumed;
		}

		public static TEvent CreateEvent<TEvent>()
			where TEvent : InputEvent, new()
		{
			var newEvent = s_EventPool.ReuseOrCreate<TEvent>();
			newEvent.time = Time.time;
			return newEvent;
		}

		public static void ListenForBinding (BindingListener listener)
		{
			s_BindingListeners.Add(listener);
		}

		#endregion

		#region Non-Public Methods

		internal static void ExecuteEvents()
		{
			var currentTime = Time.time;
			InputEvent nextEvent;
			while (s_EventQueue.Dequeue(currentTime, out nextEvent))
			{
				ExecuteEvent(nextEvent);
			}
		}

		internal static void BeginFrame()
		{
			s_EventTree.BeginFrame();
		}

		internal static void EndFrame()
		{
			s_EventTree.EndFrame();
		}

		private static bool SendSimulatedMouseEvents(InputEvent inputEvent)
		{
			////FIXME: should take actual touchdevice in inputEvent into account
			var touchEvent = inputEvent as TouchEvent;
			if (touchEvent != null)
				Touchscreen.current.SendSimulatedPointerEvents(touchEvent, UnityEngine.Cursor.lockState == CursorLockMode.Locked);
			return false;
		}
		
		internal static void RegisterBinding(InputControl control)
		{
			for (int i = s_BindingListeners.Count - 1; i >= 0; i--)
			{
				if (s_BindingListeners[i] == null)
				{
					s_BindingListeners.RemoveAt(i);
					continue;
				}
				bool used = s_BindingListeners[i](control);
				if (used)
				{
					s_BindingListeners.RemoveAt(i);
					break;
				}
			}
		}

		#endregion

		#region Public Properties

		public static IInputConsumer eventTree
		{
			get { return s_EventTree; }
		}

		public static InputConsumerNode consumers { get; private set; }
		public static InputConsumerNode globalPlayers { get; private set; }
		public static InputConsumerNode assignedPlayers { get; private set; }
		public static InputConsumerNode rewriters { get; private set; }

		public static bool listeningForBinding
		{
			get { return s_BindingListeners.Count > 0; }
		}

		public static List<InputDevice> devices
		{
			get { return s_Devices.devices; }
		}
		
		public static TDevice GetMostRecentlyUsedDevice<TDevice>()
			where TDevice : InputDevice
		{
			return s_Devices.GetMostRecentlyUsedDevice<TDevice>();
		}

		internal static int GetNewDeviceIndex(InputDevice device)
		{
			return s_Devices.GetNewDeviceIndex(device);
		}

		public static bool simulateMouseWithTouches
		{
			get { return s_SimulateMouseWithTouches; }
			set
			{
				if (value == s_SimulateMouseWithTouches)
					return;

				if (value)
				{
					if (s_SimulateMouseWithTouchesProcess == null)
						s_SimulateMouseWithTouchesProcess = new InputConsumerCallback
						{
							processEvent = SendSimulatedMouseEvents
						};

					rewriters.children.Add(s_SimulateMouseWithTouchesProcess);
				}
				else
				{
					if (s_SimulateMouseWithTouchesProcess != null)
						rewriters.children.Remove(s_SimulateMouseWithTouchesProcess);
				}

				s_SimulateMouseWithTouches = value;
			}
		}

		#endregion

		#region Fields

		static InputDeviceManager s_Devices;
		static InputEventQueue s_EventQueue;
		static InputEventPool s_EventPool;
		static InputConsumerNode s_EventTree;
		static bool s_SimulateMouseWithTouches;
		static IInputConsumer s_SimulateMouseWithTouchesProcess;
		static List<BindingListener> s_BindingListeners = new List<BindingListener>();

		#endregion
	}
}
