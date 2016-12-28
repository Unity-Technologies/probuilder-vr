using UnityEngine;
using ProBuilder2.Common;

namespace ProBuilder2.VR
{
	public class CreateShapeButton : MonoBehaviour
	{
		public Shape shape;
		public CreateProBuilderPrimitiveMenu menu;

		public void SelectedPrimitive()
		{
			if(menu != null)
				menu.SelectPrimitive(shape);
		}
	}
}
