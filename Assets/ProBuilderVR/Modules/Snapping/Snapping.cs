using UnityEngine;

namespace ProBuilder2.VR
{
	public static class Snapping
	{
		const float EPSILON = .0001f;
			
		public static float Snap(float val, float round)
		{
			return round * Mathf.Round(val / round);
		}

		public static float SnapToFloor(float val, float snapValue)
		{
			return snapValue * Mathf.Floor(val / snapValue);
		}

		public static float SnapToCeil(float val, float snapValue)
		{
			return snapValue * Mathf.Ceil(val / snapValue);
		}
		
		/**
		 * Snap a Vector3 to the nearest on-grid point.
		 */
		public static Vector3 Snap(Vector3 val, float snapValue, Vector3 mask = default(Vector3))
		{
			float _x = val.x, _y = val.y, _z = val.z;

			return new Vector3(
				( Mathf.Abs(mask.x) < EPSILON ? _x : Snap(_x, snapValue) ),
				( Mathf.Abs(mask.y) < EPSILON ? _y : Snap(_y, snapValue) ),
				( Mathf.Abs(mask.z) < EPSILON ? _z : Snap(_z, snapValue) )
				);
		}

	}
}
