using System;
using UnityEngine;
using UnityEngine.InputNew;
using UnityEditor.Experimental.EditorVR;
using UnityEditor.Experimental.EditorVR.Proxies;
using UnityEditor.Experimental.EditorVR.Utilities;
using ProBuilder2.Common;

namespace ProBuilder2.VR
{
	[MainMenuItem("Create Shape", "ProBuilder", "Create geometry in the scene")]
	public class CreateShapeTool : 	ProBuilderToolBase,
									ITool,
									IProcessInput,
									IStandardActionMap,
									IUsesRayOrigin,
									IUsesRaycastResults,
									IUsesSpatialHash,
									IUsesCameraRig
	{

		[SerializeField] private AudioClip m_TriggerReleased;
		[SerializeField] private AudioClip m_DragAudio;
		[SerializeField] private Material m_HighlightMaterial;

		[SerializeField] private DefaultProxyRay m_ProxyRayPrefab;

		Shape m_Shape = Shape.Cube;

		public Transform cameraRig { get; set; }

		public ActionMap standardActionMap
		{
			set
			{
				m_StandardActionMap = value;
			}
		}

		enum ShapeCreationState
		{
			StartPoint,
			EndPoint
		}

#pragma warning disable 414
		ActionMap m_StandardActionMap;
#pragma warning restore 414

		// shape creation vars
		ShapeCreationState m_State = ShapeCreationState.StartPoint;
		GameObject m_CurrentGameObject;
		AShapeCreator m_CurrentShape;
		Plane m_Plane = new Plane(Vector3.up, Vector3.zero);
		pb_Object m_HoveredObject = null;
		float m_SnapIncrement = Snapping.DEFAULT_INCREMENT;

		SelectionBoundsModule m_ShapeBounds;
		VRAudioModule m_AudioModule;
		GridModule m_GridModule;
		GuideModule m_GuideModule;
		DefaultProxyRay m_ProxyRay;

		private RollingAverage_Vector3 rayForwardSmoothed = new RollingAverage_Vector3(Vector3.zero);

		public override void pb_Start()
		{
			m_ShapeBounds = ObjectUtils.CreateGameObjectWithComponent<SelectionBoundsModule>();
			m_AudioModule = ObjectUtils.CreateGameObjectWithComponent<VRAudioModule>();
			m_GridModule = ObjectUtils.CreateGameObjectWithComponent<GridModule>();
			m_GuideModule = ObjectUtils.CreateGameObjectWithComponent<GuideModule>();
			m_GridModule.SetVisible(false);

			m_ProxyRay = ObjectUtils.Instantiate(m_ProxyRayPrefab.gameObject).GetComponent<DefaultProxyRay>();
			m_ProxyRay.transform.position = Vector3.zero;
			m_ProxyRay.transform.localRotation = Quaternion.identity;
			m_ProxyRay.transform.SetParent(rayOrigin, false);

			//this.HideDefaultRay(rayOrigin);
			//this.LockRay(rayOrigin, this);
		}

		public override void pb_OnDestroy()
		{
			if (m_AudioModule != null) ObjectUtils.Destroy(m_AudioModule.gameObject);
			if (m_ShapeBounds != null) ObjectUtils.Destroy(m_ShapeBounds.gameObject);
			if (m_GridModule != null) ObjectUtils.Destroy(m_GridModule.gameObject);
			if (m_GuideModule != null) ObjectUtils.Destroy(m_GuideModule.gameObject);
			// EditorVR is freeing this ?
			//ObjectUtils.Destroy(m_ProxyRay.gameObject);

			//this.ShowDefaultRay(rayOrigin);
			//this.UnlockRay(rayOrigin, this);
		}

		public void ProcessInput(ActionMapInput input, ConsumeControlDelegate consumeControl)
		{
			var standardInput = (Standard)input;

			switch (m_State)
			{
				case ShapeCreationState.StartPoint:
					{
						HandleStartPoint(standardInput, consumeControl);
						break;
					}
				case ShapeCreationState.EndPoint:
					{
						HandleFinishPoint(standardInput, consumeControl);
						break;
					}
			}
		}

		void SetSelectedPrimitive(Shape shape)
		{
			m_Shape = shape;
		}

		private pb_RaycastHit m_RaycastHit;

		void HandleStartPoint(Standard standardInput, ConsumeControlDelegate consumeControl)
		{
			// HandleStartPoint can be called before Start()?
			if (m_GridModule == null)
				return;

	   		GameObject first = this.GetFirstGameObject(rayOrigin);
	   		pb_Object pb = first != null ? first.GetComponent<pb_Object>() : null;
	   		Vector3 rayCollisionPoint = Vector3.zero;

			Ray ray = new Ray(rayOrigin.position, rayForwardSmoothed.Add(rayOrigin.forward));

	   		if(pb != null && pb_HandleUtility.FaceRaycast(ray, pb, out m_RaycastHit))
	   		{
	   			if(m_HoveredObject == null)
		   			m_HoveredObject = pb;

		   		rayCollisionPoint = pb.transform.TransformPoint(m_RaycastHit.point);
		   		rayCollisionPoint = Snapping.Snap(rayCollisionPoint, m_SnapIncrement, Vector3.one);
		   		m_Plane.SetNormalAndPosition( pb.transform.TransformDirection(m_RaycastHit.normal).normalized, rayCollisionPoint );
	   		}
	   		else
	   		{
	   			if(m_HoveredObject != null)
	   			{
	   				m_HoveredObject = null;
	   				m_Plane.SetNormalAndPosition(Vector3.up, Vector3.zero);
	   			}
	   		}

			if( m_HoveredObject != null || VRMath.GetPointOnPlane(ray, m_Plane, out rayCollisionPoint) )
			{
				m_ProxyRay.SetLength(Vector3.Distance(rayCollisionPoint, ray.origin));

				m_GridModule.SetVisible(true);
				m_GridModule.transform.position = Snapping.Snap(rayCollisionPoint, m_SnapIncrement, Vector3.one);
				m_GridModule.transform.localRotation = Quaternion.LookRotation(m_Plane.normal);

				// Calculate the snap increment based on distance from viewer.  Split into 4 segments.
				float distance = Mathf.Clamp(Vector3.Distance(m_GridModule.transform.position, cameraRig.position), 0f, 10f) / 10f;
				m_SnapIncrement = Snapping.DEFAULT_INCREMENT * System.Math.Max(1, Mathf.Pow(2, (int)(distance * 4)));

				m_GridModule.SetSnapIncrement(m_SnapIncrement);
				AShapeCreator.SetSnapIncrement(m_SnapIncrement);

				m_GuideModule.transform.position = m_GridModule.transform.position;
				m_GuideModule.transform.localRotation = m_GridModule.transform.localRotation;
			}
			else
			{
				m_GridModule.SetVisible(false);
			}

			if (standardInput != null && standardInput.action.wasJustPressed)
			{
				// @todo
				switch(m_Shape)
				{
					default:
						m_CurrentShape = new CreateCube();
						break;
				}

				// If shape initialization failed no gameobject will have been created,
				// so don't worry about cleaning up.
				if( m_CurrentShape.HandleStart(ray, m_Plane) )
				{
					m_AudioModule.Play(m_TriggerReleased, true);
					m_CurrentShape.onShapeChanged = (v) =>
					{
						m_GuideModule.transform.position = v;
						m_AudioModule.Play(m_DragAudio);
					};
					m_CurrentShape.gameObject.GetComponent<MeshRenderer>().sharedMaterial = m_HighlightMaterial;
					m_State = ShapeCreationState.EndPoint;
					this.AddToSpatialHash(m_CurrentShape.gameObject);
					consumeControl(standardInput.action);
				}
			}
		}

		void HandleFinishPoint(Standard standardInput, ConsumeControlDelegate consumeControl)
		{
			if(m_CurrentShape == null)
				return;

			Ray ray = new Ray(rayOrigin.position, rayForwardSmoothed.Add(rayOrigin.forward));

			Vector3 planeIntersect = Vector3.zero;

			if( m_CurrentShape.HandleDrag(ray, ref planeIntersect) )
			{
				m_ProxyRay.SetLength(Vector3.Distance(planeIntersect, rayOrigin.position));
			}

			m_ShapeBounds.SetHighlight(m_CurrentShape.pbObject, true);

			if (standardInput.action.wasJustReleased)
			{
				m_AudioModule.Play(m_TriggerReleased, true);

				if(!m_CurrentShape.HandleTriggerRelease(ray))
					m_State = ShapeCreationState.StartPoint;

				m_ShapeBounds.SetHighlight(m_CurrentShape.pbObject, false);
			}

			consumeControl(standardInput.action);
		}
	}
}
