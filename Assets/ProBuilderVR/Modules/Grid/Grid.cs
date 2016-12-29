using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder2.VR
{
	[RequireComponent(typeof(LineSegmentRenderer))]
	public class Grid : MonoBehaviour
	{

		void Start()
		{
			List<Vector3> positions = new List<Vector3>();

			int count = 11;
			float size = 10f;
			float half = size * .5f;

			for(int i = 0; i < count; i++)
			{
				float nrm = i / (count - 1f);

				positions.Add(new Vector3(-half, 0f, -half + nrm * size));
				positions.Add(new Vector3( half, 0f, -half + nrm * size));

				positions.Add(new Vector3(-half + nrm * size, 0f, -half));
				positions.Add(new Vector3(-half + nrm * size, 0f,  half));
			}

			GetComponent<LineSegmentRenderer>().SetPositions(positions.ToArray());
		}
	}
}
