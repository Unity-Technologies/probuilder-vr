using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputNew;
using UnityEditor.Experimental.EditorVR;
using UnityEditor.Experimental.EditorVR.Menus;
using UnityEditor.Experimental.EditorVR.Helpers;
using UnityEditor.Experimental.EditorVR.Tools;
using UnityEditor.Experimental.EditorVR.Utilities;
using UnityEditor.Experimental.EditorVR.Manipulators;

namespace ProBuilder2.VR
{
	[MainMenuItem("Grabby Locomotor", "ProBuilder", "Maneuver around the scene.")]
	public class GrabbyLocomotionTool : MonoBehaviour,
										ITool,
										ICustomActionMap,
										IUsesRayOrigin,
										IUsesCameraRig,
										IUsesRaycastResults,
										IExclusiveMode
	{
		public Transform rayOrigin { get; set; }
		public ActionMap actionMap { get { return m_GrabbyActionMap; } }

		// public DirectManipulator directManipulator { get { return m_DirectManipulator; } }
		// [SerializeField]
		// private DirectManipulator m_DirectManipulator;

		[SerializeField]
		private ActionMap m_GrabbyActionMap;
		public Transform cameraRig { get; set; }
		private Vector3 m_GrabTarget = Vector3.zero;
		private bool m_GrabbyEngaged = false;
		private Vector3 m_LastRay = Vector3.up;

		public void ProcessInput(ActionMapInput input, ConsumeControlDelegate consumeControl)
		{
			GrabbyLocomotion grabInput = (GrabbyLocomotion) input;

			if(grabInput.trigger2.wasJustPressed)
			{
				// engage grabby
				GameObject first = this.GetFirstGameObject(rayOrigin);

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
			m_LastRay = rayOrigin.forward;
		}

		void Dragging()
		{
			float angle = Vector3.Angle(m_LastRay, rayOrigin.forward);

			Vector3 cross = Vector3.Cross(m_LastRay, Vector3.up);

			if(Vector3.Dot(rayOrigin.forward, cross) > 0f)
				angle = -angle;

			cameraRig.RotateAround(m_GrabTarget, Vector3.up, angle);

			m_LastRay = rayOrigin.forward;
		}

		void DragEnd()
		{
			m_GrabbyEngaged = false;
		}
	}
}
