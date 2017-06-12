using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Experimental.EditorVR.Utilities;

namespace ProBuilder2.VR
{
	/**
	 * Render an outline of the bounds of a selected object.
	 */
	public class SelectionBoundsModule : MonoBehaviour
	{
		[SerializeField]
		private Material m_HighlightMaterial;
		private readonly Dictionary<pb_Object, Mesh> m_Highlights = new Dictionary<pb_Object, Mesh>();
		
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
			List<Mesh> m = m_Highlights.Values.ToList();
			for(int i = m.Count - 1; i > -1; i--)
				ObjectUtils.Destroy(m[i]);
			m_Highlights.Clear();
		}

		public void SetHighlight(pb_Object pb, bool isHighlighted)
		{
			Mesh m = null;

			if(m_Highlights.TryGetValue(pb, out m))
			{
				if(!isHighlighted)
				{
					ObjectUtils.Destroy(m);
					m_Highlights.Remove(pb);
				}
				else
				{
					GenerateBounds(m, pb.msh.bounds);
				}
			}
			else
			{
				if(isHighlighted)
				{
					m = new Mesh();
					GenerateBounds(m, pb.msh.bounds);
					m_Highlights.Add(pb, m);
				}
			}
		}

		/**
		 * Generate a line segment bounds representation.
		 */
		void GenerateBounds(Mesh m, Bounds bounds)
		{
			Vector3 cen = bounds.center;
			Vector3 ext = bounds.extents; // + bounds.extents.normalized * .1f;

			// Draw Wireframe
			List<Vector3> v = new List<Vector3>();
			int[] t = new int[48];

			v.AddRange( DrawBoundsEdge(cen, -ext.x, -ext.y, -ext.z, .2f) );
			v.AddRange( DrawBoundsEdge(cen, -ext.x, -ext.y,  ext.z, .2f) );
			v.AddRange( DrawBoundsEdge(cen,  ext.x, -ext.y, -ext.z, .2f) );
			v.AddRange( DrawBoundsEdge(cen,  ext.x, -ext.y,  ext.z, .2f) );

			v.AddRange( DrawBoundsEdge(cen, -ext.x,  ext.y, -ext.z, .2f) );
			v.AddRange( DrawBoundsEdge(cen, -ext.x,  ext.y,  ext.z, .2f) );
			v.AddRange( DrawBoundsEdge(cen,  ext.x,  ext.y, -ext.z, .2f) );
			v.AddRange( DrawBoundsEdge(cen,  ext.x,  ext.y,  ext.z, .2f) );

			for(int i = 0; i < 48; i++)
				t[i] = i;

			m.Clear();
			m.vertices = v.ToArray();
			m.subMeshCount = 1;
			m.SetIndices(t, MeshTopology.Lines, 0);
		}

		Vector3[] DrawBoundsEdge(Vector3 center, float x, float y, float z, float size)
		{
			Vector3 p = center;
			Vector3[] v = new Vector3[6];

			p.x += x;
			p.y += y;
			p.z += z;

			v[0] = p;
			v[1] = (p + ( -(x/Mathf.Abs(x)) * Vector3.right 	* Mathf.Min(size, Mathf.Abs(x))));

			v[2] = p;
			v[3] = (p + ( -(y/Mathf.Abs(y)) * Vector3.up 		* Mathf.Min(size, Mathf.Abs(y))));

			v[4] = p;
			v[5] = (p + ( -(z/Mathf.Abs(z)) * Vector3.forward 	* Mathf.Min(size, Mathf.Abs(z))));

			return v;
		}
	}
}
