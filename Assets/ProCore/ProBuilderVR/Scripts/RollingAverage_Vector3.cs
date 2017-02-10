using UnityEngine;
using System.Collections;

namespace ProBuilder2.VR
{
	/**
	 *	Smooth out a vector3 over SIZE frames.
	 */
	internal struct RollingAverage_Vector3
	{
		const int SIZE = 16;
		private Vector3 mValue;
		private float mCount;
		private int mIndex;
		private Vector3[] mRingBuffer;
		private Vector3 mSum;

		public RollingAverage_Vector3(Vector3 v)
		{
			mValue = v;
			mCount = 1f;
			mIndex = 1;
			mSum = mValue;
 			mRingBuffer = new Vector3[SIZE];
 			mRingBuffer[0] = mValue;
		}

		public static implicit operator Vector3(RollingAverage_Vector3 v)
		{
			return v.mValue;
		}

		public static implicit operator RollingAverage_Vector3(Vector3 v)
		{
			return new RollingAverage_Vector3(v);
		}

		public Vector3 Add(Vector3 n)
		{
			mSum.x -= mRingBuffer[mIndex].x;
			mSum.y -= mRingBuffer[mIndex].y;
			mSum.z -= mRingBuffer[mIndex].z;

			mRingBuffer[mIndex] = n;

			mSum.x += n.x;
			mSum.y += n.y;
			mSum.z += n.z;

			mCount = Mathf.Min(SIZE, (++mCount));
			
			mValue = mSum / mCount;

			if(++mIndex > SIZE-1)
				mIndex = 0;

			return mValue;
		}

		public void Reset(Vector3 value = default(Vector3))
		{
			mValue = value;
			mCount = 1f;
			mIndex = 1;
			mSum = mValue;
 			mRingBuffer = new Vector3[SIZE];
 			mRingBuffer[0] = mValue;
		}
	}
}
