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
									IConnectInterfaces,
									IInstantiateMenuUI,
									IUsesRayOrigin,
									IUsesRaycastResults,
									IUsesSpatialHash,
									IExclusiveMode, 
									IUsesViewerPivot
	{
		[SerializeField]
		CreateShapeMenu m_ShapeMenuPrefab;
		GameObject m_ShapeMenu;
		Shape m_Shape = Shape.Cube;

		[SerializeField] GameObject m_PlaneVisualPrefab;
		GameObject m_PlaneVisual;

		public Func<Transform, IMenu, GameObject> instantiateMenuUI { private get; set; }
		public Transform rayOrigin { get; set; }
		public Transform viewerPivot { get; set; }
		public ConnectInterfacesDelegate connectInterfaces { private get; set; }
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
		private SelectionBoundsModule m_ShapeBounds;
		private Plane m_Plane = new Plane(Vector3.up, Vector3.zero);
		private pb_Object m_HoveredObject = null;

		void Start()
		{
			m_ShapeMenu = instantiateMenuUI(rayOrigin, m_ShapeMenuPrefab);
			var menu = m_ShapeMenu.GetComponent<CreateShapeMenu>();
			connectInterfaces(menu, rayOrigin);
			menu.selectPrimitive = SetSelectedPrimitive;

			m_ShapeBounds = U.Object.CreateGameObjectWithComponent<SelectionBoundsModule>();

			m_PlaneVisual = U.Object.Instantiate(m_PlaneVisualPrefab);
			m_PlaneVisual.SetActive(false);
		}

		void OnDestroy()
		{
			U.Object.Destroy(m_ShapeMenu);	
			U.Object.Destroy(m_PlaneVisual);	
			U.Object.Destroy(m_ShapeBounds.gameObject);
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
			if(m_PlaneVisual == null)
				return;

	   		GameObject first = getFirstGameObject(rayOrigin);
	   		pb_Object pb = first != null ? first.GetComponent<pb_Object>() : null;
	   		Vector3 rayCollisionPoint;

	   		if(pb != null && pb_HandleUtility.FaceRaycast(new Ray(rayOrigin.position, rayOrigin.forward), pb, out m_RaycastHit))
	   		{
	   			if(m_HoveredObject == null)
		   			m_HoveredObject = pb;

		   		rayCollisionPoint = pb.transform.TransformPoint(m_RaycastHit.point);
		   		rayCollisionPoint = Snapping.Snap(rayCollisionPoint, Snapping.DEFAULT_INCREMENT, Vector3.one);
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

			if( m_HoveredObject != null || VRMath.GetPointOnPlane(rayOrigin, m_Plane, out rayCollisionPoint) )
			{
				m_PlaneVisual.SetActive(true);
				m_PlaneVisual.transform.position = Snapping.Snap(rayCollisionPoint, Snapping.DEFAULT_INCREMENT, Vector3.one);
				m_PlaneVisual.transform.localRotation = Quaternion.LookRotation(m_Plane.normal);
			}
			else
			{
				m_PlaneVisual.SetActive(false);
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
				if( m_CurrentShape.HandleStart(rayOrigin, m_Plane) )
				{
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

			m_CurrentShape.HandleDrag(rayOrigin);

			m_ShapeBounds.SetHighlight(m_CurrentShape.pbObject, true);

			if (standardInput.action.wasJustReleased)
			{
				if(!m_CurrentShape.HandleTriggerRelease(rayOrigin))
					m_State = ShapeCreationState.StartPoint;

				m_ShapeBounds.SetHighlight(m_CurrentShape.pbObject, false);
			}
			
			consumeControl(standardInput.action);
		}
	}
}
