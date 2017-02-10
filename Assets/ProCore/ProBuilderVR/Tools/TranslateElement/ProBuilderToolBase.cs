using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine.InputNew;
using UnityEngine.Experimental.EditorVR;
using UnityEngine.Experimental.EditorVR.Menus;
using UnityEngine.Experimental.EditorVR.Tools;
using UnityEngine.Experimental.EditorVR.Utilities;
using System.Reflection;

namespace ProBuilder2.VR
{
	/**
	 *	Defines some common behaviors among ProBuilderVR tools.
	 */
	public class ProBuilderToolBase : 	MonoBehaviour,
										IUsesRayOrigin,
										IConnectInterfaces,
										IInstantiateUI,
										ISelectTool,
										IUsesMenuOrigins
	{
		[SerializeField] private ProBuilderToolMenu m_ToolMenuPrefab;

		private GameObject m_ToolMenu;
		public InstantiateUIDelegate instantiateUI { private get; set; }
		public ConnectInterfacesDelegate connectInterfaces { private get; set; }
		public Func<Transform, Type, bool> selectTool { private get; set; }
		public Transform menuOrigin { get; set; }
		public Transform alternateMenuOrigin { get; set; }
		public Transform rayOrigin { get; set; }
		
		// Disable the selection tool to turn of the hover highlight.  Not using
		// IExclusiveMode because we still want locomotion.
		private SelectionTool m_SelectionTool = null;

		private void Start()
		{
			if(m_ToolMenuPrefab == null)
			{
				Debug.LogError("Assign ProBuilder Tool Menu prefab on inheriting script!");
			}
			else
			{
				m_ToolMenu = instantiateUI(m_ToolMenuPrefab.gameObject, alternateMenuOrigin, false);
				var toolsMenu = m_ToolMenu.GetComponent<ProBuilderToolMenu>();
				connectInterfaces(toolsMenu, rayOrigin);
				toolsMenu.onSelectTranslateTool += () => { selectTool(rayOrigin, typeof(TranslateElementTool)); };
				toolsMenu.onSelectShapeTool += () => { selectTool(rayOrigin, typeof(CreateShapeTool)); };
			}
				
			m_SelectionTool = gameObject.GetComponent<SelectionTool>();

			if(m_SelectionTool != null)
			{
				m_SelectionTool.enabled = false;
			}

			// In order to use some HandleUtility functions we'll need access to the OnSceneGUI delegate.  
			// This hooks up that event.
			// @todo - Ask Unity team about making this less hack-y.
			foreach(EditorWindow win in Resources.FindObjectsOfTypeAll<EditorWindow>())
			{
				if(win.GetType().ToString().Contains("VRView"))
				{
					Type vrViewType = win.GetType();

					EventInfo eventInfo = vrViewType.GetEvent("onGUIDelegate", BindingFlags.Public | BindingFlags.Static);
					Delegate onGUIDelegate = Delegate.CreateDelegate(eventInfo.EventHandlerType,
						this,
						typeof(ProBuilderToolBase).GetMethod("OnGUIInternal", BindingFlags.Instance | BindingFlags.NonPublic) );
					eventInfo.AddEventHandler(this, onGUIDelegate);
				}
			}

			pb_Start();
		}

		private void OnGUIInternal(EditorWindow window)
		{
			OnSceneGUI(window);
		}

		private void OnDestroy()
		{
			foreach(EditorWindow win in Resources.FindObjectsOfTypeAll<EditorWindow>())
			{
				if(win.GetType().ToString().Contains("VRView"))
				{
					Type vrViewType = win.GetType();

					EventInfo eventInfo = vrViewType.GetEvent("onGUIDelegate", BindingFlags.Public | BindingFlags.Static);
					Delegate onGUIDelegate = Delegate.CreateDelegate(eventInfo.EventHandlerType,
						this,
						typeof(ProBuilderToolBase).GetMethod("OnGUIInternal", BindingFlags.Instance | BindingFlags.NonPublic) );
					eventInfo.RemoveEventHandler(this, onGUIDelegate);
				}
			}


			if(m_SelectionTool != null)
				m_SelectionTool.enabled = true;

			if(m_ToolMenu != null)
				U.Object.Destroy(m_ToolMenu);

			pb_OnDestroy();
		}

		public virtual void pb_Start() {}

		public virtual void OnSceneGUI(EditorWindow window) {}

		public virtual void pb_OnDestroy() {}
	}
}
