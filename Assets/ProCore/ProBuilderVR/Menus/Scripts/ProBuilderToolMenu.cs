using System;
using UnityEngine;
using UnityEditor.Experimental.EditorVR;
using UnityEditor.Experimental.EditorVR.Menus;

namespace ProBuilder2.VR
{
	public class ProBuilderToolMenu : MonoBehaviour, IMenu
	{
		public bool visible { get { return gameObject.activeSelf; } set { gameObject.SetActive(value); } }

		public GameObject menuContent { get { return gameObject; } }

		public MenuHideFlags menuHideFlags
		{
			get
			{
				throw new NotImplementedException();
			}

			set
			{
				throw new NotImplementedException();
			}
		}

		public Bounds localBounds
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int priority
		{
			get
			{
				throw new NotImplementedException();
			}
		}

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
