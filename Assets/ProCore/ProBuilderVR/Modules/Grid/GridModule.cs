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

		private int lines = 32;
		private float scale = Snapping.DEFAULT_INCREMENT;
		private Color32 gridColor = new Color32(99, 99, 99, 128);
		private Color32 centerColor = new Color32(0, 210, 255, 235);
		private bool m_isVisible = true;

		void Start()
		{
			m_Mesh = GridMesh(lines, scale);
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

		public void SetVisible(bool isVisible)
		{
			m_isVisible = isVisible;
		}

		/**
		 * Builds a grid object in 2d space
		 */
		Mesh GridMesh(int lineCount, float scale)
		{
			float half = (lineCount/2f) * scale;

			// to make grid lines equal and such
			lineCount++;

			Vector3[] lines = new Vector3[lineCount * 4];	// 2 vertices per line, 2 * lines per grid
			Vector2[] uv = new Vector2[lineCount * 4];
			Color32[] color = new Color32[lineCount * 4];
			int[] indices = new int[lineCount * 4];

			int n = 0;
			for(int xx = 0; xx < lineCount; xx++)
			{
				// <--->
				indices[n] = n;
				uv[n] = new Vector2(xx / (float)(lineCount-1), 0f);
				color[n] = (xx == lineCount / 2) ? centerColor : gridColor;
				lines[n++] = new Vector3( xx * scale - half,  -half, 0f);

				indices[n] = n;
				uv[n] = new Vector2(xx / (float)(lineCount-1), 1f);
				color[n] = (xx == lineCount / 2) ? centerColor : gridColor;
				lines[n++] = new Vector3( xx * scale - half, half, 0f );

				// ^
				// |
				// v
				indices[n] = n;
				uv[n] = new Vector2(0f, xx / (float)(lineCount-1));
				color[n] = (xx == lineCount / 2) ? centerColor : gridColor;
				lines[n++] = new Vector3( -half, xx * scale - half, 0f );

				indices[n] = n;
				uv[n] = new Vector2(1f, xx / (float)(lineCount-1));
				color[n] = (xx == lineCount / 2) ? centerColor : gridColor;
				lines[n++] = new Vector3(  half, xx * scale - half, 0f );
			}

			Mesh tm = new Mesh();

			tm.name = "GridMesh";
			tm.vertices = lines;
			tm.colors32 = color;
			tm.subMeshCount = 1;
			tm.SetIndices(indices, MeshTopology.Lines, 0);
			tm.uv = uv;

			return tm;
		}
	}
}
