using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder2.VR
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class LineSegmentRenderer : MonoBehaviour
	{
		public Vector3[] m_Positions;
		private Mesh m_Mesh = null;

		void Start()
		{
			if(m_Positions != null)
				RebuildMesh();
		}

		public void SetPositions(Vector3[] positions)
		{
			m_Positions = positions;
			RebuildMesh();
		}

		private void RebuildMesh()
		{
			if(m_Positions.Length < 2 || m_Positions.Length % 2 > 0)
			{
				Debug.LogWarning("Line segment positions must be > 1 and even.");
				return;
			}

			if(m_Mesh == null)
				m_Mesh = new Mesh();

			m_Mesh.Clear();

			int count = m_Positions.Length;

			Vector3[] positions = new Vector3[count * 2];
			Vector3[] link = new Vector3[count * 2];
			List<Vector4> uv0 = new List<Vector4>(count * 2);
			int[] triangles = new int[count * 2 * 6];

			for(int i = 0, n = 0, t = 0; i < count; i += 2, n += 4, t += 6)
			{
				positions[n+0] = m_Positions[i  ]; // - Vector3.forward;
				positions[n+1] = m_Positions[i  ]; // + Vector3.forward;
				positions[n+2] = m_Positions[i+1]; // - Vector3.forward;
				positions[n+3] = m_Positions[i+1]; // + Vector3.forward;

				link[n+0] = m_Positions[i+1];
				link[n+1] = m_Positions[i+1];
				link[n+2] = m_Positions[i  ];
				link[n+3] = m_Positions[i  ];

				uv0.Add( new Vector4(0f, 0f, -1f, 0f) );
				uv0.Add( new Vector4(1f, 0f,  1f, 1f) );
				uv0.Add( new Vector4(0f, 1f,  1f, 2f) );
				uv0.Add( new Vector4(1f, 1f, -1f, 3f) );

				triangles[t+0] = n + 0;
				triangles[t+1] = n + 1;
				triangles[t+2] = n + 2;
				triangles[t+3] = n + 1;
				triangles[t+4] = n + 3;
				triangles[t+5] = n + 2;
			}

			m_Mesh.vertices = positions;
			m_Mesh.normals = link;
			m_Mesh.SetUVs(0, uv0);
			m_Mesh.triangles = triangles;

			GetComponent<MeshFilter>().sharedMesh = m_Mesh;
		}
	}
}
