using System;
using UnityEngine;
using UnityEngine.Experimental.EditorVR.Menus;
using ProBuilder2.Common;

namespace ProBuilder2.VR
{
	public class CreateProBuilderPrimitiveMenu : MonoBehaviour, IMenu
	{
		[SerializeField]
		CreateShapeButton[] m_HighlightObjects;

		public Action<Shape> selectPrimitive;

		public bool visible { get { return gameObject.activeSelf; } set { gameObject.SetActive(value); } }

		public GameObject menuContent { get { return gameObject; } }

		public void SelectPrimitive(Shape shape)
		{
			selectPrimitive( shape );

			// the order of the objects in m_HighlightObjects is matched to the values of the PrimitiveType enum elements
			for (var i = 0; i < m_HighlightObjects.Length; i++)
			{
				var go = m_HighlightObjects[i];
				go.gameObject.GetComponent<UnityEngine.UI.Image>().color = go.shape == shape ? Color.green : Color.white;
			}
		}
	}
}
