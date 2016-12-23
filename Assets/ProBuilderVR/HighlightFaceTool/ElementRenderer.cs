using UnityEngine;
using System.Collections;

namespace ProBuilder2.VR
{
	[ExecuteInEditMode]
	public abstract class ElementRenderer : MonoBehaviour
	{
		// HideFlags.DontSaveInEditor isn't exposed for whatever reason, so do the bit math on ints
		// and just cast to HideFlags.
		// HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.NotEditable
		HideFlags SceneCameraHideFlags = (HideFlags) (1 | 4 | 8);

		protected Mesh m_Mesh = null;
		[SerializeField] protected Material m_Material = null;
		private Transform m_Transform = null;

		void Start()
		{
			m_Transform = transform;
		}

		public abstract void SetMesh(pb_Object mesh);

		void OnRenderObject()
		{
			// instead of relying on 'SceneCamera' string comparison, check if the hideflags match.
			// this could probably even just check for one bit match, since chances are that any
			// game view camera isn't going to have hideflags set.
			if((Camera.current.gameObject.hideFlags & SceneCameraHideFlags) != SceneCameraHideFlags || Camera.current.name != "SceneCamera" )
				return;

			if(m_Mesh != null && m_Material != null)
			{
				m_Material.SetPass(0);
				Graphics.DrawMeshNow(m_Mesh, m_Transform.localToWorldMatrix);
			}
		}
	}
}
