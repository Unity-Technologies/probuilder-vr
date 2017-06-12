using UnityEngine;
using UnityEditor.Experimental.EditorVR.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder2.VR
{
	/**
	 * Renders a line guide with tick marks at increments.
	 */
	public class HeightGuideModule : MonoBehaviour
	{
		[SerializeField] private Material m_GridMaterial;

		private Mesh m_Mesh;
		private float m_tickIncrement = Snapping.DEFAULT_INCREMENT * 4f;
		private int m_maxTicks = 64;
		private Color32 m_Color = new Color32(0, 210, 255, 235);
		private bool m_isVisible = true;

		void Start()
		{
			m_Mesh = new Mesh();
			RebuildGuideModule(m_Mesh, m_tickIncrement, m_maxTicks, m_Color);
			transform.position = Vector3.zero;
		}

		void OnDestroy()
		{
			ObjectUtils.Destroy(m_Mesh);
		}

		void LateUpdate()
		{
			if(m_isVisible)
				Graphics.DrawMesh(m_Mesh, transform.localToWorldMatrix, m_GridMaterial, gameObject.layer, null, 0);
		}

		public void SetIncrement(float tickIncrement)
		{
			m_tickIncrement = tickIncrement;
			RebuildGuideModule(m_Mesh, m_tickIncrement, m_maxTicks, m_Color);
		}       

		public void SetVisible(bool isVisible)
		{
			m_isVisible = isVisible;
		}

		/**
		 *	Mathf.Smoothstep is *not* GLSL smoothstep.
		 */
		static float smoothstep(float x, float y, float a)
		{
			float t = Mathf.Clamp((a - x) / (y - x), 0.0f, 1.0f);
			return t * t * (3.0f - 2.0f * t);
		}

		/**
		 * Builds a grid object in 2d space
		 */
		static void RebuildGuideModule(Mesh m, float tickIncrement, int numberOfTicks, Color32 color)
		{
			m.Clear();
			m.name = "Guide Mesh";

			List<Vector3> positions = new List<Vector3>();
			List<Color32> colors = new List<Color32>();
			List<int> indices = new List<int>();
			int index = 0;
			float offset = numberOfTicks * tickIncrement;
			Color32 tickColor = color;
			tickColor.a = 0;
			const float FADE_DIST = .5f;
			byte alpha = color.a;

			for(int i = 0; i < numberOfTicks * 2; i++)
			{
				float p = (i * tickIncrement) - offset;
				float ss = smoothstep(0f, 1f - FADE_DIST, 1f - (Mathf.Abs((p/offset))));

				positions.Add( new Vector3(  0f * ss, 0f, p) );
				positions.Add( new Vector3(-.05f * ss, 0f, p) );
				positions.Add( new Vector3( .05f * ss, 0f, p) );

				color.a = (byte) (ss * alpha);
				colors.Add(color);
				colors.Add(tickColor);
				colors.Add(tickColor);
				
				indices.Add(index+0);
				indices.Add(index+1);
				indices.Add(index+0);
				indices.Add(index+2);

				index += 3;
			}

			positions.Add( new Vector3(0f, 0f, - offset * FADE_DIST) );
			positions.Add( new Vector3(0f, 0f, 	 offset * FADE_DIST) );
			color.a = alpha;
			colors.Add(color);
			colors.Add(color);
			indices.Add(index++);
			indices.Add(index++);

			positions.Add( new Vector3(0f, 0f, -offset) );
			positions.Add( new Vector3(0f, 0f, 	offset) );
			color.a = 0;
			colors.Add(color);
			colors.Add(color);
			indices.Add(index - 2);
			indices.Add(index    );
			indices.Add(index - 1);
			indices.Add(index + 1);


			m.vertices = positions.ToArray();
			m.colors32 = colors.ToArray();
			m.uv = Enumerable.Repeat(new Vector2(.5f, .5f), positions.Count).ToArray();
			m.subMeshCount = 1;
			m.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
		}
	}
}
