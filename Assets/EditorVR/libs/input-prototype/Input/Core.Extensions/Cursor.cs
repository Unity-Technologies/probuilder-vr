using UnityEngine;

namespace UnityEngine.InputNew
{
	public class Cursor
	{
		public bool isLocked
		{
			get { return UnityEngine.Cursor.lockState == CursorLockMode.Locked; }
			set
			{
				if (value)
				{
					UnityEngine.Cursor.lockState = CursorLockMode.Locked;
					isVisible = false; ////REVIEW: directly linking these two states is probably bogus
				}
				else
				{
					UnityEngine.Cursor.lockState = CursorLockMode.None;
					isVisible = true;
				}
			}
		}

		public bool isVisible
		{
			get { return UnityEngine.Cursor.visible; }
			set { UnityEngine.Cursor.visible = value; }
		}
	}
}

