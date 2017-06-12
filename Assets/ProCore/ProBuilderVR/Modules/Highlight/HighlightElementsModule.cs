using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using UnityEditor.Experimental.EditorVR.Utilities;

namespace ProBuilder2.VR
{
	/**
	 * Set face and edge highlights on selected objects.
	 */
	public class HighlightElementsModule : MonoBehaviour
	{
		[SerializeField]
		private Material m_HighlightMaterial;
		private readonly Dictionary<pb_Object, Mesh> m_Highlights = new Dictionary<pb_Object, Mesh>();
		private Color32 m_CurrentColor = new Color32(0,0,0,0);

		void LateUpdate()
		{
			foreach (var kvp in m_Highlights)
			{
				if (kvp.Key == null)
					continue;

				Graphics.DrawMesh(kvp.Value, kvp.Key.gameObject.transform.localToWorldMatrix, m_HighlightMaterial, kvp.Key.gameObject.layer, null, 0);
			}
		}

		void OnDestroy()
		{
			Clear();
		}

		public Color32 color
		{
			set
			{
				if(!m_CurrentColor.Equals(value))
				{
					m_CurrentColor = value;
					m_HighlightMaterial.color = value;
				}
			}
		}

		public void Clear()
		{
			List<Mesh> m = m_Highlights.Values.ToList();

			for(int i = m.Count - 1; i > -1; i--)
				ObjectUtils.Destroy(m[i]);

			m_Highlights.Clear();
		}

		/**
		 * Highlight a set of faces on a pb_Object.  If exclusive is true also clear any other pb_Objects that had been highlighted.
		 */
		public void SetFaceHighlight(pb_Object pb, pb_Face[] faces, bool exclusive = false)
		{
			if(pb == null)
				return;

			Mesh m = null;

			if(!m_Highlights.TryGetValue(pb, out m))
			{
				if(exclusive)
					Clear();

				if(faces != null)
				{
					if(m == null)
						m = new Mesh();

					m.Clear();
					m.vertices	= pb.vertices;
					m_Highlights.Add(pb, m);
				}
				else
				{
					return;
				}
			}
			else 
			{
				if(faces == null)
				{
					if(exclusive)
					{
						Clear();
					}
					else
					{	
						Mesh t = m;
						m_Highlights.Remove(pb);
						ObjectUtils.Destroy(t);
					}
					return;
				}
				else
				{
					if(exclusive)
					{
						IEnumerable<pb_Object> rm = m_Highlights.Keys.Where(x => x != pb);

						foreach(pb_Object p in rm)
						{
							Mesh t = m_Highlights[p];
							m_Highlights.Remove(p);
							Object.Destroy(t);
						}
					}
				}
			}

			m.triangles = pb_Face.AllTriangles(faces);

			Vector3[] normals = new Vector3[pb.vertexCount];

			for(int i = 0; i < faces.Length; i++)
			{
				pb_Face face = faces[i];

				Vector3 normal = pb_Math.Normal(pb, face);

				for(int n = 0; n < face.distinctIndices.Length; n++)
					normals[face.distinctIndices[n]] = normal;
			}

			m.normals = normals;
		}

		/**
		 * Update the vertex positions for a pb_Object in the selection.
		 */
		public void UpdateVertices(pb_Object pb)
		{
			Mesh m = null;

			if(!m_Highlights.TryGetValue(pb, out m))
				return;
				
			m.vertices = pb.vertices;
		}
	}
}
