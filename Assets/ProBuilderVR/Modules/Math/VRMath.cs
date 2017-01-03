using UnityEngine;

namespace ProBuilder2.VR
{
	public static class VRMath
	{
		/**
		 * Calculate the nearest point on ray A to ray B.
		 */
		public static Vector3 CalculateNearestPointRayRay(Vector3 ao, Vector3 ad, Vector3 bo, Vector3 bd)
		{
			// ray-ray don't do parallel
			if(ad == bd)	
				return ao;

			Vector3 c = bo - ao;

			float n = -Vector3.Dot(ad, bd) * Vector3.Dot(bd, c) + Vector3.Dot(ad, c) * Vector3.Dot(bd, bd);
			float d = Vector3.Dot(ad, ad) * Vector3.Dot(bd, bd) - Vector3.Dot(ad, bd) * Vector3.Dot(ad, bd);

			return ao + ad * (n/d);
		}
		
		/**
		 * Get the point on a plane that this ray intersects.
		 */
		public static Vector3 GetPointOnPlane(Transform rayOrigin, Plane plane)
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
