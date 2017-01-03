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

		private Transform m_GrabTarget = null;
		private Vector3 m_PositionOffset;
		private Quaternion m_RotationOffset;
		private bool m_GrabbyEngaged = false;

		private Vector3 m_OriginPosition;
		private Quaternion m_OriginRotation;

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

		void Translate(Vector3 delta) {
			m_GrabTarget.transform.position += delta;
		}

		void Rotate(Quaternion delta) {
			m_GrabTarget.transform.rotation *= delta;
		}

		void DragStart(Transform target)
		{
			m_OriginPosition = target.position;
			m_OriginRotation = target.rotation;

			m_GrabTarget = target;
			var inverseRotation = Quaternion.Inverse(rayOrigin.rotation);
			m_PositionOffset = inverseRotation * (m_GrabTarget.transform.position - rayOrigin.position);
			m_RotationOffset = inverseRotation * m_GrabTarget.transform.rotation;
		}

		void Dragging()
		{
			Translate(rayOrigin.position + rayOrigin.rotation * m_PositionOffset - m_GrabTarget.position);
			Rotate(Quaternion.Inverse(m_GrabTarget.rotation) * rayOrigin.rotation * m_RotationOffset);
		}

		void DragEnd()
		{
			m_GrabbyEngaged = false;

			Vector3 p = m_OriginPosition - m_GrabTarget.position;
			Quaternion r = Quaternion.Inverse(m_OriginRotation) * m_GrabTarget.rotation;

			m_GrabTarget.position = m_OriginPosition;
			m_GrabTarget.rotation = m_OriginRotation;

			viewerPivot.rotation = viewerPivot.rotation * r;
			viewerPivot.position += p;
		}
	}
}
