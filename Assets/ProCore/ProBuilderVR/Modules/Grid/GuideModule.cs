using UnityEngine;
using UnityEngine.Experimental.EditorVR.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder2.VR
{
	/**
	 * Renders a 3D line guide.
	 */
	public class GuideModule : MonoBehaviour
	{
		[SerializeField] private Material m_GridMaterial;

		private Mesh m_Mesh;
		private Vector3 m_Size = new Vector3(100f, 100f, 100f);
		private Color32 m_Color = new Color32(0, 210, 255, 235);
		private bool m_isVisible = true;

		void Start()
		{
			m_Mesh = new Mesh();
			RebuildGuideModule(m_Mesh, m_Size, m_Color);
			transform.position = Vector3.zero;
		}

		void OnDestroy()
		{
			U.Object.Destroy(m_Mesh);
		}

		void LateUpdate()
		{
			if(m_isVisible)
				Graphics.DrawMesh(m_Mesh, transform.localToWorldMatrix, m_GridMaterial, gameObject.layer, null, 0);
		}

		public void SetScale(Vector3 newSize)
		{
			m_Size = newSize;
			RebuildGuideModule(m_Mesh, m_Size, m_Color);
		}       

		public void SetVisible(bool isVisible)
		{
			m_isVisible = isVisible;
		}

		/**
		 * Builds a grid object in 2d space
		 */
		static void RebuildGuideModule(Mesh m, Vector3 size, Color32 color)
		{
			float 	x = size.x * .5f,
					y = size.y * .5f,
					z = size.z * .5f;

			m.Clear();
			m.name = "Guide Mesh";
			m.vertices = new Vector3[]
			{
				new Vector3(-1f * x,       0f,        0f),
				new Vector3( 1f * x,       0f,        0f),
				new Vector3(      0f, -1f * y,        0f),
				new Vector3(      0f,  1f * y,        0f),
				new Vector3(      0f,       0f,  -1f * z),
				new Vector3(      0f,       0f,   1f * z)
			};
			m.colors32 = Enumerable.Repeat(color, 6).ToArray();
			m.uv = Enumerable.Repeat(new Vector2(.5f, .5f), 6).ToArray();
			m.subMeshCount = 1;
			m.SetIndices(new int[] { 0, 1, 2, 3, 4, 5 }, MeshTopology.Lines, 0);
		}
	}
}
