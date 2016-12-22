using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEngine.InputNew
{
	public class InputAction : ScriptableObject
	{
		public new string name
		{
			get
			{
				return m_ControlData.name;
			}
			set
			{
				m_ControlData.name = value;
				base.name = value;
			}
		}

		[SerializeField]
		private ActionMap m_ActionMap;
		public ActionMap actionMap { get { return m_ActionMap; } set { m_ActionMap = value; } }

		[SerializeField]
		private int m_ActionIndex;
		public int actionIndex { get { return m_ActionIndex; } set { m_ActionIndex = value; } }

		[SerializeField]
		private InputControlData m_ControlData;
		public InputControlData controlData { get { return m_ControlData; } set { m_ControlData = value; } }
	}
}
