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

namespace ProBuilder2.VR
{
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

			pb_Start();
		}

		private void OnDestroy()
		{
			if(m_SelectionTool != null)
				m_SelectionTool.enabled = true;

			if(m_ToolMenuPrefab != null)
				U.Object.Destroy(m_ToolMenu);

			pb_OnDestroy();
		}

		public virtual void pb_Start() {}

		public virtual void pb_OnDestroy() {}
	}
}
