using UnityEngine;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;

namespace ProBuilder2.VR
{
	public class CreateCube : ProBuilderShapeInstantiator
	{
		Vector3 m_StartPoint, m_EndPoint;
		Plane m_Plane;

		public override bool HandleStart(Transform rayOrigin, Plane drawPlane)
		{
			m_Plane = drawPlane;
			m_StartPoint = GetPointOnPlane(rayOrigin, drawPlane);
			m_EndPoint = m_StartPoint;


			pb_Object pb = pb_ShapeGenerator.CubeGenerator(Vector3.one);
			pb.CenterPivot(new int[] {0});
			m_GameObject = pb.gameObject;
			m_GameObject.transform.position = m_StartPoint;

			UpdateShape();

			return true;
		}

		public override void HandleDrag(Transform rayOrigin)
		{
			m_EndPoint = GetPointOnPlane(rayOrigin, m_Plane);
			UpdateShape();
		}

		public override bool HandleTriggerRelease(Transform rayOrigin)
		{
			return false;
		}

		private void UpdateShape()
		{
			float scale = Mathf.Max(.25f, (m_EndPoint - m_StartPoint).magnitude );
			m_GameObject.transform.localScale = Vector3.one * scale;
		}
	}
}
