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

		protected static float m_SnapIncrement = Snapping.DEFAULT_INCREMENT;

		public static void SetSnapIncrement(float inc)
		{
			m_SnapIncrement = inc < 0f ? 0f : inc;
		}

		/**
		 * Begin drawing a shape.  If HandleStart returns false no GameObject has been created
		 * and drawing has been aborted.
		 */
		public abstract bool HandleStart(Transform rayOrigin, Plane drawPlane);

		/**
		 * Handle drags after starting shape creation.
		 */
		public abstract void HandleDrag(Transform rayOrigin);

		/**
		 * Finalize the shape.
		 */
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
