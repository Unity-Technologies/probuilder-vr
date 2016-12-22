using System;

namespace UnityEngine.InputNew
{
	[Serializable]
	public class SerializableType
	{
		[SerializeField]
		private string m_TypeName;

		private Type m_CachedType;

		public SerializableType(Type t)
		{
			value = t;
		}

		public Type value
		{
			get
			{
				if (m_CachedType == null)
				{
					if (string.IsNullOrEmpty(m_TypeName))
						return null;
					m_CachedType = Type.GetType(m_TypeName);
				}
				return m_CachedType;
			}
			set
			{
				m_CachedType = value;
				if (m_CachedType == null)
					m_TypeName = string.Empty;
				else
					m_TypeName = m_CachedType.AssemblyQualifiedName;
			}
		}

		public string Name { get { return value.Name; } }

		public static implicit operator Type(SerializableType t)
		{
			return (t == null) ? null : t.value;
		}

		public static implicit operator SerializableType(Type t)
		{
			return new SerializableType(t);
		}
	}
}