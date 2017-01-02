using UnityEngine;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;

namespace ProBuilder2.VR
{
	public class CreateCube : ProBuilderShapeInstantiator
	{
		const float MIN_SIZE = .1f;

		private Vector3[] template = new Vector3[8];
		private Vector3[] positions = new Vector3[24];
		private Vector3 m_StartPoint, m_EndPoint, m_BaseEndPoint, m_Size;
		private Plane m_Plane;
		private pb_Object m_Mesh = null;

		enum State
		{
			Base,
			Height
		}

		State state = State.Base;

		public override bool HandleStart(Transform rayOrigin, Plane drawPlane)
		{
			m_Plane = drawPlane;
			m_StartPoint = GetPointOnPlane(rayOrigin, drawPlane);
			m_EndPoint = m_StartPoint;

			m_Mesh = pb_ShapeGenerator.CubeGenerator(Vector3.one);
			m_GameObject = m_Mesh.gameObject;

			// we'll place vertex positions in world space while drawing
			m_GameObject.transform.position = Vector3.zero;

			m_Size.x = MIN_SIZE;
			m_Size.y = MIN_SIZE;
			m_Size.z = MIN_SIZE;

			m_BaseEndPoint.y = (drawPlane.normal * drawPlane.distance).y;

			UpdateShape();

			return true;
		}

		public override void HandleDrag(Transform rayOrigin)
		{
			if(state == State.Base)
				m_EndPoint = GetPointOnPlane(rayOrigin, m_Plane);
			else
				m_EndPoint = VRMath.CalculateNearestPointRayRay(rayOrigin.position, rayOrigin.forward, m_BaseEndPoint, m_Plane.normal);

			UpdateShape();
		}

		public override bool HandleTriggerRelease(Transform rayOrigin)
		{
			if(state == State.Base)
			{
				state = State.Height;
				return true;
			}
			else
			{
				state = State.Base;
				return false;
			}
		}

		private void UpdateShape()
		{     
			/**
			 * How pb_Constant.CUBE_VERTICES arranges the template
			 *
			 *     4----5 
			 *    /    /|
			 *   /    / |
			 *  7----6  1   
			 *  |    | /
			 *  |    |/
			 *  3----2
			 *
			 */

			Vector3 size = m_EndPoint - m_StartPoint;

			if(state == State.Base)
			{
				m_Size.x = size.x;
				m_Size.z = size.z;

				m_BaseEndPoint.x = m_EndPoint.x;
				m_BaseEndPoint.z = m_EndPoint.z;
			}
			else
			{
				m_Size.y = size.y;
			}

			template[0] = m_StartPoint;
			template[1] = new Vector3(m_StartPoint.x + m_Size.x, 	m_StartPoint.y, m_StartPoint.z);
			template[2] = new Vector3(m_StartPoint.x + m_Size.x, 	m_StartPoint.y, m_StartPoint.z + m_Size.z);
			template[3] = new Vector3(m_StartPoint.x, 				m_StartPoint.y,	m_StartPoint.z + m_Size.z);

			template[4] = new Vector3(m_StartPoint.x, 				m_StartPoint.y + m_Size.y, 	m_StartPoint.z);
			template[5] = new Vector3(m_StartPoint.x + m_Size.x, 	m_StartPoint.y + m_Size.y,	m_StartPoint.z);
			template[6] = new Vector3(m_StartPoint.x + m_Size.x, 	m_StartPoint.y + m_Size.y,	m_StartPoint.z + m_Size.z);
			template[7] = new Vector3(m_StartPoint.x, 				m_StartPoint.y + m_Size.y,	m_StartPoint.z + m_Size.z);

			for(int i = 0; i < pb_Constant.TRIANGLES_CUBE.Length; i++)
				positions[i] = template[pb_Constant.TRIANGLES_CUBE[i]];

			m_Mesh.SetVertices(positions);

			m_Mesh.ToMesh();
			m_Mesh.Refresh();
		}
	}
}
