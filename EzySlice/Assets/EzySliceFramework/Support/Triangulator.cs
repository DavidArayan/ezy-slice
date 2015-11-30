using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EzySlice {
    public class Triangulator {

        /*
         * Triangulates the points returned by NDPlane used to slice a single 3D triangle.
         * This function will expect a maximum of 4 points in hullPoints. If the count is greater
         * than 4, a filtering mechanism will be used to try and remove duplicate vertices. If the
         * filtering fails, this function will no longer proceed. WARNING: The filtering process
         * is very expensive O(n^2) complexity.
         * indices linking back to the original hullPoints to form a triangle will
         * be added in triangles
         */
        public static void TriangulateNDSlice(List<Vector3> hullPoints, List<int> triangles) {
            int count = hullPoints.Count;

            // do nothing if there are less than 3 vertices. We need minimum of 3 to triangluate
            if (count < 3) {
                return;
            }

            // we need to filter this special case
            if (count > 4) {
                count = FilterCommonVertices(hullPoints, 0, count);
            }

            // filtering has failed, return without processing
            if (count > 4) {
                return;
            }

            // add the indices in CW order (face up) - only formes 1 triangle
            if (count == 3) {
                Vector3 c = hullPoints[2];
                Vector3 b = hullPoints[1];
                Vector3 a = hullPoints[0];

                if (SignedSquare(ref a, ref b, ref c) >= 0.0f) {
                    triangles.Add(0);
                    triangles.Add(1);
                    triangles.Add(2);

                    return;
                }

                triangles.Add(0);
                triangles.Add(2);
                triangles.Add(1);

                return;
            }

            // add indices in CW order (face up) - will form 2 triangles
            if (count == 4) {
                float bu = 0.0f;
                float bv = 0.0f;
                float bw = 0.0f;

                Vector3 d = hullPoints[3];
                Vector3 c = hullPoints[2];
                Vector3 b = hullPoints[1];
                Vector3 a = hullPoints[0];

                // compute the barycentric coordinates
                Barycentric(ref a, ref b, ref c, ref d, ref bu, ref bv, ref bw);

                // add the first triangle
                if (SignedSquare(ref a, ref b, ref c) >= 0.0f) {
                    triangles.Add(0);
                    triangles.Add(1);
                    triangles.Add(2);
                }
                else {
                    triangles.Add(0);
                    triangles.Add(2);
                    triangles.Add(1);
                }

                // if true, form another triangle b-c-d and exit
                if (bu < 0.0f && bv > 0.0f && bw > 0.0f) {
                    if (SignedSquare(ref b, ref c, ref d) >= 0.0f) {
                        triangles.Add(1);
                        triangles.Add(2);
                        triangles.Add(3);
                    } 
                    else {
                        triangles.Add(1);
                        triangles.Add(3);
                        triangles.Add(2);
                    }

                    return;
                }

                // if true, form another triangle b-a-d and exit
                if (bu > 0.0f && bv > 0.0f && bw < 0.0f) {
                    if (SignedSquare(ref b, ref a, ref d) >= 0.0f) {
                        triangles.Add(1);
                        triangles.Add(0);
                        triangles.Add(3);
                    } 
                    else {
                        triangles.Add(1);
                        triangles.Add(3);
                        triangles.Add(0);
                    }

                    return;
                }

                // if true, form another triangle a-c-d and exit
                if (bu > 0.0f && bv < 0.0f && bw > 0.0f) {
                    if (SignedSquare(ref a, ref c, ref d) >= 0.0f) {
                        triangles.Add(0);
                        triangles.Add(2);
                        triangles.Add(3);
                    } 
                    else {
                        triangles.Add(0);
                        triangles.Add(3);
                        triangles.Add(2);
                    }

                    return;
                }
            }
        }

        /*
         * Compute and return the signed square of triangles a - b - c. Mainly used
         * in computing if the triangle is CW or CCW
         */
        private static float SignedSquare(ref Vector3 a, ref Vector3 b, ref Vector3 c) {
            return (a.x * (b.y * c.z - b.z * c.y) - a.y * (b.x * c.z - b.z * c.x) + a.z * (b.x * c.y - b.y * c.x));
        }

        /*
         * Compute and return the barycentric coordinates of triangle a-b-c in respect to p.
         * Used in quadrelateral triangulation and generating UV coordinates for new points
         */
        private static void Barycentric(ref Vector3 a, ref Vector3 b, ref Vector3 c, ref Vector3 p, ref float u, ref float v, ref float w) {
            Vector3 m = Vector3.Cross(b - a, c - a);

            float nu;
            float nv;
            float ood;

            float x = Mathf.Abs(m.x);
            float y = Mathf.Abs(m.y);
            float z = Mathf.Abs(m.z);

            // compute areas of plane with largest projections
            if (x >= y && x >= z) {
                // area of PBC in yz plane
                nu = TriArea2D(ref p.y, ref p.z, ref b.y, ref b.z, ref c.y, ref c.z);
                // area of PCA in yz plane
                nv = TriArea2D(ref p.y, ref p.z, ref c.y, ref c.z, ref a.y, ref a.z);
                // 1/2*area of ABC in yz plane
                ood = 1.0f / m.x;
            } 
            else if (y >= x && y >= z) {
                // project in xz plane
                nu = TriArea2D(ref p.x, ref p.z, ref b.x, ref b.z, ref c.x, ref c.z);
                nv = TriArea2D(ref p.x, ref p.z, ref c.x, ref c.z, ref a.x, ref a.z);
                ood = 1.0f / -m.y;
            } 
            else {
                // project in xy plane
                nu = TriArea2D(ref p.x, ref p.y, ref b.x, ref b.y, ref c.x, ref c.y);
                nv = TriArea2D(ref p.x, ref p.y, ref c.x, ref c.y, ref a.x, ref a.y);
                ood = 1.0f / m.z;
            }

            u = nu * ood;
            v = nv * ood;
            w = 1.0f - u - v;
        }

        private static float TriArea2D(ref float x1, ref float y1, ref float x2, ref float y2, ref float x3, ref float y3) {
            return (x1 - x2) * (y2 - y3) - (x2 - x3) * (y1 - y2);
        }

        /*
         * This function will recursively filter the vertices from provided index up to end index
         */
        public static int FilterCommonVertices(List<Vector3> vec, int startIndex, int endIndex) {
            int totalCount = vec.Count;

            if (vec.Count <= 1) {
                return totalCount;
            }

            for (int i = startIndex; i < endIndex - 1; i++) {
                Vector3 testVector = vec[i];
                Vector3 endVector = vec[i + 1];

                if ((testVector - endVector).sqrMagnitude <= 0.0001f) {
                    vec.RemoveAt(i + 1);

                    return FilterCommonVertices(vec, startIndex, endIndex - 1);
                }
            }

            return totalCount;
        }
    }
}
