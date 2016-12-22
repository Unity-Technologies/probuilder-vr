using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.InputNew
{
	public class PlayerHandle : IInputConsumer
	{
		public readonly int index;
		public List<PlayerDeviceAssignment> assignments = new List<PlayerDeviceAssignment>();
		public List<ActionMapInput> maps = new List<ActionMapInput>();

		private bool m_Global = false;

		public bool processAll { get; set; }
		
		public delegate void ChangeEvent();
		public static ChangeEvent onChange;

		InputConsumerNode currentInputConsumer
		{
			get
			{
				return m_Global ? InputSystem.globalPlayers : InputSystem.assignedPlayers;
			}
		}

		internal PlayerHandle(int index)
		{
			this.index = index;
			currentInputConsumer.children.Add(this);

			if (onChange != null)
				onChange.Invoke();
		}

		public void Destroy()
		{
			foreach (var map in maps)
				map.active = false;

			for (int i = assignments.Count - 1; i >= 0; i--)
				assignments[i].Unassign();
			
			currentInputConsumer.children.Remove(this);

			PlayerHandleManager.RemovePlayerHandle(this);
			if (onChange != null)
				onChange.Invoke();
		}

		public bool global
		{
			get { return m_Global; }
			set
			{
				if (value == m_Global)
					return;

				// Note: value of m_Global changes what currentInputConsumer is.
				currentInputConsumer.children.Remove(this);
				m_Global = value;
				currentInputConsumer.children.Add(this);

				if (onChange != null)
					onChange.Invoke();
			}
		}

		public T GetActions<T>() where T : ActionMapInput
		{
			// If already contains ActionMapInput if this type, return that.
			for (int i = 0; i < maps.Count; i++)
				if (maps[i].GetType() == typeof(T))
					return (T)maps[i];
			return null;
		}

		public ActionMapInput GetActions(ActionMap actionMap)
		{
			// If already contains ActionMapInput based on this ActionMap, return that.
			for (int i = 0; i < maps.Count; i++)
				if (maps[i].actionMap == actionMap)
					return maps[i];
			return null;
		}

		public bool AssignDevice(InputDevice device, bool assign)
		{
			if (assign)
			{
				if (device.assignment != null)
				{
					// If already assigned to this player, accept as success. Otherwise, fail.
					if (device.assignment.player == this)
						return true;
					else
						return false;
				}

				var assignment = new PlayerDeviceAssignment(this, device);
				assignment.Assign();

				return true;
			}
			else
			{
				if (device.assignment.player == this)
				{
					device.assignment.Unassign();
					return true;
				}
				return false;
			}
		}

		public bool ProcessEvent(InputEvent inputEvent)
		{
		    if (!global && (inputEvent.device.assignment == null || inputEvent.device.assignment.player != this))
		        return false;
			for (int i = 0; i < maps.Count; i++)
			{
				if ((global || maps[i].CurrentlyUsesDevice(inputEvent.device)))
				{
					if ((ProcessEventInMap(maps[i], inputEvent) && !processAll) || maps[i].blockSubsequent)
						return true;
				}
			}

			return false;
		}

	    bool ProcessEventInMap(ActionMapInput map, InputEvent inputEvent)
	    {
	        if (map.ProcessEvent(inputEvent))
	            return true;

	        if (map.CurrentlyUsesDevice(inputEvent.device))
	            return false;

            // If this event uses a different device than the current control scheme then try and initialize a control scheme that has that device.
            // Otherwise, leave the current current control scheme state alone as a re-initialization of the same control scheme will cause a reset in the process.
	        if (!map.autoReinitialize || !map.TryInitializeWithDevices(GetApplicableDevices(), new List<InputDevice>() { inputEvent.device }))
	            return false;

            // When changing control scheme, we do not want to init control scheme to device states
            // like we normally want, so do a hard reset here, before processing the new event.
            map.Reset(false);

            return map.ProcessEvent(inputEvent);
        }

		public IEnumerable<InputDevice> GetApplicableDevices()
		{
			if (global)
				return InputSystem.devices.Where(e => e.assignment == null);
			else
				return assignments.Select(e => e.device);
		}

		public void BeginFrame()
		{
			for (int i = 0; i < maps.Count; i++)
			{
				maps[i].BeginFrame();
			}
		}
		
		public void EndFrame()
		{
			for (int i = 0; i < maps.Count; i++)
			{
				maps[i].EndFrame();
			}
		}
	}
	
}
