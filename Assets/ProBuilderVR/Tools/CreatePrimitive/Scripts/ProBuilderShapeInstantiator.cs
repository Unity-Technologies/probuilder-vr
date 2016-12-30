using UnityEngine;
using System.Collections;

namespace ProBuilder2.VR
{
	/**
	 * Shape creation base class.  All functions return True if they still require
	 * input, false if creation is finished.
	 */
	public abstract class ProBuilderShapeInstantiator
	{
		protected GameObject m_GameObject;

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

		/**
		 * Get the point on a plane that this ray intersects.
		 */
		protected Vector3 GetPointOnPlane(Transform rayOrigin, Plane plane)
		{
			Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

			float hit;

			if( plane.Raycast(ray, out hit) )
				return ray.GetPoint(hit);
			else
				return rayOrigin.position;
		}
	}
}
