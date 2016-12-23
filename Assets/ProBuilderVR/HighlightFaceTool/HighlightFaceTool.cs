using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
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
	public class HighlightFaceTool : MonoBehaviour, ITool, IStandardActionMap, IUsesRayOrigin, IUsesRaycastResults
	{
		public Transform rayOrigin { get; set; }
	   	public Func<Transform, GameObject> getFirstGameObject { get; set; }

	   	[SerializeField] GameObject pointer;
	   	private GameObject m_Pointer;

		void Start()
		{
			Selection.selectionChanged += OnSelectionChanged;
			m_Pointer = U.Object.Instantiate(pointer);
		}

		void OnSelectionChanged()
		{
		}

		public void ProcessInput(ActionMapInput input, Action<InputControl> consumeControl)
		{
			Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
			pb_RaycastHit hit;

			foreach(pb_Object pb in Selection.transforms.GetComponents<pb_Object>())
			{
				if( pb_HandleUtility.FaceRaycast(ray, pb, out hit) )
				{
					Vector3 p = pb.transform.TransformPoint(hit.point);
					Vector3 n = pb.transform.TransformDirection(hit.normal);

					m_Pointer.transform.position = p;
					m_Pointer.transform.localRotation = Quaternion.FromToRotation(Vector3.up, n);
				}
			}
		}

		void OnDestroy()
		{
			U.Object.Destroy(m_Pointer);
		}
	}
}
