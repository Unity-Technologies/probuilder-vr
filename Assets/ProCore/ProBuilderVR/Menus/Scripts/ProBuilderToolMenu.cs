using System;
using UnityEngine;
using UnityEngine.Experimental.EditorVR.Menus;

namespace ProBuilder2.VR
{
	public class ProBuilderToolMenu : MonoBehaviour, IMenu
	{
		public bool visible { get { return gameObject.activeSelf; } set { gameObject.SetActive(value); } }

		public GameObject menuContent { get { return gameObject; } }

		public Action onSelectShapeTool, onSelectTranslateTool;

		public void SelectShapeTool()
		{
			onSelectShapeTool();
		}

		public void SelectTranslateTool()
		{
			onSelectTranslateTool();
		}
	}
}
