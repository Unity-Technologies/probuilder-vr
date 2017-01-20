using UnityEngine;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;

namespace ProBuilder2.VR
{
	public class CreateCube : AShapeCreator
	{
		const float MIN_SIZE = .01f;

		private Vector3[] template = new Vector3[8];
		private Vector3[] positions = new Vector3[24];
		private Vector3 m_StartPoint, m_EndPoint, m_BaseEndPoint, m_Size;
		private Plane m_Plane;
		private pb_Object m_Mesh = null;
		private bool m_FacesReversed = false;
		private static readonly Vector3 VECTOR3_ONE = Vector3.one;
		private Quaternion m_Orientation = Quaternion.identity;

		enum State
		{
			Base,
			Height
		}

		State state = State.Base;

		public override bool HandleStart(Transform rayOrigin, Plane drawPlane)
		{
			m_Plane = drawPlane;
			m_Orientation = Quaternion.Inverse(Quaternion.LookRotation(Vector3.up)) * Quaternion.LookRotation(m_Plane.normal);

			if(!VRMath.GetPointOnPlane(rayOrigin, m_Plane, out m_StartPoint))
				return false;

			m_EndPoint = Snapping.Snap(m_StartPoint, m_SnapIncrement, VECTOR3_ONE);
			m_StartPoint = m_EndPoint;

			m_Mesh = pb_ShapeGenerator.CubeGenerator(VECTOR3_ONE);

			foreach(pb_Face face in m_Mesh.faces)
				face.uv.useWorldSpace = true;

			m_GameObject = m_Mesh.gameObject;

			// we'll place vertex positions in world space while drawing
			m_GameObject.transform.position = Vector3.zero;

			m_Size.x = MIN_SIZE;
			m_Size.y = MIN_SIZE;
			m_Size.z = MIN_SIZE;

			UpdateShape();

			return true;
		}

		public override void HandleDrag(Transform rayOrigin)
		{
			Vector3 endPoint;

			if(state == State.Base)
				VRMath.GetPointOnPlane(rayOrigin, m_Plane, out endPoint);
			else
				endPoint = VRMath.CalculateNearestPointRayRay(rayOrigin.position, rayOrigin.forward, m_BaseEndPoint, m_Plane.normal);

			// Apply simple smoothing to ray input.
			m_EndPoint = Vector3.Lerp(m_EndPoint, endPoint, .5f);
			m_EndPoint = Snapping.Snap(m_EndPoint, m_SnapIncrement, VECTOR3_ONE);

			UpdateShape();
		}

		public override bool HandleTriggerRelease(Transform rayOrigin)
		{
			if(state == State.Base)
			{
				m_FacesReversed = false;
				state = State.Height;
				m_BaseEndPoint = m_EndPoint;
				return true;
			}
			else
			{
				m_Mesh.CenterPivot(null);
				state = State.Base;
				return false;
			}
		}

		private void UpdateShape()
		{     
			/**
			 * How pb_Constant.CUBE_VERTICES arranges the template
			 *
			 *     4-------5 
			 *    /       /|
			 *   /       / |
			 *  7-------6  1   
			 *  |       | /
			 *  |       |/
			 *  3-------2
			 *
			 */

			Vector3 size = m_EndPoint - m_StartPoint;
			m_Size = size;

			// if(state == State.Base)
			// {
			// 	m_Size.x = size.x;
			// 	m_Size.z = size.z;

			// 	bool isFlipped = !(m_Size.x < 0 ^ m_Size.z < 0);

			// 	if(isFlipped != m_FacesReversed)
			// 	{
			// 		m_FacesReversed = isFlipped;
			// 		m_Mesh.ReverseWindingOrder(m_Mesh.faces);
			// 	}
			// }
			// else
			// {
			// 	m_Size.y = size.y;

			// 	bool isFlipped = m_Size.y < 0;

			// 	if(isFlipped != m_FacesReversed)
			// 	{
			// 		m_FacesReversed = isFlipped;
			// 		m_Mesh.ReverseWindingOrder(m_Mesh.faces);
			// 	}
			// }

			template[0] = m_StartPoint;
			template[1] = m_StartPoint + Vector3.Scale(new Vector3(1f, 0f, 0f), m_Size);
			template[2] = m_StartPoint + Vector3.Scale(new Vector3(1f, 0f, 1f), m_Size);
			template[3] = m_StartPoint + Vector3.Scale(new Vector3(0f, 0f, 1f), m_Size);

			template[4] = m_StartPoint + Vector3.Scale(new Vector3(0f, 1f, 0f), m_Size);
			template[5] = m_StartPoint + Vector3.Scale(new Vector3(1f, 1f, 0f), m_Size);
			template[6] = m_StartPoint + Vector3.Scale(new Vector3(1f, 1f, 1f), m_Size);
			template[7] = m_StartPoint + Vector3.Scale(new Vector3(0f, 1f, 1f), m_Size);

			int len = pb_Constant.TRIANGLES_CUBE.Length;

			for(int i = 0; i < len; i++)
				positions[i] = template[pb_Constant.TRIANGLES_CUBE[i]];

			m_Mesh.SetVertices(positions);

			m_Mesh.ToMesh();
			m_Mesh.Refresh();
		}
	}
}
