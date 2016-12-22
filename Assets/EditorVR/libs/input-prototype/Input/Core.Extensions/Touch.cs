using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.InputNew
{
	public struct Touch
	{
		public int fingerId;
		public TouchPhase phase;
		public Vector2 position;
		public Vector2 rawPosition;
		public Vector2 delta;
		public float diameter;
		public float deltaTime;
		public float time;

		public bool isValid
		{
			get { return fingerId != 0; }
		}
	}
}
