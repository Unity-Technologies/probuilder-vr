using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputNew;
using UnityEngine.Experimental.EditorVR;
using UnityEngine.Experimental.EditorVR.Menus;
using UnityEngine.Experimental.EditorVR.Tools;
using UnityEngine.Experimental.EditorVR.Utilities;
using ProBuilder2.Common;

namespace ProBuilder2.VR
{
	[MainMenuItem("Highlight Faces", "ProBuilder", "Highlight the face a pointer is currently hovering")]
	public class HighlightFaceTool : MonoBehaviour, ITool, IStandardActionMap, IUsesRayOrigin, IUsesRaycastResults, IExclusiveMode
	{
		public Transform rayOrigin { get; set; }
	   	public Func<Transform, GameObject> getFirstGameObject { get; set; }

		[SerializeField] GameObject pointer;
		private GameObject m_Pointer;
		private HighlightElementsModule m_HighlightModule = null;

	   	enum CreateState
	   	{
	   		Start,
	   		Finish
	   	}

	   	CreateState m_State = CreateState.Start;
	   	Vector3 m_DragOrigin = Vector3.zero;
		Vector3 m_DragDirection = Vector3.zero;
		Vector3 m_DraggedPoint = Vector3.zero;

		pb_Object m_Object;
		pb_Face m_Face;
		IEnumerable<int> m_SelectedIndices;
		Vector3[] m_Positions;
		Vector3[] m_SettingPositions;

		void Start()
		{
			Selection.objects = new UnityEngine.Object[0];
			m_HighlightModule = U.Object.CreateGameObjectWithComponent<HighlightElementsModule>();
		}

		void OnDestroy()
		{
			U.Object.Destroy(m_HighlightModule.gameObject);
		}

		public void ProcessInput(ActionMapInput input, Action<InputControl> consumeControl)
		{
			if(m_State == CreateState.Start)
			{
				HandleStart( (Standard) input, consumeControl );
			}
			else if(m_State == CreateState.Finish)
			{
				HandleFinish( (Standard) input, consumeControl );
			}
		}

		private void HandleStart(Standard input, Action<InputControl> consumeControl)
		{
			Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
			pb_RaycastHit hit;
			GameObject first = getFirstGameObject(rayOrigin);
			if(first == null)	
				return;
			pb_Object pb = first.GetComponent<pb_Object>();
			if(pb == null)	
				return;

			if( pb_HandleUtility.FaceRaycast(ray, pb, out hit) )
			{
				m_HighlightModule.SetFaceHighlight(pb, new pb_Face[] { pb.faces[hit.face] } );

				if(!input.action.wasJustPressed)
					return;

				m_Object = pb;
				m_Face = pb.faces[hit.face];
				m_SelectedIndices = pb.sharedIndices.AllIndicesWithValues(m_Face.distinctIndices);

				m_DragOrigin = pb.transform.TransformPoint(hit.point);
				m_DragDirection = pb.transform.TransformDirection(hit.normal);

				m_Pointer = U.Object.Instantiate(pointer);
				m_Pointer.transform.localRotation = Quaternion.FromToRotation(Vector3.up, m_DragDirection);

				m_State = CreateState.Finish;

				m_Positions = new Vector3[pb.vertexCount];
				m_SettingPositions = new Vector3[pb.vertexCount];
				System.Array.Copy(pb.vertices, m_Positions, pb.vertexCount);
				System.Array.Copy(pb.vertices, m_SettingPositions, pb.vertexCount);

				m_Object.ToMesh();

				consumeControl(input.action);
			}
			else
			{
				m_HighlightModule.SetFaceHighlight(pb, null);
			}
		}

		private void HandleFinish(Standard input, Action<InputControl> consumeControl)
		{
			// Ready for next object to be created
			if (input.action.wasJustReleased)
			{
				U.Object.Destroy(m_Pointer);
				m_HighlightModule.SetFaceHighlight(m_Object, null);

				m_State = CreateState.Start;

				m_Object.ToMesh();
				m_Object.Refresh();

				consumeControl(input.action);
			}
			else
			{
				m_DraggedPoint = CalculateNearestPointRayRay(m_DragOrigin, m_DragDirection, rayOrigin.position, rayOrigin.forward);
				m_Pointer.transform.position = m_DraggedPoint;

				Vector3 localDragOrigin = m_Object.transform.InverseTransformPoint(m_DragOrigin);
				Vector3 localDraggedPoint = m_Object.transform.InverseTransformPoint(m_DraggedPoint);

				foreach(int ind in m_SelectedIndices)
					m_SettingPositions[ind] = m_Positions[ind] + (localDraggedPoint - localDragOrigin);

				m_Object.SetVertices(m_SettingPositions);
				m_Object.msh.vertices = m_SettingPositions;
				m_HighlightModule.UpdateVertices(m_Object);
			}
		}

		private Vector3 CalculateNearestPointRayRay(Vector3 ao, Vector3 ad, Vector3 bo, Vector3 bd)
		{
			// ray-ray don't do parallel
			if(ad == bd)	
				return ao;

			Vector3 c = (bo - ao).normalized;

			float n = -Vector3.Dot(ad, bd) * Vector3.Dot(bd, c) + Vector3.Dot(ad, c) * Vector3.Dot(bd, bd);
			float d = Vector3.Dot(ad, ad) * Vector3.Dot(bd, bd) - Vector3.Dot(ad, bd) * Vector3.Dot(ad, bd);

			Vector3 v = ao + ad * (n/d);
			
			return v;
		}
	}
}
