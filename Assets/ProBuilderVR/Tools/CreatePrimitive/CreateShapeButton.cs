using UnityEngine;
using ProBuilder2.Common;

namespace ProBuilder2.VR
{
	public class CreateShapeButton : MonoBehaviour
	{
		public Shape shape;
		private CreateShapeMenu m_CreateShapeMenu;

		void Start()
		{
			Transform parent = transform.parent;

			while(m_CreateShapeMenu == null && parent != null)
			{
				m_CreateShapeMenu = parent.GetComponent<CreateShapeMenu>();
				parent = parent.parent;
			}

			if(m_CreateShapeMenu == null)
				Debug.LogWarning("CreateShapeMenu component not found in parent GameObjects!");
		}

		public void SelectedPrimitive()
		{
			if(m_CreateShapeMenu != null)
				m_CreateShapeMenu.SelectPrimitive(shape);
		}
	}
}
