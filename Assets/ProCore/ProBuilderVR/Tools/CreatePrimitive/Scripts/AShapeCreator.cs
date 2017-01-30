using UnityEngine;
using System.Collections;
using ProBuilder2.Common;

namespace ProBuilder2.VR
{
	/**
	 * Shape creation base class.  All functions return True if they still require
	 * input, false if creation is finished.
	 */
	public abstract class AShapeCreator
	{
		protected GameObject m_GameObject;

		protected pb_Object m_Mesh;

		protected static float m_SnapIncrement = Snapping.DEFAULT_INCREMENT;

		public static void SetSnapIncrement(float inc)
		{
			m_SnapIncrement = inc < 0f ? 0f : inc;
		}

		/**
		 * Delegate to be called when the drawn shape is modified.
		 */
		public System.Action<Vector3> onShapeChanged;

		/**
		 * Begin drawing a shape.  If HandleStart returns false no GameObject has been created
		 * and drawing has been aborted.
		 */
		public abstract bool HandleStart(Ray ray, Plane drawPlane);

		/**
		 * Handle drags after starting shape creation.
		 */
		public abstract void HandleDrag(Ray ray);

		/**
		 * Finalize the shape.
		 */
		public abstract bool HandleTriggerRelease(Ray ray);

		/**
		 * Get the GameObject created by this instantiator.
		 */
		public GameObject gameObject {
			get {
				return m_GameObject;
			}
		}

		public pb_Object pbObject {
			get {
				return m_Mesh;
			}
		}
	}
}
