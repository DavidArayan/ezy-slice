using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EzySlice {
    public class MeshSlicer {
        private static List<Vector3> tmpLowerHull = new List<Vector3>();
        private static List<Vector3> tmpUpperHull = new List<Vector3>();
        private static List<Vector3> tmpIntersectionPt = new List<Vector3>();
        private static List<Vector2> tmpIntersectionUv = new List<Vector2>();
        private static List<Vector2> tmpLowerHullUV = new List<Vector2>();
        private static List<Vector2> tmpUpperHullUV = new List<Vector2>();
        private static List<int> tmpLowerIndices = new List<int>();
        private static List<int> tmpUpperIndices = new List<int>();

        public static List<GameObject> CutObjectInstantiate(GameObject obj, NDPlane plane, bool destroyPrevious = false) {
            List<Mesh> newMeshes = CutObject(obj, plane);

            List<GameObject> finalObjects = new List<GameObject>();

            int finalMeshCount = newMeshes.Count;

            if (finalMeshCount == 0) {
                return finalObjects;
            }

            for (int i = 0; i < finalMeshCount; i++) {
                GameObject newObject = new GameObject(obj.name + "+" + i);

                MeshRenderer renderer = newObject.AddComponent<MeshRenderer>();
                MeshFilter filter = newObject.AddComponent<MeshFilter>();

                filter.mesh = newMeshes[i];

                newObject.transform.position = obj.transform.position;
                newObject.transform.rotation = obj.transform.rotation;
                newObject.transform.localScale = obj.transform.localScale;

                renderer.materials = obj.GetComponent<MeshRenderer>().materials;

                finalObjects.Add(newObject);
            }

            if (destroyPrevious) {
                Object.Destroy(obj);
            }

            return finalObjects;
        }

        /*
         * Cuts the given object using the provided NDPlane. This function returns a List of Meshes
         * which is the final mesh cut by the NDPlane. At most, this function will return 2 meshes.
         */
        public static List<Mesh> CutObject(GameObject obj, NDPlane plane) {
            List<Mesh> newMeshes = new List<Mesh>();

            if (plane == null) {
                return newMeshes;
            }

            MeshFilter renderer = obj.GetComponent<MeshFilter>();

            if (renderer == null) {
                return newMeshes;
            }

            Mesh sharedMesh = renderer.sharedMesh;

            if (sharedMesh == null) {
                return newMeshes;
            }

            Vector3[] vertices = sharedMesh.vertices;
            Vector2[] uv = sharedMesh.uv;
            int[] indices = sharedMesh.triangles;

            // the primary buffers which will be used to construct a new mesh
            List<Vector3> upperHull = new List<Vector3>();
            List<Vector3> lowerHull = new List<Vector3>();
            List<int> upperIndices = new List<int>();
            List<int> lowerIndices = new List<int>();
            List<Vector2> upperUV = new List<Vector2>();
            List<Vector2> lowerUV = new List<Vector2>();
            List<Vector3> closingHull = new List<Vector3>();
            List<Vector2> closingHullUV = new List<Vector2>();
            List<int> closingHullIndices = new List<int>();

            int indicesCount = indices.Length;

            for (int index = 0; index < indicesCount; index += 3) {
                // get the first triangles
                Vector3 tri1 = vertices[indices[index]];
                Vector3 tri2 = vertices[indices[index + 1]];
                Vector3 tri3 = vertices[indices[index + 2]];

                // get the first triangle UV's
                Vector2 tri1uva = uv[indices[index]];
                Vector2 tri1uvb = uv[indices[index + 1]];
                Vector2 tri1uvc = uv[indices[index + 2]];

                // clear our temporary Buffers
                tmpLowerHull.Clear();
                tmpUpperHull.Clear();
                tmpIntersectionPt.Clear();
                tmpIntersectionUv.Clear();
                tmpLowerHullUV.Clear();
                tmpUpperHullUV.Clear();
                tmpLowerIndices.Clear();
                tmpUpperIndices.Clear();

                // intersect the first triangle
                plane.IntersectTriangleHull(ref tri1, 
                                            ref tri2, 
                                            ref tri3, 
                                            ref tri1uva, 
                                            ref tri1uvb, 
                                            ref tri1uvc, 
                                            tmpLowerHull, 
                                            tmpLowerHullUV,
                                            tmpUpperHull, 
                                            tmpUpperHullUV, 
                                            tmpIntersectionPt, 
                                            tmpIntersectionUv);

                // add intersection point into overall buffers
                tmpLowerHull.AddRange(tmpIntersectionPt);
                tmpUpperHull.AddRange(tmpIntersectionPt);
                tmpLowerHullUV.AddRange(tmpIntersectionUv);
                tmpUpperHullUV.AddRange(tmpIntersectionUv);

                closingHull.AddRange(tmpIntersectionPt);

                // triangulate the temporary buffers and get the indices
                Triangulator.TriangulateNDSlice(tmpLowerHull, tmpLowerIndices, lowerHull.Count);
                Triangulator.TriangulateNDSlice(tmpUpperHull, tmpUpperIndices, upperHull.Count);

                // add to the final buffers
                upperHull.AddRange(tmpUpperHull);
                lowerHull.AddRange(tmpLowerHull);
                upperIndices.AddRange(tmpUpperIndices);
                lowerIndices.AddRange(tmpLowerIndices);
                upperUV.AddRange(tmpUpperHullUV);
                lowerUV.AddRange(tmpLowerHullUV);
            }

            List<Vector3> finalClosingHull = new List<Vector3>();

            // generate the closing hull (if any)
            if (closingHull.Count > 0) {
                Triangulator.TriangulateHullPt(closingHull, finalClosingHull, closingHullIndices, closingHullUV, plane.Normal);
            }

            if (upperIndices.Count > 0) {
                Mesh newMesh = new Mesh();

                newMesh.subMeshCount = 2;

                // close the hull
                int count = upperHull.Count;
                upperHull.AddRange(finalClosingHull);
                upperUV.AddRange(closingHullUV);

                newMesh.SetVertices(upperHull);
                newMesh.SetTriangles(upperIndices, 0);
                newMesh.SetUVs(0, upperUV);

                // set the closing hull UV's
                int triCount = closingHullIndices.Count;

                if (triCount > 0) {
                    List<int> finalTriangles = new List<int>();

                    for (int i = 0; i < triCount; i += 3) {
                        finalTriangles.Add(closingHullIndices[i] + count);
                        finalTriangles.Add(closingHullIndices[i + 2] + count);
                        finalTriangles.Add(closingHullIndices[i + 1] + count);
                    }

                    newMesh.SetTriangles(finalTriangles, 1);
                }

                newMesh.RecalculateNormals();

                newMeshes.Add(newMesh);
            }

            if (lowerIndices.Count > 0) {
                Mesh newMesh = new Mesh();

                newMesh.subMeshCount = 2;

                // close the hull
                int count = lowerHull.Count;
                lowerHull.AddRange(finalClosingHull);
                lowerUV.AddRange(closingHullUV);

                newMesh.SetVertices(lowerHull);
                newMesh.SetTriangles(lowerIndices, 0);
                newMesh.SetUVs(0, lowerUV);

                // set the closing hull UV's
                int triCount = closingHullIndices.Count;

                if (triCount > 0) {
                    List<int> finalTriangles = new List<int>();

                    for (int i = 0; i < triCount; i += 3) {
                        finalTriangles.Add(closingHullIndices[i] + count);
                        finalTriangles.Add(closingHullIndices[i + 1] + count);
                        finalTriangles.Add(closingHullIndices[i + 2] + count);
                    }

                    newMesh.SetTriangles(finalTriangles, 1);
                }

                newMesh.RecalculateNormals();

                newMeshes.Add(newMesh);
            }

            return newMeshes;
        }
    }
}
