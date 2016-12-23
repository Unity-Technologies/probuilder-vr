using UnityEngine;
using ProBuilder2.Common;

namespace ProBuilder2.VR
{
	public class FaceRenderer : ElementRenderer
	{
		public Color32 selectedColor = new Color32(0, 192, 255, 255);

		private Color32[] colors = null;
		private Vector3[] normals = null;

		pb_Object pb = null;
		private int vertexCount = 0;

		public override void SetMesh(pb_Object mesh)
		{
			pb = mesh;

			if(m_Mesh == null)
				m_Mesh = new Mesh();

			m_Mesh.Clear();

			vertexCount = mesh.vertexCount;
			m_Mesh.vertices	= mesh.vertices;
			normals = new Vector3[vertexCount];
			colors = new Color32[vertexCount];
		}

		public void SetSelectedFaces(pb_Face[] faces)
		{
			m_Mesh.triangles = pb_Face.AllTriangles(faces);

			for(int i = 0; i < vertexCount; i++)
				colors[i] = new Color32(0,0,0,0);

			if(faces != null)
			{
				for(int i = 0; i < faces.Length; i++)
				{
					pb_Face face = faces[i];

					Vector3 normal = pb_Math.Normal(pb, face);

					for(int n = 0; n < face.distinctIndices.Length; n++)
					{
						normals[face.distinctIndices[n]] = normal;
						colors[face.distinctIndices[n]] = selectedColor;
					}
				}
			}

			m_Mesh.normals = normals;
			m_Mesh.colors32 = colors;
		}
	}
}
