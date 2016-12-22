using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	public class InputState
	{
		#region Constructors

		public InputState(InputControlProvider controlProvider)
			: this(controlProvider, null) { }

		public InputState(InputControlProvider controlProvider, List<int> usedControlIndices)
		{
			this.controlProvider = controlProvider;

			var controlCount = controlProvider.controlCount;
			m_CurrentStates = new float[controlCount];
			m_PreviousStates = new float[controlCount];
			m_Enabled = new bool[controlCount];
			
			SetUsedControls(usedControlIndices);
		}
		
		public void SetUsedControls(List<int> usedControlIndices)
		{
			if (usedControlIndices == null)
			{
				SetAllControlsEnabled(true);
			}
			else
			{
				SetAllControlsEnabled(false);
				for (var i = 0; i < usedControlIndices.Count; i++)
					m_Enabled[usedControlIndices[i]] = true;
			}
		}

		#endregion

		#region Public Methods

		public float GetCurrentValue(int index)
		{
			return m_CurrentStates[index];
		}

		public float GetPreviousValue(int index)
		{
			return m_PreviousStates[index];
		}

		public bool SetCurrentValue(int index, bool value)
		{
			return SetCurrentValue(index, value ? 1.0f : 0.0f);
		}

		public bool SetCurrentValue(int index, float value)
		{
			if (index < 0 || index >= m_CurrentStates.Length)
				throw new ArgumentOutOfRangeException("index",
					string.Format("Control index {0} is out of range; state has {1} entries", index, m_CurrentStates.Length));

			if (!IsControlEnabled(index))
				return false;

			m_CurrentStates[index] = value;

			return true;
		}

		public bool IsControlEnabled(int index)
		{
			return m_Enabled[index];
		}

		public void SetControlEnabled(int index, bool enabled)
		{
			m_Enabled[index] = enabled;
		}

		public void SetAllControlsEnabled(bool enabled)
		{
			for (var i = 0; i < m_Enabled.Length; ++ i)
			{
				m_Enabled[i] = enabled;
			}
		}

		public void InitToDevice()
		{
			if (controlProvider.state == this)
				return;
			
			float[] referenceStates = controlProvider.state.m_CurrentStates;
			for (int i = 0; i < m_CurrentStates.Length; i++)
			{
				if (m_Enabled[i])
				{
					m_CurrentStates[i] = referenceStates[i];
					m_PreviousStates[i] = m_CurrentStates[i];
				}
				else
				{
					ResetStateForControl(i);
				}
			}
		}
		
		public void Reset()
		{
			for (int i = 0; i < m_CurrentStates.Length; i++)
				ResetStateForControl(i);
		}

		public void ResetStateForControl(int controlIndex)
		{
			float defaultValue = controlProvider.GetControlData(controlIndex).defaultValue;
			m_CurrentStates[controlIndex] = defaultValue;
			m_PreviousStates[controlIndex] = defaultValue;
		}

		#endregion

		#region Non-Public Methods

		internal void BeginFrame()
		{
			var stateCount = m_Enabled.Length;
			for (var index = 0; index < stateCount; ++index)
			{
				if (!m_Enabled[index])
					continue;
				if (m_PreviousStates[index] == m_CurrentStates[index])
					continue;
				
				if (InputSystem.listeningForBinding)
				{
					// TODO: Figure out how to use sensible thresholds for different controls.
					if (Mathf.Abs(m_CurrentStates[index]) >= 0.5f && Mathf.Abs(m_PreviousStates[index]) < 0.5f)
						InputSystem.RegisterBinding(controlProvider[index]);
				}
				
				m_PreviousStates[index] = m_CurrentStates[index];
			}
		}

		#endregion

		#region Public Properties

		public InputControlProvider controlProvider { get; set; }

		public int count
		{
			get { return m_CurrentStates.Length; }
		}

		#endregion

		#region Fields

		readonly float[] m_CurrentStates;
		readonly float[] m_PreviousStates;
		readonly bool[] m_Enabled;

		#endregion
	}
}
