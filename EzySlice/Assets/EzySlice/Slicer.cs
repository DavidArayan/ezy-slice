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

			// start creating our hulls
			Mesh finalUpperHull = CreateFrom(upperHull);
			Mesh finalLowerHull = CreateFrom(lowerHull);

			// we need to generate the cross section if set
			// NOTE -> This uses a MonotoneChain algorithm which will only work
			// on cross sections which are Convex
			if (genCrossSection) {
				Mesh[] crossSections = CreateFrom(crossHull, pl.normal);

				if (crossSections != null) {
					return new SlicedHull(finalUpperHull, finalLowerHull, crossSections[0], crossSections[1]);
				}
			}

			return new SlicedHull(finalUpperHull, finalLowerHull);
		}

		/**
		 * Generate Two Meshes (an upper and lower) cross section from a set of intersection
		 * points and a plane normal. Intersection Points do not have to be in order.
		 */
		private static Mesh[] CreateFrom(List<Vector3> intPoints, Vector3 planeNormal) {
			Vector3[] newVertices;
			Vector2[] newUvs;
			int[] newIndices;

			if (Triangulator.MonotoneChain(intPoints, planeNormal, out newVertices, out newIndices, out newUvs)) {
				Mesh upperCrossSection = new Mesh();

				// fill the mesh structure
				upperCrossSection.vertices = newVertices;
				upperCrossSection.uv = newUvs;
				upperCrossSection.triangles = newIndices;

				// consider computing this array externally instead
				upperCrossSection.RecalculateNormals();

				// for the lower cross section, we need to flip the triangles so they are 
				// facing the right way
				int indiceCount = newIndices.Length;
				int[] flippedIndices = new int[indiceCount];

				for (int i = 0; i < indiceCount; i+=3) {
					flippedIndices[i] = newIndices[i];
					flippedIndices[i + 1] = newIndices[i + 2];
					flippedIndices[i + 2] = newIndices[i + 1];
				}

				Mesh lowerCrossSection = new Mesh();

				// fill the mesh structure
				lowerCrossSection.vertices = newVertices;
				lowerCrossSection.uv = newUvs;
				lowerCrossSection.triangles = flippedIndices;

				// consider computing this array externally instead
				lowerCrossSection.RecalculateNormals();

				return new Mesh[] {upperCrossSection, lowerCrossSection};
			}

			return null;
		}

		/**
		 * Generate a mesh from the provided hull made of triangles
		 */
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

				int i0 = addedCount + 0;
				int i1 = addedCount + 1;
				int i2 = addedCount + 2;

				newVertices[i0] = newTri.positionA;
				newVertices[i1] = newTri.positionB;
				newVertices[i2] = newTri.positionC;

				newUvs[i0] = newTri.uvA;
				newUvs[i1] = newTri.uvB;
				newUvs[i2] = newTri.uvC;

				// note -> this case could be optimized by having the order
				// returned properly from the intersector
				if (newTri.IsCW()) {
					newIndices[i0] = i0;
					newIndices[i1] = i1;
					newIndices[i2] = i2;	
				}
				else {
					newIndices[i0] = i0;
					newIndices[i1] = i2;
					newIndices[i2] = i1;
				}

				addedCount += 3;
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