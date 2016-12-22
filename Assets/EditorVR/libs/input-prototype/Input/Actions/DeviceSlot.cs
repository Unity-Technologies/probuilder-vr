using System;

namespace UnityEngine.InputNew
{
	[Serializable]
	public class DeviceSlot
	{
		public static readonly int kInvalidKey = -1;

		public int key
		{
			get
			{
				return m_Key;
			}
			set
			{
				m_Key = value;
			}
		}

		public SerializableType type
		{
			get
			{
				return m_Type;
			}
			set
			{
				m_Type = value;
			}
		}

		public int tagIndex
		{
			get
			{
				return m_TagIndex;
			}
			set
			{
				m_TagIndex = value;
			}
		}

		[SerializeField]
		private int m_Key = kInvalidKey;

		[SerializeField]
		private SerializableType m_Type;

		[SerializeField]
		private int m_TagIndex = -1;

		public DeviceSlot Clone()
		{
			var clone = new DeviceSlot();
			clone.m_Key = m_Key;
			clone.m_TagIndex = m_TagIndex;
			clone.m_Type = m_Type;

			return clone;
		}
	}
}
