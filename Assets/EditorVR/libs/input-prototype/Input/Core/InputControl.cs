using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	public class InputControl
	{
		protected readonly int m_Index;
		protected readonly InputState m_State;

		public InputControl(int index, InputState state)
		{
			m_Index = index;
			m_State = state;
		}

		public int index
		{
			get { return m_Index; }
		}
		
		public InputControlProvider provider
		{
			get { return m_State.controlProvider; }
		}

		public bool isEnabled
		{
			get { return m_State.IsControlEnabled(m_Index); }
		}

		public InputControlData data
		{
			get { return provider.GetControlData(index); }
		}

		public string name
		{
			get { return data.name; }
		}

		public string GetPrimarySourceName(string buttonAxisFormattingString = "{0} & {1}")
		{
			return m_State.controlProvider.GetPrimarySourceName(index, buttonAxisFormattingString);
		}

		public float rawValue
		{
			get { return m_State.GetCurrentValue(m_Index); }
		}

		public virtual string[] sourceControlNames { get { return null; } }
	}

	[Serializable]
	public class AxisAction : ActionSlot<AxisInputControl> {}
	public class AxisInputControl : InputControl
	{
		public readonly ButtonInputControl negative;
		public readonly ButtonInputControl positive;

		public AxisInputControl(int index, InputState state) : base(index, state)
		{
			negative = new ButtonInputControl(index, state);
			negative.SetValueMultiplier(-1);
			positive = new ButtonInputControl(index, state);
		}

		public float value
		{
			get { return m_State.GetCurrentValue(m_Index); }
		}
	}

	[Serializable]
	public class ButtonAction : ActionSlot<ButtonInputControl> {}
	public class ButtonInputControl : InputControl
	{
		private const float k_ButtonThreshold = 0.5f;
		private float m_ValueMultiplier = 1;

		public ButtonInputControl(int index, InputState state) : base(index, state) {}

		public bool isHeld
		{
			get { return provider.active && m_State.GetCurrentValue(m_Index) * m_ValueMultiplier > k_ButtonThreshold; }
		}

		public bool wasJustPressed
		{
			get { return provider.active && isHeld && (m_State.GetPreviousValue(m_Index) * m_ValueMultiplier <= k_ButtonThreshold); }
		}

		public bool wasJustReleased
		{
			get { return provider.active && !isHeld && (m_State.GetPreviousValue(m_Index) * m_ValueMultiplier > k_ButtonThreshold); }
		}

		public void SetValueMultiplier(float multiplier)
		{
			m_ValueMultiplier = multiplier;
		}
	}

	[Serializable]
	public class Vector2Action : ActionSlot<Vector2InputControl> {}
	public class Vector2InputControl : InputControl
	{
		public Vector2InputControl(int index, InputState state) : base(index, state) {}

		public Vector2 vector2
		{
			get
			{
				var controlData = m_State.controlProvider.GetControlData(m_Index);
				return new Vector2(
					m_State.GetCurrentValue(controlData.componentControlIndices[0]),
					m_State.GetCurrentValue(controlData.componentControlIndices[1])
				);
			}
		}

		public override string[] sourceControlNames { get { return new string[] { "X", "Y" }; } }
	}

	[Serializable]
	public class Vector3Action : ActionSlot<Vector3InputControl> {}
	public class Vector3InputControl : InputControl
	{
		public Vector3InputControl(int index, InputState state) : base(index, state) {}

		public Vector3 vector3
		{
			get
			{
				var controlData = m_State.controlProvider.GetControlData(m_Index);
				return new Vector3(
					m_State.GetCurrentValue(controlData.componentControlIndices[0]),
					m_State.GetCurrentValue(controlData.componentControlIndices[1]),
					m_State.GetCurrentValue(controlData.componentControlIndices[2])
				);
			}
		}

		public override string[] sourceControlNames { get { return new string[] { "X", "Y", "Z" }; } }
	}

	[Serializable]
	public class QuaternionAction : ActionSlot<QuaternionInputControl> { }
	public class QuaternionInputControl : InputControl
	{
		public QuaternionInputControl(int index, InputState state) : base(index, state) { }

		public Quaternion quaternion
		{
			get
			{
				var controlData = m_State.controlProvider.GetControlData(m_Index);
				return new Quaternion(
					m_State.GetCurrentValue(controlData.componentControlIndices[0]),
					m_State.GetCurrentValue(controlData.componentControlIndices[1]),
					m_State.GetCurrentValue(controlData.componentControlIndices[2]),
					m_State.GetCurrentValue(controlData.componentControlIndices[3])
				);
			}
		}

		public override string[] sourceControlNames { get { return new string[] { "X", "Y", "Z", "W" }; } }
	}
}
