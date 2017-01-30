using UnityEngine;
using UnityEngine.Experimental.EditorVR.Utilities;
using System.Collections;

namespace ProBuilder2.VR
{
	/**
 	 * Renders a line grid.
	 */
	public class GridModule : MonoBehaviour
	{
		[SerializeField] private Material m_GridMaterial;

		private Mesh m_Mesh;

		private int m_LineCount = 32;
		private bool m_HasUpGuide = false;	// what's upguide? ... not much, you?
		private float m_Snap = Snapping.DEFAULT_INCREMENT;
		private Color32 m_GridColor = new Color32(99, 99, 99, 128);
		private Color32 m_CenterColor = new Color32(0, 210, 255, 235);
		private bool m_isVisible = true;

		void Start()
		{
			m_Mesh = new Mesh();
			RebuildGridMesh();
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

		public void SetSnapIncrement(float newScale)
		{
			if(m_Snap != newScale)
			{
				m_Snap = newScale;
				RebuildGridMesh();
			}
		}		

		public void SetVisible(bool isVisible)
		{
			m_isVisible = isVisible;
		}

		/**
		 * Builds a grid object in 2d space
		 */
		void RebuildGridMesh()
		{
			float half = (m_LineCount/2f) * m_Snap;

			// to make grid lines equal and such
			int lineCount = m_LineCount + 1;

			// 2 vertices per line, 2 * lines per grid, + 2 for Y
			Vector3[] lines = new Vector3[lineCount * 4 + (m_HasUpGuide ? 2 : 0)];
			Vector2[] uv = new Vector2[lineCount * 4 + (m_HasUpGuide ? 2 : 0)];
			Color32[] color = new Color32[lineCount * 4 + (m_HasUpGuide ? 2 : 0)];
			int[] indices = new int[lineCount * 4 + (m_HasUpGuide ? 2 : 0)];

			int n = 0;
			for(int xx = 0; xx < lineCount; xx++)
			{
				// <--->
				indices[n] = n;
				uv[n] = new Vector2(xx / (float)(lineCount-1), 0f);
				color[n] = (xx == lineCount / 2) ? m_CenterColor : m_GridColor;
				lines[n++] = new Vector3( xx * m_Snap - half,  -half, 0f);

				indices[n] = n;
				uv[n] = new Vector2(xx / (float)(lineCount-1), 1f);
				color[n] = (xx == lineCount / 2) ? m_CenterColor : m_GridColor;
				lines[n++] = new Vector3( xx * m_Snap - half, half, 0f );

				// ^
				// |
				// v
				indices[n] = n;
				uv[n] = new Vector2(0f, xx / (float)(lineCount-1));
				color[n] = (xx == lineCount / 2) ? m_CenterColor : m_GridColor;
				lines[n++] = new Vector3( -half, xx * m_Snap - half, 0f );

				indices[n] = n;
				uv[n] = new Vector2(1f, xx / (float)(lineCount-1));
				color[n] = (xx == lineCount / 2) ? m_CenterColor : m_GridColor;
				lines[n++] = new Vector3(  half, xx * m_Snap - half, 0f );
			}

			if(m_HasUpGuide)
			{
				indices[n] = n;
				uv[n] = new Vector2(.5f, .5f);
				color[n] = m_CenterColor;
				lines[n] = new Vector3(0f, 0f, 100f);

				n++;

				indices[n] = n;
				uv[n] = new Vector2(.5f, .5f);
				color[n] = m_CenterColor;
				lines[n] = new Vector3(0f, 0f, -100f);
			}

			m_Mesh.Clear();
			m_Mesh.name = "GridMesh";
			m_Mesh.vertices = lines;
			m_Mesh.colors32 = color;
			m_Mesh.subMeshCount = 1;
			m_Mesh.SetIndices(indices, MeshTopology.Lines, 0);
			m_Mesh.uv = uv;
		}
	}
}
