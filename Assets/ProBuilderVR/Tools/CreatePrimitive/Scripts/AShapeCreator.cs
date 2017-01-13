using UnityEngine;
using System.Collections;

namespace ProBuilder2.VR
{
	/**
	 * Shape creation base class.  All functions return True if they still require
	 * input, false if creation is finished.
	 */
	public abstract class AShapeCreator
	{
		protected GameObject m_GameObject;

		protected static float m_SnapIncrement = 1/16f;

		public static void SetSnapIncrement(float inc)
		{
			m_SnapIncrement = inc < 0f ? 0f : inc;
		}

		public abstract bool HandleStart(Transform rayOrigin, Plane drawPlane);
		public abstract void HandleDrag(Transform rayOrigin);
		public abstract bool HandleTriggerRelease(Transform rayOrigin);

		/**
		 * Get the GameObject created by this instantiator.
		 */
		public GameObject gameObject {
			get {
				return m_GameObject;
			}
		}
	}
}
