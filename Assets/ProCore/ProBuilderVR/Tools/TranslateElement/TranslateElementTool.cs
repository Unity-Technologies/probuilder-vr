using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputNew;
using UnityEditor.Experimental.EditorVR;
using UnityEditor.Experimental.EditorVR.Utilities;
using UnityEditor.Experimental.EditorVR.Proxies;
using ProBuilder2.Common;

namespace ProBuilder2.VR
{
	/**
	 * Translates faces along their normal.
	 */
	[MainMenuItem("Move Elements", "ProBuilder", "Translate selected mesh elements.")]
	public class TranslateElementTool : ProBuilderToolBase,
										ITool,
										IProcessInput,
										IStandardActionMap,
										IUsesRaycastResults,
										ISetHighlight
	{
		[SerializeField] private AudioClip m_Drag;
		[SerializeField] private AudioClip m_Trigger;

		enum CreateState
		{
			Start,
			Finish
		}

#pragma warning disable 414
		private ActionMap m_StandardActionMap;
#pragma warning restore 414

		public Color32 highlightFaceColor = new Color32(0, 188, 212, 96);
		public Color32 directSelectFaceColor = new Color32(212, 0, 171, 96);

		private HighlightElementsModule m_HighlightModule = null;
		private VRAudioModule m_AudioModule = null;
		private HeightGuideModule m_GuideModule = null;

		const float MAX_TRANSLATE_DISTANCE = 100f;
		private static readonly Vector3 VECTOR3_ONE = Vector3.one;
		private float m_SnapIncrement = Snapping.DEFAULT_INCREMENT;
		private float m_DirectSelectThreshold = .1f;

	   	private CreateState m_State = CreateState.Start;
	   	private bool m_Dragging = false;
	   	private bool m_IsDirectSelect = false;
	   	private Vector3 m_DragOrigin = Vector3.zero;
		private Vector3 m_DragDirection = Vector3.zero;
		private Vector3 m_Offset = Vector3.zero;
		private Vector3 m_DraggedPoint = Vector3.zero;
		private GameObject[] m_ProBuilderObjectsInScene = null;
		private Vector3 m_PreviousVertexTranslation = Vector3.zero;
		private VertexSnap m_VertexSnap = new VertexSnap(false, Vector3.zero);

		private pb_Object m_Object;
		private pb_Face m_Face;
		private IEnumerable<int> m_SelectedIndices;
		private Vector3[] m_Positions;
		private Vector3[] m_SettingPositions;

		public ActionMap standardActionMap
		{
			set
			{
				m_StandardActionMap = value;
			}
		}

		public override void pb_Start()
		{
			m_HighlightModule = ObjectUtils.CreateGameObjectWithComponent<HighlightElementsModule>();
			m_GuideModule = ObjectUtils.CreateGameObjectWithComponent<HeightGuideModule>();
			m_AudioModule = ObjectUtils.CreateGameObjectWithComponent<VRAudioModule>();
			m_GuideModule.SetVisible(false);
			try { m_DirectSelectThreshold = Mathf.Max(m_DirectSelectThreshold, rayOrigin.GetComponentInChildren<DefaultProxyRay>().pointerLength * 1.3f); } catch {}
		}

		public override void pb_OnDestroy()
		{
			if(m_HighlightModule != null) ObjectUtils.Destroy(m_HighlightModule.gameObject);
			if(m_AudioModule != null) ObjectUtils.Destroy(m_AudioModule.gameObject);
			if(m_GuideModule != null) ObjectUtils.Destroy(m_GuideModule.gameObject);
		}

		public void ProcessInput(ActionMapInput input, ConsumeControlDelegate consumeControl)
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

		public override void pb_OnSceneGUI(EditorWindow win)
		{
			if(Event.current.type != EventType.MouseMove)
				return;

			if (m_State == CreateState.Finish && m_Object != null)
			{
				Vector3 collision;

				m_VertexSnap.valid = Snapping.FindNearestVertex2(new Ray(rayOrigin.position, rayOrigin.forward), m_ProBuilderObjectsInScene, out collision, out m_VertexSnap.point);

				if(m_VertexSnap.valid)
				{
					m_VertexSnap.valid = Vector3.Distance(collision, m_VertexSnap.point) < VertexSnap.MAX_VERTEX_SNAP_DISTANCE;
					// m_VertexSnap.valid = HandleUtility.DistancePointLine(m_VertexSnap.point, rayOrigin.position, rayOrigin.position + rayOrigin.forward * 100f) < VertexSnap.MAX_VERTEX_SNAP_DISTANCE;
				}
			}
			else
			{
				m_VertexSnap.valid = false;
			}
		}

		private void HandleStart(Standard input, ConsumeControlDelegate consumeControl)
		{
			GameObject first = this.GetFirstGameObject(rayOrigin);

			if(first == null)
			{
				if(m_HighlightModule != null)
					m_HighlightModule.Clear();

				return;
			}

			pb_Object pb = first.GetComponent<pb_Object>();

			if(pb == null)
			{
				m_HighlightModule.Clear();
				return;
			}

			pb_RaycastHit hit;

			if( pb_HandleUtility.FaceRaycast(new Ray(rayOrigin.position, rayOrigin.forward), pb, out hit) )
			{
				m_IsDirectSelect = hit.distance < m_DirectSelectThreshold;

				if(m_HighlightModule != null)
				{
					m_HighlightModule.color = m_IsDirectSelect ? directSelectFaceColor : highlightFaceColor;
					m_HighlightModule.SetFaceHighlight(pb, new pb_Face[] { pb.faces[hit.face] }, true);
				}

				this.SetHighlight(pb.gameObject, false);

				consumeControl(input.action);

				if(!input.action.wasJustPressed)
					return;

				m_AudioModule.Play(m_Trigger, true);

				m_Object = pb;
				m_Face = pb.faces[hit.face];
				m_SelectedIndices = pb.sharedIndices.AllIndicesWithValues(m_Face.distinctIndices);

				m_DragOrigin = pb.transform.TransformPoint(hit.point);
				m_DragDirection = pb.transform.TransformDirection(hit.normal);
				m_DragDirection.Normalize();

				if(m_GuideModule != null)
				{
					m_GuideModule.SetVisible(true);
					m_GuideModule.transform.position = m_DragOrigin;
					m_GuideModule.transform.rotation = Quaternion.LookRotation(m_DragDirection);
				}

				m_State = CreateState.Finish;
				m_ProBuilderObjectsInScene = UnityEngine.Object.FindObjectsOfType<pb_Object>().Where(x => x != m_Object).Select(y => y.gameObject).ToArray();

				m_Positions = new Vector3[pb.vertexCount];
				m_SettingPositions = new Vector3[pb.vertexCount];
				System.Array.Copy(pb.vertices, m_Positions, pb.vertexCount);
				System.Array.Copy(pb.vertices, m_SettingPositions, pb.vertexCount);

				m_Object.ToMesh();
			}
			else
			{
				if(m_HighlightModule != null)
					m_HighlightModule.SetFaceHighlight(pb, null);
			}
		}

		private void HandleFinish(Standard input, ConsumeControlDelegate consumeControl)
		{
			// Ready for next object to be created
			if (input.action.wasJustReleased)
			{
				m_AudioModule.Play(m_Trigger, true);
				m_GuideModule.SetVisible(false);
				m_HighlightModule.SetFaceHighlight(m_Object, null);

				m_Dragging = false;
				m_State = CreateState.Start;
				m_Object.ToMesh();
				m_Object.Refresh();
			}
			else
			{
				if(m_IsDirectSelect)
					m_DraggedPoint = m_DragOrigin + (Vector3.Project(rayOrigin.position - m_DragOrigin, m_DragDirection));
				else if(m_VertexSnap.valid)
					m_DraggedPoint = m_DragOrigin + (Vector3.Project(m_VertexSnap.point - m_DragOrigin, m_DragDirection));
				else
					m_DraggedPoint = VRMath.CalculateNearestPointRayRay(m_DragOrigin, m_DragDirection, rayOrigin.position, rayOrigin.forward);

				if(!m_Dragging)
				{
					m_Offset = m_IsDirectSelect ? m_DraggedPoint - m_DragOrigin : Vector3.zero;
					m_Dragging = true;
				}

				m_DraggedPoint -= m_Offset;

				Vector3 localDragOrigin = m_Object.transform.InverseTransformPoint(m_DragOrigin);
				Vector3 localDraggedPoint = m_Object.transform.InverseTransformPoint(m_DraggedPoint);
				Vector3 vertexTranslation = localDraggedPoint - localDragOrigin;

				if(vertexTranslation.magnitude > MAX_TRANSLATE_DISTANCE)
					vertexTranslation = vertexTranslation.normalized * MAX_TRANSLATE_DISTANCE;

				vertexTranslation = Snapping.Snap(vertexTranslation, m_SnapIncrement, VECTOR3_ONE);

				if(vertexTranslation != m_PreviousVertexTranslation)
				{
					m_PreviousVertexTranslation = vertexTranslation;
					m_AudioModule.Play(m_Drag);
				}

				foreach(int ind in m_SelectedIndices)
					m_SettingPositions[ind] = m_Positions[ind] + vertexTranslation;

				m_Object.SetVertices(m_SettingPositions);
				m_Object.msh.vertices = m_SettingPositions;
				m_Object.RefreshUV();
				m_Object.msh.RecalculateBounds();
				m_HighlightModule.UpdateVertices(m_Object);
			}

			this.SetHighlight(m_Object.gameObject, false);
			consumeControl(input.action);
		}
	}
}
