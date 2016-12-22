using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace UnityEngine.InputNew
{
	public class PlayerInput : MonoBehaviour
	{
		// Should this player handle request assignment of an input device as soon as the component awakes?
		[FormerlySerializedAs("autoSinglePlayerAssign")]
		public bool autoAssignGlobal = true;

		public List<ActionMapSlot> actionMaps = new List<ActionMapSlot>();

		public PlayerHandle handle { get; set; }

		void Awake()
		{
			if (autoAssignGlobal)
			{
				handle = PlayerHandleManager.GetNewPlayerHandle();
				handle.global = true;
				foreach (ActionMapSlot actionMapSlot in actionMaps)
				{
					ActionMapInput actionMapInput = ActionMapInput.Create(actionMapSlot.actionMap);
					actionMapInput.TryInitializeWithDevices(handle.GetApplicableDevices());
					actionMapInput.active = actionMapSlot.active;
					actionMapInput.blockSubsequent = actionMapSlot.blockSubsequent;
					handle.maps.Add(actionMapInput);
				}
			}
		}

		public T GetActions<T>() where T : ActionMapInput
		{
			if (handle == null)
				return null;
			return handle.GetActions<T>();
		}
	}
}
