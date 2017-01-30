using System;
using UnityEngine;
using UnityEngine.InputNew;
using UnityEngine.Experimental.EditorVR;
using UnityEngine.Experimental.EditorVR.Menus;
using UnityEngine.Experimental.EditorVR.Tools;
using UnityEngine.Experimental.EditorVR.Utilities;
using ProBuilder2.Common;

namespace ProBuilder2.VR
{
	[MainMenuItem("Create Shape", "ProBuilder", "Create geometry in the scene")]
	public class CreateShapeTool : 	MonoBehaviour,
									ITool, 
									IStandardActionMap,
									IUsesRayOrigin,
									IUsesRaycastResults,
									IUsesSpatialHash,
									IExclusiveMode,
									IUsesViewerPivot
	{

		[SerializeField] private AudioClip m_TriggerReleased;
		[SerializeField] private AudioClip m_DragAudio;
		[SerializeField] private Material m_HighlightMaterial;

		Shape m_Shape = Shape.Cube;

		public Transform rayOrigin { get; set; }
		public Transform viewerPivot { get; set; }
		public Action<GameObject> addToSpatialHash { get; set; }
		public Action<GameObject> removeFromSpatialHash { get; set; }
	   	public Func<Transform, GameObject> getFirstGameObject { get; set; }

		enum ShapeCreationState
		{
			StartPoint,
			EndPoint
		}

		// shape creation vars
		private ShapeCreationState m_State = ShapeCreationState.StartPoint;
		private GameObject m_CurrentGameObject;
		private AShapeCreator m_CurrentShape;
		private Plane m_Plane = new Plane(Vector3.up, Vector3.zero);
		private pb_Object m_HoveredObject = null;
		private float m_SnapIncrement = Snapping.DEFAULT_INCREMENT;

		private SelectionBoundsModule m_ShapeBounds;
		private VRAudioModule m_AudioModule;
		private GridModule m_GridModule;
		private GuideModule m_GuideModule;

		private RollingAverage_Vector3 rayForwardSmoothed = new RollingAverage_Vector3(Vector3.zero);

		void Start()
		{
			m_ShapeBounds = U.Object.CreateGameObjectWithComponent<SelectionBoundsModule>();
			m_AudioModule = U.Object.CreateGameObjectWithComponent<VRAudioModule>();
			m_GridModule = U.Object.CreateGameObjectWithComponent<GridModule>();
			m_GuideModule = U.Object.CreateGameObjectWithComponent<GuideModule>();

			m_GridModule.SetVisible(false);
		}

		void OnDestroy()
		{
			U.Object.Destroy(m_AudioModule.gameObject);
			U.Object.Destroy(m_ShapeBounds.gameObject);
			U.Object.Destroy(m_GridModule.gameObject);
			U.Object.Destroy(m_GuideModule.gameObject);
		}

		public void ProcessInput(ActionMapInput input, Action<InputControl> consumeControl)
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

		void HandleStartPoint(Standard standardInput, Action<InputControl> consumeControl)
		{
			// HandleStartPoint can be called before Start()?
			if(m_GridModule == null)
				return;

	   		GameObject first = getFirstGameObject(rayOrigin);
	   		pb_Object pb = first != null ? first.GetComponent<pb_Object>() : null;
	   		Vector3 rayCollisionPoint;
	   		
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
				m_GridModule.SetVisible(true);
				m_GridModule.transform.position = Snapping.Snap(rayCollisionPoint, m_SnapIncrement, Vector3.one);
				m_GridModule.transform.localRotation = Quaternion.LookRotation(m_Plane.normal);

				// Calculate the snap increment based on distance from viewer.  Split into 4 segments.
				float distance = Mathf.Clamp(Vector3.Distance(m_GridModule.transform.position, viewerPivot.position), 0f, 10f) / 10f;
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

			if (standardInput.action.wasJustPressed)
			{
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
					addToSpatialHash(m_CurrentShape.gameObject);
					consumeControl(standardInput.action);
				}
			}
		}

		void HandleFinishPoint(Standard standardInput, Action<InputControl> consumeControl)
		{
			if(m_CurrentShape == null)
				return;

			Ray ray = new Ray(rayOrigin.position, rayForwardSmoothed.Add(rayOrigin.forward));

			m_CurrentShape.HandleDrag(ray);

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
