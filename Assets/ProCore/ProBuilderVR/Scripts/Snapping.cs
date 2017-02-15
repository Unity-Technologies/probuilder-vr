using UnityEngine;
using UnityEngine.Experimental.EditorVR.Utilities;
#if UNITY_EDITOR
using ProBuilder2.Common;
using System.Reflection;
using System.Linq;
using UnityEditor;
#endif

namespace ProBuilder2.VR
{
	public static class Snapping
	{
		const float EPSILON = .0001f;

		public const float DEFAULT_INCREMENT = 1/8f;

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
		public static Vector3 Snap(Vector3 val, float snapValue, Vector3 mask)
		{
			float _x = val.x, _y = val.y, _z = val.z;

			return new Vector3(
				( Mathf.Abs(mask.x) < EPSILON ? _x : Snap(_x, snapValue) ),
				( Mathf.Abs(mask.y) < EPSILON ? _y : Snap(_y, snapValue) ),
				( Mathf.Abs(mask.z) < EPSILON ? _z : Snap(_z, snapValue) )
				);
		}

#if UNITY_EDITOR
		private static MethodInfo m_FindNearestVertexMethod = null;
		private static Camera m_CurrentCamera = null;
		private static Camera m_HandleCamera = null;

		/**
		 *	Set up a temporary camera for HandleUtility to use.
		 *	Important - despite the name "Push" this isn't actually
		 *	a stack, it's just one camera.  Don't treat this like a
		 *	FILO queue.
		 */
		private static void PushCamera(Ray ray, Camera camera)
		{
			if(m_HandleCamera == null)
			{
				m_HandleCamera = (Camera) U.Object.CreateGameObjectWithComponent(typeof(Camera));
			}

			m_CurrentCamera = camera;

			if(m_CurrentCamera != null)
				m_HandleCamera.CopyFrom(m_CurrentCamera);

			m_HandleCamera.transform.position = ray.origin;
			m_HandleCamera.transform.forward = ray.direction;
			m_HandleCamera.targetTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);
			Camera.SetupCurrent(m_HandleCamera);
		}

		private static void PopCamera()
		{
			RenderTexture.ReleaseTemporary(m_HandleCamera.targetTexture);
			m_HandleCamera.targetTexture = null;

			if(m_CurrentCamera != null)
				Camera.SetupCurrent(m_CurrentCamera);
		}
#endif

		/**
		 * Find the nearest vertex among all visible objects.
		 */
		public static bool FindNearestVertex(Ray ray, out Vector3 vertex)
		{
#if UNITY_EDITOR
			PushCamera(ray, null);

			Transform[] objects = HandleUtility.PickRectObjects(new Rect(0, 0, Screen.width, Screen.height)).Select(x => x.transform).ToArray();

			object[] parameters = new object[] { (Vector2) m_HandleCamera.pixelRect.center, objects, null };

			if(m_FindNearestVertexMethod == null)
				m_FindNearestVertexMethod = typeof(HandleUtility).GetMethod("FindNearestVertex", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);

			object result = m_FindNearestVertexMethod.Invoke(null, parameters);

			vertex = (bool) result ? (Vector3)parameters[2] : Vector3.zero;

			PopCamera();

			return (bool) result;
#else
			vertex = Vector3.zero;
			return false;
#endif
		}
	}
}
