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
		public static bool Intersect(Plane pl, Line ln, out Vector3 q) {
			return Intersector.Intersect(pl, ln.positionA, ln.positionB, out q);
		}

		/**
		 * Perform an intersection between Plane and Line made up of points a and b. Intersection
		 * point will be stored in reference q. Function returns true if intersection has been
		 * found or false otherwise.
		 */
		public static bool Intersect(Plane pl, Vector3 a, Vector3 b, out Vector3 q) {
			Vector3 normal = pl.normal;
			Vector3 ab = b - a;

			float t = (pl.dist - Vector3.Dot(normal, a)) / Vector3.Dot(normal, ab);

			if (t >= 0F && t <= 1F) {
				q = a + t * ab;

				return true;
			}

			q = Vector3.zero;

			return false;
		}

		/**
		 * Perform an intersection between Plane and Triangle, storing intersection points in array
		 * position outPt and Separate Triangles in outTri.
		 * A Plane is only able to split a triangle into 2 parts, generating 3 triangles and 
		 * 2 intersection points.
		 * Results will be filled into the IntersectionResult reference. Check result.isValid()
		 * for the final results.
		 */
		public static void Intersect(Plane pl, Triangle tri, ref IntersectionResult result) {
			// -> Quick Exit via performing a Plane->Sphere test
		}
	}
}