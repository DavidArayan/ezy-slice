using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EzySlice {
	public sealed class Slicer {

		/**
		 * An internal class for storing internal submesh values
		 */ 
		internal class SlicedSubmesh {
			public readonly List<Triangle> upperHull = new List<Triangle>();
			public readonly List<Triangle> lowerHull = new List<Triangle>();
		}

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

			int submeshCount = sharedMesh.subMeshCount;

			// each submesh will be sliced and placed in its own array structure
			SlicedSubmesh[] slices = new SlicedSubmesh[submeshCount];
			// the cross section hull is common across all submeshes
			List<Vector3> crossHull = new List<Vector3>();

			// we reuse this object for all intersection tests
			IntersectionResult result = new IntersectionResult();

            // only generate UV coordinates if the mesh has any
            bool genUV = ve.Length == uv.Length;

			// iterate over all the submeshes individually. vertices and indices
			// are all shared within the submesh
			for (int submesh = 0; submesh < submeshCount; submesh++) {
				int[] indices = sharedMesh.GetTriangles(submesh);
				int indicesCount = indices.Length;

				SlicedSubmesh mesh = new SlicedSubmesh();

				// loop through all the mesh vertices, generating upper and lower hulls
				// and all intersection points
				for (int index = 0; index < indicesCount; index += 3) {
					int i0 = indices[index + 0];
					int i1 = indices[index + 1];
					int i2 = indices[index + 2];

					Triangle newTri = genUV ? 
                        new Triangle(ve[i0], ve[i1], ve[i2], uv[i0], uv[i1], uv[i2]) :
                        new Triangle(ve[i0], ve[i1], ve[i2]);

					// slice this particular triangle with the provided
					// plane
					if (newTri.Split(pl, result)) {
						int upperHullCount = result.upperHullCount;
						int lowerHullCount = result.lowerHullCount;
						int interHullCount = result.intersectionPointCount;

						for (int i = 0; i < upperHullCount; i++) {
							mesh.upperHull.Add(result.upperHull[i]);
						}

						for (int i = 0; i < lowerHullCount; i++) {
							mesh.lowerHull.Add(result.lowerHull[i]);
						}

						for (int i = 0; i < interHullCount; i++) {
							crossHull.Add(result.intersectionPoints[i]);
						}
					}
					else {
						SideOfPlane side = pl.SideOf(ve[i0]);

						if (side == SideOfPlane.UP || side == SideOfPlane.ON) {
							mesh.upperHull.Add(newTri);
						}
						else {
							mesh.lowerHull.Add(newTri);
						}
					}
				}

				// register into the index
				slices[submesh] = mesh;
			}

			return CreateFrom(slices, CreateFrom(crossHull, pl.normal));
		}

		/**
		 * Generates a single SlicedHull from a set of cut submeshes 
		 */
		private static SlicedHull CreateFrom(SlicedSubmesh[] meshes, List<Triangle> cross) {
			int submeshCount = meshes.Length;

			int upperHullCount = 0;
			int lowerHullCount = 0;

			// get the total amount of upper, lower and intersection counts
			for (int submesh = 0; submesh < submeshCount; submesh++) {
				upperHullCount += meshes[submesh].upperHull.Count;
				lowerHullCount += meshes[submesh].lowerHull.Count;
			}

			Mesh upperHull = CreateHull(meshes, upperHullCount, cross, true);
			Mesh lowerHull = CreateHull(meshes, lowerHullCount, cross, false);

			return new SlicedHull(upperHull, lowerHull);
		}

		/**
		 * Generate a single Mesh HULL of either the UPPER or LOWER hulls. 
		 */
		private static Mesh CreateHull(SlicedSubmesh[] meshes, int total, List<Triangle> crossSection, bool isUpper) {
			if (total <= 0) {
				return null;
			}

			int submeshCount = meshes.Length;
			int crossCount = crossSection != null ? crossSection.Count : 0;

			Mesh newMesh = new Mesh();

			// vertices and uv's are common for all submeshes
			Vector3[] newVertices = new Vector3[(total + crossCount) * 3];
			Vector2[] newUvs = new Vector2[(total + crossCount) * 3];

			// each index refers to our submesh triangles
			List<int[]> triangles = new List<int[]>(submeshCount);

			int vIndex = 0;

			// first we generate all our vertices, uv's and triangles
			for (int submesh = 0; submesh < submeshCount; submesh++) {
				// pick the hull we will be playing around with
				List<Triangle> hull = isUpper ? meshes[submesh].upperHull : meshes[submesh].lowerHull;
				int hullCount = hull.Count;

				int[] indices = new int[hullCount * 3];

				// fill our mesh arrays
				for (int i = 0, triIndex = 0; i < hullCount; i++, triIndex += 3) {
					Triangle newTri = hull[i];

					int i0 = vIndex + 0;
					int i1 = vIndex + 1;
					int i2 = vIndex + 2;

					newVertices[i0] = newTri.positionA;
					newVertices[i1] = newTri.positionB;
					newVertices[i2] = newTri.positionC;

					newUvs[i0] = newTri.uvA;
					newUvs[i1] = newTri.uvB;
					newUvs[i2] = newTri.uvC;

					// triangles are returned in clocwise order from the
					// intersector, no need to sort these
					indices[triIndex] = i0;
					indices[triIndex + 1] = i1;
					indices[triIndex + 2] = i2;

					vIndex += 3;
				}

				// add triangles to the index for later generation
				triangles.Add(indices);
			}

			// generate the cross section required for this particular hull
			if (crossSection != null && crossCount > 0) {
				int[] crossIndices = new int[crossCount * 3];

				for (int i = 0, triIndex = 0; i < crossCount; i++, triIndex += 3) {
					Triangle newTri = crossSection[i];

					int i0 = vIndex + 0;
					int i1 = vIndex + 1;
					int i2 = vIndex + 2;

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
						crossIndices[triIndex] = i0;
						crossIndices[triIndex + 1] = i1;
						crossIndices[triIndex + 2] = i2;
					}
					else {
						crossIndices[triIndex] = i0;
						crossIndices[triIndex + 1] = i2;
						crossIndices[triIndex + 2] = i1;
					}

					vIndex += 3;
				}

				// add triangles to the index for later generation
				triangles.Add(crossIndices);
			}

			int totalTriangles = triangles.Count;

			newMesh.subMeshCount = totalTriangles;
			// fill the mesh structure
			newMesh.vertices = newVertices;
			newMesh.uv = newUvs;

			// add the submeshes
			for (int i = 0; i < totalTriangles; i++) {
				newMesh.SetTriangles(triangles[i], i, false);
			}

            newMesh.RecalculateNormals();

			return newMesh;
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
	}
}