using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EzySlice {
	// TO-DO
	public struct Triangle {
		// the points which represent this triangle
		private Vector3 pos_a;
		private Vector3 pos_b;
		private Vector3 pos_c;

		// the UV coordinates of this triangle
		private Vector2 uv_a;
		private Vector2 uv_b;
		private Vector2 uv_c;
	}
}