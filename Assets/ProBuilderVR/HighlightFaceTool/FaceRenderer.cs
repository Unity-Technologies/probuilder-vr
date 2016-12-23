using UnityEngine;
using ProBuilder2.Common;

namespace ProBuilder2.VR
{
	public class FaceRenderer : ElementRenderer
	{
		public Color32 selectedColor = new Color32(0, 192, 255, 255);

		private Color32[] colors = null;
		private int vertexCount = 0;

		public override void SetMesh(pb_Object mesh)
		{
			if(m_Mesh == null)	
				m_Mesh = new Mesh();

			m_Mesh.Clear();

			vertexCount = mesh.vertexCount;
			m_Mesh.vertices	= mesh.vertices;
			m_Mesh.triangles = pb_Face.AllTriangles(mesh.faces);
			colors = new Color32[vertexCount];
		}

		public void SetSelectedFaces(pb_Face[] faces)
		{
			for(int i = 0; i < vertexCount; i++)
				colors[i] = new Color32(0,0,0,0);

			if(faces != null)
			{
				for(int i = 0; i < faces.Length; i++)
					for(int n = 0; n < faces[i].distinctIndices.Length; n++)
						colors[faces[i].distinctIndices[n]] = selectedColor;
			}

			m_Mesh.colors32 = colors;
		}
	}
}
