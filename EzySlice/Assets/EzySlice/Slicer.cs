using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EzySlice {
	public sealed class Slicer {

		/**
		 * Slice the gameobject mesh (if any) using the Plane, which will generate
		 * a maximum of 2 other Meshes.
		 * This function will recalculate new UV coordinates to ensure textures are applied
		 * properly.
		 * Returns null if no intersection has been found or the GameObject does not contain
		 * a valid mesh to cut.
		 */
		public static Mesh[] Slice(GameObject obj, Plane pl) {
			MeshFilter renderer = obj.GetComponent<MeshFilter>();

			if (renderer == null) {
				return null;
			}

			Mesh sharedMesh = renderer.sharedMesh;

			if (sharedMesh == null) {
				return null;
			}

			Vector3[] ve = sharedMesh.vertices;
			Vector2[] uv = sharedMesh.uv;
			int[] indices = sharedMesh.triangles;

			int indicesCount = indices.Length;

			// we reuse this object for all intersection tests
			IntersectionResult result = new IntersectionResult();

			// all our buffers, as Triangles
			List<Triangle> upperHull = new List<Triangle>();
			List<Triangle> lowerHull = new List<Triangle>();
			List<Vector3> crossHull = new List<Vector3>();

			// loop through all the mesh vertices, generating upper and lower hulls
			// and all intersection points
			for (int index = 0; index < indicesCount; index += 3) {
				int i0 = indices[index + 0];
				int i1 = indices[index + 1];
				int i2 = indices[index + 2];

				Triangle newTri = new Triangle(ve[i0], ve[i1], ve[i2], uv[i0], uv[i1], uv[i2]);

				// slice this particular triangle with the provided
				// plane
				if (newTri.Split(pl, result)) {
					int upperHullCount = result.upperHullCount;
					int lowerHullCount = result.lowerHullCount;
					int interHullCount = result.intersectionPointCount;

					for (int i = 0; i < upperHullCount; i++) {
						upperHull.Add(result.lowerHull[i]);
					}

					for (int i = 0; i < lowerHullCount; i++) {
						lowerHull.Add(result.lowerHull[i]);
					}

					for (int i = 0; i < interHullCount; i++) {
						crossHull.Add(result.intersectionPoints[i]);
					}
				}
				else {
					SideOfPlane side = pl.SideOf(ve[i0]);

					if (side == SideOfPlane.UP || side == SideOfPlane.ON) {
						upperHull.Add(newTri);
					}
					else {
						lowerHull.Add(newTri);
					}
				}
			}

			// start creating our hulls
			Mesh finalUpperHull = CreateFrom(upperHull);
			Mesh finalLowerHull = CreateFrom(lowerHull);

			// TMP -> TO/DO -> Function not complete
			return null;
		}

		private static Mesh CreateFrom(List<Triangle> hull) {
			int count = hull.Count;

			if (count <= 0) {
				return null;
			}

			Mesh newMesh = new Mesh();

			Vector3[] newVertices = new Vector3[count * 3];
			Vector2[] newUvs = new Vector2[count * 3];
			int[] newIndices = new int[count * 3];

			int addedCount = 0;

			// fill our mesh arrays
			for (int i = 0; i < count; i++) {
				Triangle newTri = hull[i];

				int i0 = i + addedCount + 0;
				int i1 = i + addedCount + 1;
				int i2 = i + addedCount + 2;

				newVertices[i0] = newTri.positionA;
				newVertices[i1] = newTri.positionB;
				newVertices[i2] = newTri.positionC;

				newUvs[i0] = newTri.uvA;
				newUvs[i1] = newTri.uvB;
				newUvs[i2] = newTri.uvC;

				newIndices[i0] = i0;
				newIndices[i1] = i1;
				newIndices[i2] = i2;
			}

			// fill the mesh structure
			newMesh.vertices = newVertices;
			newMesh.uv = newUvs;
			newMesh.triangles = newIndices;

			// consider computing this array externally instead
			newMesh.RecalculateNormals();

			return newMesh;
		}
	}
}