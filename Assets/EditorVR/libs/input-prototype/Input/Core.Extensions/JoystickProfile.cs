using UnityEngine;
using Assets.Utilities;

namespace UnityEngine.InputNew
{
	public class JoystickProfile
		: InputDeviceProfile
	{
		#region Public Properties

		public JoystickControlMapping[] mappings;
		public string[] nameOverrides;

		protected static Range defaultDeadZones = new Range(0.2f, 0.9f);

		#endregion

		#region Public Methods

		public override void Remap(InputEvent inputEvent)
		{
			var controlEvent = inputEvent as GenericControlEvent;
			if (controlEvent != null)
			{
				var mapping = mappings[controlEvent.controlIndex];
				if (mapping.targetIndex != -1)
				{
					controlEvent.controlIndex = mapping.targetIndex;
					controlEvent.value = Mathf.InverseLerp(mapping.fromRange.min, mapping.fromRange.max, controlEvent.value);
					controlEvent.value = Mathf.Lerp(mapping.toRange.min, mapping.toRange.max, controlEvent.value);
					Range deadZones = mapping.interDeadZoneRange;
					controlEvent.value = Mathf.InverseLerp(deadZones.min, deadZones.max, Mathf.Abs(controlEvent.value))
						* Mathf.Sign(controlEvent.value);
				}
			}
		}

		public void SetMappingsCount(int sourceControlCount, int tarcontrolCount)
		{
			mappings = new JoystickControlMapping[sourceControlCount];
			nameOverrides = new string[tarcontrolCount];
		}

		public void SetMapping(int sourceControlIndex, int targetControlIndex, string displayName)
		{
			SetMapping(sourceControlIndex, targetControlIndex, displayName, defaultDeadZones, Range.full, Range.full);
		}

		public void SetMapping(int sourceControlIndex, int targetControlIndex, string displayName, Range interDeadZoneRange)
		{
			SetMapping(sourceControlIndex, targetControlIndex, displayName, interDeadZoneRange, Range.full, Range.full);
		}

		public void SetMapping(int sourceControlIndex, int targetControlIndex, string displayName, Range interDeadZoneRange, Range sourceRange, Range targetRange)
		{
			mappings[sourceControlIndex] = new JoystickControlMapping
			{
				targetIndex = targetControlIndex,
				fromRange = sourceRange,
				toRange = targetRange,
				interDeadZoneRange = interDeadZoneRange
			};
			nameOverrides[targetControlIndex] = displayName;
		}
		
		public override string GetControlNameOverride(int controlIndex)
		{
			if (controlIndex >= nameOverrides.Length)
				return null;
			return nameOverrides[controlIndex];
		}

		#endregion
	}
}
