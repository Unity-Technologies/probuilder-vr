using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputNew;
using UnityEngine.Experimental.EditorVR.Menus;
using UnityEngine.Experimental.EditorVR.Helpers;
using UnityEngine.Experimental.EditorVR.Tools;
using UnityEngine.Experimental.EditorVR.Utilities;
using UnityEngine.Experimental.EditorVR.Manipulators;

namespace ProBuilder2.VR
{
	[MainMenuItem("Grabby Locomotor", "ProBuilder", "Maneuver around the scene.")]
	public class GrabbyLocomotionTool : MonoBehaviour,
										ITool,
										ICustomActionMap,
										IUsesRayOrigin,
										IUsesViewerPivot,
										IUsesRaycastResults,
										IExclusiveMode
	{
		public Transform viewerPivot { private get; set; }
		public Transform rayOrigin { get; set; }
	   	public Func<Transform, GameObject> getFirstGameObject { get; set; }

		public ActionMap actionMap { get { return m_GrabbyActionMap; } }

		public DirectManipulator directManipulator { get { return m_DirectManipulator; } }
		[SerializeField]
		private DirectManipulator m_DirectManipulator;

		[SerializeField]
		private ActionMap m_GrabbyActionMap;

		private Vector3 m_GrabTarget = Vector3.zero;
		private bool m_GrabbyEngaged = false;
		private float m_DistanceFromTarget = 0f;
		private Vector3 m_LastRay = Vector3.up;

		void Start()
		{
		}

		void OnDestroy()
		{
		}
		
		public void ProcessInput(ActionMapInput input, Action<InputControl> consumeControl)
		{
			GrabbyLocomotion grabInput = (GrabbyLocomotion) input;

			if(grabInput.trigger2.wasJustPressed)
			{
				// engage grabby
				GameObject first = getFirstGameObject(rayOrigin);

				if( first != null)
				{
					DragStart(first.transform);
					m_GrabbyEngaged = true;
				}
			}
			else if(grabInput.trigger2.wasJustReleased)
			{
				// disengage grabby
				DragEnd();
			}

			if(m_GrabbyEngaged)
				Dragging();
		}

		void DragStart(Transform target)
		{
			m_GrabTarget = target.GetComponent<MeshRenderer>().bounds.center;
			m_DistanceFromTarget = Vector3.Distance(viewerPivot.position, target.position);
			m_LastRay = rayOrigin.forward;
		}

		void Dragging()
		{
			float angle = Vector3.Angle(m_LastRay, rayOrigin.forward);

			Vector3 cross = Vector3.Cross(m_LastRay, Vector3.up);

			if(Vector3.Dot(rayOrigin.forward, cross) > 0f)
				angle = -angle;

			viewerPivot.RotateAround(m_GrabTarget, Vector3.up, angle);

			m_LastRay = rayOrigin.forward;
		}

		void DragEnd()
		{
			m_GrabbyEngaged = false;
		}
	}
}
