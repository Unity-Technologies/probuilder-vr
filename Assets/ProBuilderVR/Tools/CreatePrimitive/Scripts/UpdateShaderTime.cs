#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ProBuilder2.VR
{
	[ExecuteInEditMode]
	public class UpdateShaderTime : MonoBehaviour
	{
		Material m_Material = null;

		void Start()
		{
			m_Material = GetComponent<MeshRenderer>().sharedMaterial;
		}

		void Update ()
		{
			if(m_Material != null)
			{
				float t = (float) EditorApplication.timeSinceStartup;
				m_Material.SetFloat("_RotateTime", t);
			}
		}
	}
}
#endif
