using System;
using UnityEngine;
using UnityEngine.Experimental.EditorVR.Menus;
using ProBuilder2.Common;

namespace ProBuilder2.VR
{
	public class CreateShapeMenu : MonoBehaviour, IMenu
	{
		[SerializeField] [HideInInspector] CreateShapeButton[] m_PrimitiveButtons;

		public Action<Shape> selectPrimitive;
		public bool visible { get { return gameObject.activeSelf; } set { gameObject.SetActive(value); } }
		public GameObject menuContent { get { return gameObject; } }

		void Start()
		{
			m_PrimitiveButtons = GetComponentsInChildren<CreateShapeButton>();
		}

		public void SelectPrimitive(Shape shape)
		{
			selectPrimitive( shape );

			// the order of the objects in m_PrimitiveButtons is matched to the values of the PrimitiveType enum elements
			for (var i = 0; i < m_PrimitiveButtons.Length; i++)
			{
				var go = m_PrimitiveButtons[i];
				go.gameObject.GetComponent<UnityEngine.UI.Image>().color = go.shape == shape ? Color.green : Color.white;
			}
		}
	}
}
