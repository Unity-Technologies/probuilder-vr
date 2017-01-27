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
			m_Mesh = new Mesh();
			RebuildGridMesh(m_Mesh, lines, scale, gridColor, centerColor);
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

		public void SetScale(float newScale)
		{
			scale = newScale;
			RebuildGridMesh(m_Mesh, lines, scale, gridColor, centerColor);
		}		

		public void SetVisible(bool isVisible)
		{
			m_isVisible = isVisible;
		}

		/**
		 * Builds a grid object in 2d space
		 */
		static void RebuildGridMesh(Mesh m, int lineCount, float scale, Color32 gridColor, Color32 centerColor)
		{
			float half = (lineCount/2f) * scale;

			// to make grid lines equal and such
			lineCount++;

			// 2 vertices per line, 2 * lines per grid, + 2 for Y
			Vector3[] lines = new Vector3[lineCount * 4 + 2];
			Vector2[] uv = new Vector2[lineCount * 4 + 2];
			Color32[] color = new Color32[lineCount * 4 + 2];
			int[] indices = new int[lineCount * 4 + 2];

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

			indices[n] = n;
			uv[n] = new Vector2(.5f, .5f);
			color[n] = centerColor;
			lines[n] = new Vector3(0f, 0f, 100f);

			n++;

			indices[n] = n;
			uv[n] = new Vector2(.5f, .5f);
			color[n] = centerColor;
			lines[n] = new Vector3(0f, 0f, -100f);

			m.Clear();
			m.name = "GridMesh";
			m.vertices = lines;
			m.colors32 = color;
			m.subMeshCount = 1;
			m.SetIndices(indices, MeshTopology.Lines, 0);
			m.uv = uv;
		}
	}
}
