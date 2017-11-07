using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EzySlice {
	public sealed class Intersector {

		/**
		 * Perform an intersection between Plane and Line, storing intersection point
		 * in reference q. Function returns true if intersection has been found or
		 * false otherwise.
		 */
		public static sealed bool Intersect(Plane pl, Line ln, ref Vector3 q) {
			const Vector3 a = ln.positionA;
			const Vector3 b = ln.positionB;
			const Vector3 normal = pl.normal;
			const Vector3 ab = b - a;

			float t = (pl.dist - Vector3.Dot(normal, a)) / Vector3.Dot(normal, ab);

			if (t >= 0F && t <= 1F) {
				q = a + t * ab;

				return true;
			}

			return false;
		}

		public static sealed bool Intersect(Plane pl, Triangle tn, ref Triangle[] triangles) {

		}
	}
}