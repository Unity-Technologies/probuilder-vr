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
	public class CreateShapeTool : MonoBehaviour, ITool, IStandardActionMap, IConnectInterfaces, IInstantiateMenuUI, IUsesRayOrigin, IUsesSpatialHash, IExclusiveMode
	{
		[SerializeField]
		CreateShapeMenu m_ShapeMenuPrefab;
		GameObject m_ShapeMenu;
		Shape m_Shape = Shape.Cube;

		[SerializeField] GameObject m_PlaneVisualPrefab;
		GameObject m_PlaneVisual;

		public Func<Transform, IMenu, GameObject> instantiateMenuUI { private get; set; }
		public Transform rayOrigin { get; set; }
		public ConnectInterfacesDelegate connectInterfaces { private get; set; }
		public Action<GameObject> addToSpatialHash { get; set; }
		public Action<GameObject> removeFromSpatialHash { get; set; }

		enum ShapeCreationState
		{
			StartPoint,
			EndPoint
		}

		// shape creation vars
		ShapeCreationState m_State = ShapeCreationState.StartPoint;
		GameObject m_CurrentGameObject;
		AShapeCreator m_CurrentShape;
		Plane m_Plane = new Plane(Vector3.up, Vector3.zero);

		void Start()
		{
			m_ShapeMenu = instantiateMenuUI(rayOrigin, m_ShapeMenuPrefab);
			var menu = m_ShapeMenu.GetComponent<CreateShapeMenu>();
			connectInterfaces(menu, rayOrigin);
			menu.selectPrimitive = SetSelectedPrimitive;

			m_PlaneVisual = U.Object.Instantiate(m_PlaneVisualPrefab);
			m_PlaneVisual.SetActive(false);
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

		void HandleStartPoint(Standard standardInput, Action<InputControl> consumeControl)
		{
			if(m_PlaneVisual == null)
				return;

			Vector3 p;

			if( VRMath.GetPointOnPlane(rayOrigin, m_Plane, out p) )
			{
				m_PlaneVisual.SetActive(true);
				m_PlaneVisual.transform.position = p;
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

			if (standardInput.action.wasJustReleased)
			{
				if(!m_CurrentShape.HandleTriggerRelease(rayOrigin))
					m_State = ShapeCreationState.StartPoint;

				consumeControl(standardInput.action);
			}
		}

		void OnDestroy()
		{
			U.Object.Destroy(m_ShapeMenu);	
			U.Object.Destroy(m_PlaneVisual);	
		}
	}
}
