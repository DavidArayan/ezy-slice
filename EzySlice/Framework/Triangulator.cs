using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EzySlice {

	/**
	 * Contains static functionality for performing Triangulation on arbitrary vertices.
	 * Read the individual function descriptions for specific details.
	 */
	public sealed class Triangulator {

		/**
		 * Represents a 3D Vertex which has been mapped onto a 2D surface
		 * and is mainly used in MonotoneChain to triangulate a set of vertices
		 * against a flat plane.
		 */
		internal struct Mapped2D {
			private readonly Vector3 original;
			private readonly Vector2 mapped;

			public Mapped2D(Vector3 newOriginal, Vector3 u, Vector3 v) {
				this.original = newOriginal;
				this.mapped = new Vector2(Vector3.Dot(newOriginal, u), Vector3.Dot(newOriginal, v));
			}

			public Vector2 mappedValue {
				get { return this.mapped; }
			}

			public Vector3 originalValue {
				get { return this.original; }
			}
		}

		/**
		 * Overloaded variant of MonotoneChain which will calculate UV coordinates of the Triangles
		 * between 0.0 and 1.0 (default).
		 * 
		 * See MonotoneChain(vertices, normal, tri, TextureRegion) for full explanation
		 */
		public static bool MonotoneChain(List<Vector3> vertices, Vector3 normal, out List<Triangle> tri) {
			// default texture region is in coordinates 0,0 to 1,1
			return MonotoneChain(vertices, normal, out tri, new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f));
		}

		/**
		 * O(n log n) Convex Hull Algorithm. 
		 * Accepts a list of vertices as Vector3 and triangulates them according to a projection
		 * plane defined as planeNormal. Algorithm will output vertices, indices and UV coordinates
		 * as arrays
		 */
		public static bool MonotoneChain(List<Vector3> vertices, Vector3 normal, out List<Triangle> tri, TextureRegion texRegion) {
			int count = vertices.Count;

			// we cannot triangulate less than 3 points. Use minimum of 3 points
			if (count < 3) {
				tri = null;
				return false;
			}

			// first, we map from 3D points into a 2D plane represented by the provided normal
			Vector3 u = Vector3.Normalize(Vector3.Cross(normal, Vector3.up));
			if ( Vector3.zero == u)
			{
				u = Vector3.Normalize(Vector3.Cross(normal, Vector3.forward));
			}
			Vector3 v = Vector3.Cross(u, normal);

			// generate an array of mapped values
			Mapped2D[] mapped = new Mapped2D[count];

			// these values will be used to generate new UV coordinates later on
			float maxDivX = float.MinValue;
			float maxDivY = float.MinValue;
			float minDivX = float.MaxValue;
			float minDivY = float.MaxValue;

			// map the 3D vertices into the 2D mapped values
			for (int i = 0; i < count; i++) {
				Vector3 vertToAdd = vertices[i];

				Mapped2D newMappedValue = new Mapped2D(vertToAdd, u, v);
				Vector2 mapVal = newMappedValue.mappedValue;

				// grab our maximal values so we can map UV's in a proper range
				maxDivX = Mathf.Max(maxDivX, mapVal.x);
				maxDivY = Mathf.Max(maxDivY, mapVal.y);
				minDivX = Mathf.Min(minDivX, mapVal.x);
				minDivY = Mathf.Min(minDivY, mapVal.y);

				mapped[i] = newMappedValue;
			}

			// sort our newly generated array values
			Array.Sort<Mapped2D>(mapped, (a, b) =>
			{
				Vector2 x = a.mappedValue;
				Vector2 p = b.mappedValue;

				return (x.x < p.x || (x.x == p.x && x.y < p.y)) ? -1 : 1;
			});

			// our final hull mappings will end up in here
			Mapped2D[] hulls = new Mapped2D[count+1];

			int k = 0;

			// build the lower hull of the chain
			for (int i = 0; i < count; i++) {
				while (k >= 2) {
					Vector2 mA = hulls[k - 2].mappedValue;
					Vector2 mB = hulls[k - 1].mappedValue;
					Vector2 mC = mapped[i].mappedValue;

					if (Intersector.TriArea2D(mA.x, mA.y, mB.x, mB.y, mC.x, mC.y) > 0.0f) {
						break;
					}

					k--;
				}

				hulls[k++] = mapped[i];
			}

			// build the upper hull of the chain
			for (int i = count - 2, t = k + 1; i >= 0; i--) {
				while (k >= t) {
					Vector2 mA = hulls[k - 2].mappedValue;
					Vector2 mB = hulls[k - 1].mappedValue;
					Vector2 mC = mapped[i].mappedValue;

					if (Intersector.TriArea2D(mA.x, mA.y, mB.x, mB.y, mC.x, mC.y) > 0.0f) {
						break;
					}

					k--;
				}

				hulls[k++] = mapped[i];
			}

			// finally we can build our mesh, generate all the variables
			// and fill them up
			int vertCount = k - 1;
			int triCount = (vertCount - 2) * 3;

			// this should not happen, but here just in case
			if (vertCount < 3) {
				tri = null;
				return false;
			}

			// ensure List does not dynamically grow, performing copy ops each time!
			tri = new List<Triangle>(triCount / 3);

			float width = maxDivX - minDivX;
			float height = maxDivY - minDivY;

			int indexCount = 1;

			// generate both the vertices and uv's in this loop
			for (int i = 0; i < triCount; i+=3) {
				// the Vertices in our triangle
				Mapped2D posA = hulls[0];
				Mapped2D posB = hulls[indexCount];
				Mapped2D posC = hulls[indexCount + 1];

				// generate UV Maps
				Vector2 uvA = posA.mappedValue;
				Vector2 uvB = posB.mappedValue;
				Vector2 uvC = posC.mappedValue;

				uvA.x = (uvA.x - minDivX) / width;
				uvA.y = (uvA.y - minDivY) / height;

				uvB.x = (uvB.x - minDivX) / width;
				uvB.y = (uvB.y - minDivY) / height;

				uvC.x = (uvC.x - minDivX) / width;
				uvC.y = (uvC.y - minDivY) / height;

				Triangle newTriangle = new Triangle(posA.originalValue, posB.originalValue, posC.originalValue);

				// ensure our UV coordinates are mapped into the requested TextureRegion
				newTriangle.SetUV(texRegion.Map(uvA), texRegion.Map(uvB), texRegion.Map(uvC));

				// the normals is the same for all vertices since the final mesh is completly flat
				newTriangle.SetNormal(normal, normal, normal);
				newTriangle.ComputeTangents();

				tri.Add(newTriangle);

				indexCount++;
			}

			return true;
		}
	}
}