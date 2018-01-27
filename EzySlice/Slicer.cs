using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EzySlice {
	public sealed class Slicer {

		/**
		 * Helper function which will slice the provided object with the provided plane
		 * and instantiate and return the final GameObjects
		 * 
		 * This function will return null if the object failed to slice
		 */
		public static GameObject[] SliceInstantiate(GameObject obj, Plane pl, bool genCrossSection = true) {
			SlicedHull slice = Slice(obj, pl, genCrossSection);

			if (slice == null) {
				return null;
			}

			GameObject upperHull = slice.CreateUpperHull();

			if (upperHull != null) {
				// set the positional information
				upperHull.transform.position = obj.transform.position;
				upperHull.transform.rotation = obj.transform.rotation;
				upperHull.transform.localScale = obj.transform.localScale;

				// the the material information
				upperHull.GetComponent<Renderer>().sharedMaterials = obj.GetComponent<MeshRenderer>().sharedMaterials;
			}

			GameObject lowerHull = slice.CreateLowerHull();

			if (lowerHull != null) {
				// set the positional information
				lowerHull.transform.position = obj.transform.position;
				lowerHull.transform.rotation = obj.transform.rotation;
				lowerHull.transform.localScale = obj.transform.localScale;

				// the the material information
				lowerHull.GetComponent<Renderer>().sharedMaterials = obj.GetComponent<MeshRenderer>().sharedMaterials;
			}

			// return both if upper and lower hulls were generated
			if (upperHull != null && lowerHull != null) {
				return new GameObject[] {upperHull, lowerHull};
			}

			// otherwise return only the upper hull
			if (upperHull != null) {
				return new GameObject[] {upperHull};
			}

			// otherwise return null
			return null;
		}

		/**
		 * Helper function to accept a gameobject which will transform the plane
		 * approprietly before the slice occurs
		 * See -> Slice(Mesh, Plane) for more info
		 */
		public static SlicedHull Slice(GameObject obj, Plane pl, bool genCrossSection = true) {
			MeshFilter renderer = obj.GetComponent<MeshFilter>();

			if (renderer == null) {
				return null;
			}

			return Slice(renderer.sharedMesh, pl, genCrossSection);
		}

		/**
		 * Slice the gameobject mesh (if any) using the Plane, which will generate
		 * a maximum of 2 other Meshes.
		 * This function will recalculate new UV coordinates to ensure textures are applied
		 * properly.
		 * Returns null if no intersection has been found or the GameObject does not contain
		 * a valid mesh to cut.
		 */
		public static SlicedHull Slice(Mesh sharedMesh, Plane pl, bool genCrossSection = true) {
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
						upperHull.Add(result.upperHull[i]);
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

			// we don't want cross sections, 
			if (!genCrossSection) {
				// start creating our hulls
				Mesh finalUpperHullNoCross = CreateFrom(upperHull, null);
				Mesh finalLowerHullNoCross = CreateFrom(lowerHull, null);

				return new SlicedHull(finalUpperHullNoCross, finalLowerHullNoCross);
			}

			// we need to generate the cross section if set
			// NOTE -> This uses a MonotoneChain algorithm which will only work
			// on cross sections which are Convex
			List<Triangle> crossSection = CreateFrom(crossHull, pl.normal);

			Mesh finalUpperHull = CreateFrom(upperHull, crossSection, true);
			Mesh finalLowerHull = CreateFrom(lowerHull, crossSection, false);

			return new SlicedHull(finalUpperHull, finalLowerHull);
		}

		/**
		 * Generate Two Meshes (an upper and lower) cross section from a set of intersection
		 * points and a plane normal. Intersection Points do not have to be in order.
		 */
		private static List<Triangle> CreateFrom(List<Vector3> intPoints, Vector3 planeNormal) {
			List<Triangle> tris;

			if (Triangulator.MonotoneChain(intPoints, planeNormal, out tris)) {
				return tris;
			}

			return null;
		}

		/**
		 * Generate a mesh from the provided hull made of triangles
		 * ADDED -> Generate the Cross Section into the same mesh
		 */
		private static Mesh CreateFrom(List<Triangle> hull, List<Triangle> crossSection, bool isUpper = true) {
			int count = crossSection == null ? hull.Count : (hull.Count + crossSection.Count);

			if (count <= 0) {
				return null;
			}

			Mesh newMesh = new Mesh();

			int hullCount = hull.Count;

			Vector3[] newVertices = new Vector3[count * 3];
			Vector2[] newUvs = new Vector2[count * 3];
			int[] newIndices = new int[hullCount * 3];

			int addedCount = 0;

			// fill our mesh arrays
			for (int i = 0; i < hullCount; i++) {
				Triangle newTri = hull[i];

				int i0 = addedCount + 0;
				int i1 = addedCount + 1;
				int i2 = addedCount + 2;

				newVertices[i0] = newTri.positionA;
				newVertices[i1] = newTri.positionB;
				newVertices[i2] = newTri.positionC;

				newUvs[i0] = newTri.uvA;
				newUvs[i1] = newTri.uvB;
				newUvs[i2] = newTri.uvC;

				// triangles are returned in clocwise order from the
				// intersector, no need to sort these
				newIndices[i0] = i0;
				newIndices[i1] = i1;
				newIndices[i2] = i2;

				addedCount += 3;
			}

			int[] crossIndices = null;

			// also generate our cross sections
			if (crossSection != null) {
				int crossCount = crossSection.Count;
				int crossIndex = 0;

				crossIndices = new int[crossCount * 3];

				for (int i = 0; i < crossCount; i++) {
					Triangle newTri = crossSection[i];

					int i0 = addedCount + 0;
					int i1 = addedCount + 1;
					int i2 = addedCount + 2;

					newVertices[i0] = newTri.positionA;
					newVertices[i1] = newTri.positionB;
					newVertices[i2] = newTri.positionC;

					newUvs[i0] = newTri.uvA;
					newUvs[i1] = newTri.uvB;
					newUvs[i2] = newTri.uvC;

					// add triangles in clockwise for upper
					// and reversed for lower hulls, to ensure the mesh
					// is facing the right direction
					if (isUpper) {
						crossIndices[crossIndex] = i0;
						crossIndices[crossIndex + 1] = i1;
						crossIndices[crossIndex + 2] = i2;
					}
					else {
						crossIndices[crossIndex] = i0;
						crossIndices[crossIndex + 1] = i2;
						crossIndices[crossIndex + 2] = i1;
					}

					addedCount += 3;
					crossIndex += 3;
				}
			}

			newMesh.subMeshCount = 2;
			// fill the mesh structure
			newMesh.vertices = newVertices;
			newMesh.uv = newUvs;

			// set the first group of triangles
			newMesh.SetTriangles(newIndices, 0, false);

			// set the cross indices if they exist
			if (crossIndices != null) {
				newMesh.SetTriangles(crossIndices, 1, false);
			}

			// consider computing this array externally instead
			newMesh.RecalculateNormals();

			return newMesh;
		}
	}
}