using UnityEngine;
using UnityEngine.Experimental.EditorVR.Utilities;
#if UNITY_EDITOR
using ProBuilder2.Common;
using System.Reflection;
using System.Linq;
using UnityEditor;
#endif
using UnityEditor.Experimental.EditorVR;

namespace ProBuilder2.VR
{
	public struct VertexSnap
	{
		public const float MAX_VERTEX_SNAP_DISTANCE = .5f;

		public bool valid;

		public Vector3 point;

		public VertexSnap(bool valid, Vector3 point)
		{
			this.valid = valid;
			this.point = point;
		}
	}

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
		private static MethodInfo m_PickGameObjectMethod = null;
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
				m_HandleCamera = (Camera) U.Object.CreateGameObjectWithComponent(typeof(Camera));

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
			GameObject.DestroyImmediate(m_HandleCamera.gameObject);

			if(m_CurrentCamera != null)
				Camera.SetupCurrent(m_CurrentCamera);
		}
#endif

		/**
		 * Find the nearest vertex among all visible objects.  Must be called from OnSceneGUI.
		 */
		public static bool FindNearestVertex(Ray ray, Transform[] targets, out Vector3 vertex)
		{
#if UNITY_EDITOR
			PushCamera(ray, null);

			object[] parameters = new object[] { (Vector2) m_HandleCamera.pixelRect.center, targets, null };

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

		/**
		 *	Similar to FindNearestVertex, except this function returns the first point of contact respecting backface culling.
		 */
		public static bool FindNearestVertex2(Ray ray, GameObject[] targets, out Vector3 point, out Vector3 vertex)
		{
#if UNITY_EDITOR

			if(m_PickGameObjectMethod == null)
				m_PickGameObjectMethod = typeof(HandleUtility).GetMethod(
					"PickGameObject",
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance,
					null,
					new System.Type[] {typeof(Vector2), typeof(bool), typeof(GameObject[]), typeof(GameObject[]) },
					null);

			PushCamera(ray, null);

			object[] pickGameObjectParams = new object[] {
				(Vector2) m_HandleCamera.pixelRect.center,
				false,
				null,
				targets
			};

			object res = m_PickGameObjectMethod.Invoke(null, pickGameObjectParams);

			PopCamera();

			GameObject nearest = res == null ? null : res as GameObject;

			pb_Object pb = nearest != null ? nearest.GetComponent<pb_Object>() : null;

			if(pb != null)
			{
				pb_RaycastHit hit;

				if( pb_HandleUtility.FaceRaycast(ray, pb, out hit) )
				{
					float distance = Mathf.Infinity;
					Vector3 best = Vector3.zero;
					int[] faceVertices = pb.faces[hit.face].distinctIndices;

					for(int i = 0; i < faceVertices.Length; i++)
					{
						Vector3 v = pb.vertices[faceVertices[i]];
						float d = Vector3.Distance(v, hit.point);

						if(d < distance)
						{
							distance = d;
							best = v;
						}
					}

					point = pb.transform.TransformPoint(hit.point);
					vertex = pb.transform.TransformPoint(best);

					return true;
				}
			}
#endif

			point = Vector3.zero;
			vertex = Vector3.zero;

			return false;
		}
	}
}
