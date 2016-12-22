using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.InputNew
{
	[CreateAssetMenu()]
#if UNITY_EDITOR
	[InitializeOnLoad]
#endif
	public class ActionMap : ScriptableObject
	{
		public static readonly string kDefaultNamespace = "UnityEngine.InputNew";

		[FormerlySerializedAs("entries")]
		[SerializeField]
		private List<InputAction> m_Actions = new List<InputAction>();
		public List<InputAction> actions { get { return m_Actions; } set { m_Actions = value; } }
		
		[SerializeField]
		private List<ControlScheme> m_ControlSchemes = new List<ControlScheme>();
		private List<ControlScheme> m_ControlSchemeCopies; // In players or playmode we always hand out copies and retain the originals.
		public List<ControlScheme> controlSchemes
		{
			get
			{
#if UNITY_EDITOR
				if (!s_IsInPlayMode)
				{
					return m_ControlSchemes; // ActionMapEditor modifies this directly.
				}
#endif
				if (m_ControlSchemeCopies == null || m_ControlSchemeCopies.Count == 0)
				{
					m_ControlSchemeCopies = m_ControlSchemes.Select(x => x.Clone()).ToList();
#if UNITY_EDITOR
					s_ActionMapsToCleanUpAfterPlayMode.Add(this);
#endif
				}
				return m_ControlSchemeCopies;
			}

			set
			{
				m_ControlSchemes = value;
				m_ControlSchemeCopies = null;
			}
		}

		// In the editor, throw away all customizations when exiting playmode.
#if UNITY_EDITOR
		private static List<ActionMap> s_ActionMapsToCleanUpAfterPlayMode = new List<ActionMap>();
		private static bool s_IsInPlayMode;

		static ActionMap()
		{
			EditorApplication.delayCall += HandlePlayModeCustomizations;
			EditorApplication.playmodeStateChanged += HandlePlayModeCustomizations;
		}

		private static void HandlePlayModeCustomizations()
		{
			// TODO: Fix error.
			// get_isPlayingOrWillChangePlaymode is not allowed to be called from a MonoBehaviour constructor,
			// call it in Awake or Start instead. Called from script 'ActionMap' on game object 'MenuActions'.
			// See "Script Serialization" page in the Unity Manual for further details.
			if (EditorApplication.isPlayingOrWillChangePlaymode)
			{
				s_IsInPlayMode = true;
			}
			else if (!EditorApplication.isPlaying)
			{
				s_IsInPlayMode = false;
				// Throw away all the copies of ControlSchemes we made in play mode.
				foreach (var actionMap in s_ActionMapsToCleanUpAfterPlayMode)
				{
					actionMap.m_ControlSchemeCopies = null;
				}
				s_ActionMapsToCleanUpAfterPlayMode.Clear();
			}
		}
#endif

		public Type mapType
		{
			get
			{
				if ( m_CachedMapType == null )
				{
					if (m_MapTypeName == null)
						return null;
					m_CachedMapType = Type.GetType( m_MapTypeName );
				}
				return m_CachedMapType;
			}
			set
			{
				m_CachedMapType = value;
				m_MapTypeName = m_CachedMapType.AssemblyQualifiedName;
			}
		}
		[SerializeField]
		private string m_MapTypeName;
		private Type m_CachedMapType;
		public void SetMapTypeName(string name)
		{
			m_MapTypeName = name;
		}

		public Type customActionMapType {
			get
			{
				string typeString = string.Format(
					"{0}.{1}, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
					string.IsNullOrEmpty(m_CustomNamespace) ? kDefaultNamespace : m_CustomNamespace,
                    name);
				Type t = null;
				try
				{
					t = Type.GetType(typeString);
				}
				catch (Exception e)
				{
					throw new Exception("Failed to create type from string \"" + typeString + "\".", e);
				}

				if (t == null)
					throw new Exception("Failed to create type from string \"" + typeString + "\".");

				return t;
			}
		}

		[SerializeField]
		private string m_CustomNamespace;
		public string customNamespace
		{
			get
			{
				return m_CustomNamespace;
			}
			set
			{
				m_CustomNamespace = value;
			}
		}

		public string GetCustomizations()
		{
			var customizedControlSchemes = m_ControlSchemeCopies.Where(x => x.customized).ToList();
			return JsonUtility.ToJson(customizedControlSchemes);
		}

		public void RevertCustomizations()
		{
			m_ControlSchemeCopies = null;
		}

		public void RevertCustomizations(ControlScheme controlScheme)
		{
			if (m_ControlSchemeCopies != null)
			{
				for (var i = 0; i < m_ControlSchemeCopies.Count; ++i)
				{
					if (m_ControlSchemeCopies[i] == controlScheme)
					{
						m_ControlSchemeCopies[i] = m_ControlSchemes[i].Clone();
						break;
					}
				}
			}
		}

		public void RestoreCustomizations(string customizations)
		{
			var customizedControlSchemes = JsonUtility.FromJson<List<ControlScheme>>(customizations);
			foreach (var customizedScheme in customizedControlSchemes)
			{
				// See if it replaces an existing scheme.
				var replacesExisting = false;
				for (var i = 0; i < controlSchemes.Count; ++i)
				{
					if (String.Compare(controlSchemes[i].name, customizedScheme.name, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase) == 0)
					{
						// Yes, so get rid of current scheme.
						controlSchemes[i] = customizedScheme;
						replacesExisting = true;
						break;
					}
				}

				if (!replacesExisting)
				{
					// No, so add as new scheme.
					controlSchemes.Add(customizedScheme);
				}
			}
		}
	}
}
