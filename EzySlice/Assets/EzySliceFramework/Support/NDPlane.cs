using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EzySlice {

    /*
     * Represents an infinite plane in 3D space. Computed from
     * A position and a normalized direction vector.
     * Modified to accept reference Vector's for controlling
     * Garbage Collection.
     */
    public class NDPlane {
        public const float ERROR_TOL = 0.0001f;

        private Vector3 normal;
        private float distance;

        public NDPlane() {
            ComputePlane(Vector3.zero, Vector3.one);
        }

        public NDPlane(ref Vector3 position, ref Vector3 normal) {
            ComputePlane(ref position, ref normal);
        }

        public NDPlane(Vector3 position, Vector3 normal) {
            ComputePlane(ref position, ref normal);
        }

        /*
         * Compute this plane from a position and a direction.
         */
        public void ComputePlane(ref Vector3 position, ref Vector3 normal) {
            this.normal = normal;
            this.distance = Vector3.Dot(normal, position);
        }

        public void ComputePlane(Vector3 position, Vector3 normal) {
            ComputePlane(ref position, ref normal);
        }

        public Vector3 Normal {
            get { return this.normal ; }
        }

        public float Distance {
            get { return this.distance; }
        }

        /*
         * Test and return which side of this infinite ND Plane
         * the point PT is on. Takes a reference value.
         */
        public bool SideOfPlane(ref Vector3 pt) {
            return Vector3.Dot(normal, pt) >= distance;
        }

        public bool SideOfPlane(Vector3 pt) {
            return SideOfPlane(ref pt);
        }

        /*
         * Intersects the line made from Points a and b. Returns true
         * If an intersection is found, otherwise returns false.
         */
        public bool IntersectLine(ref Vector3 a, ref Vector3 b) {
            Vector3 ab = b - a;

            float t = (distance - Vector3.Dot(normal, a)) / Vector3.Dot(normal, ab);

            return t >= 0F && t <= 1F;
        }

        public bool IntersectLine(Vector3 a, Vector3 b) {
            return IntersectLine(ref a, ref b);
        }

        /*
         * Intersects the line made from Points a and b. Returns true
         * If an intersection is found, otherwise returns false. The intersection
         * Point will be stored in q.
         */
        public bool IntersectLine(ref Vector3 a, ref Vector3 b, ref Vector3 q) {
            Vector3 ab = b - a;

            float t = (distance - Vector3.Dot(normal, a)) / Vector3.Dot(normal, ab);

            if (t >= 0F && t <= 1F) {
                q = a + t * ab;

                return true;
            }

            return false;
        }

        public bool IntersectLine(Vector3 a, Vector3 b, ref Vector3 q) {
            return IntersectLine(ref a, ref b, ref q);
        }

        /*
         * Intersect the triangle composed of points a-b-c and store potential intersection
         * points in List fptr.
         * NOTE: List fpts will not be cleared, previous points in the list will remain.
         */
        public bool IntersectTriangle(ref Vector3 a, ref Vector3 b, ref Vector3 c, List<Vector3> fpts) {
            if (SideOfPlane(ref a) && SideOfPlane(ref b) && SideOfPlane(ref c)) {
                return false;
            }

            Vector3 intersectionPt1 = Vector3.zero;
            Vector3 intersectionPt2 = Vector3.zero;
            Vector3 intersectionPt3 = Vector3.zero;

            // test segment a-b
            if (IntersectLine(ref a, ref b, ref intersectionPt1)) {
                fpts.Add(intersectionPt1);
            }

            // test segment a-c
            if (IntersectLine(ref a, ref c, ref intersectionPt2)) {
                // don't add the same intersection point
                if ((intersectionPt1 - intersectionPt2).sqrMagnitude > 0F) {
                    fpts.Add(intersectionPt2);
                }
            }

            // test segment b-c
            if (IntersectLine(ref b, ref c, ref intersectionPt3)) {
                // don't add the same intersection point
                if ((intersectionPt1 - intersectionPt3).sqrMagnitude > 0F && (intersectionPt2 - intersectionPt3).sqrMagnitude > 0F) {
                    fpts.Add(intersectionPt3);
                }
            }

            return true;
        }

        /*
         * Intersect the triangle composed of points a-b-c and store potential intersection
         * points in List fptr.
         * This function will also generate relevant UV Coordinates of the intersection points
         * NOTE: List fpts will not be cleared, previous points in the list will remain.
         */
        public bool IntersectTriangle(ref Vector3 a, ref Vector3 b, ref Vector3 c, ref Vector2 uvA, ref Vector2 uvB, ref Vector2 uvC, List<Vector3> fpts, List<Vector2> fptsUV) {
            if (SideOfPlane(ref a) && SideOfPlane(ref b) && SideOfPlane(ref c)) {
                return false;
            }

            Vector3 intersectionPt1 = Vector3.zero;
            Vector3 intersectionPt2 = Vector3.zero;
            Vector3 intersectionPt3 = Vector3.zero;

            // test segment a-b
            if (IntersectLine(ref a, ref b, ref intersectionPt1)) {
                fpts.Add(intersectionPt1);

                fptsUV.Add(Triangulator.GenerateUVCoords(ref a, ref b, ref c, ref uvA, ref uvB, ref uvC, ref intersectionPt1));
            }

            // test segment a-c
            if (IntersectLine(ref a, ref c, ref intersectionPt2)) {
                // don't add the same intersection point
                if ((intersectionPt1 - intersectionPt2).sqrMagnitude > 0F) {
                    fpts.Add(intersectionPt2);
                    fptsUV.Add(Triangulator.GenerateUVCoords(ref a, ref b, ref c, ref uvA, ref uvB, ref uvC, ref intersectionPt2));
                }
            }

            // test segment b-c
            if (IntersectLine(ref b, ref c, ref intersectionPt3)) {
                // don't add the same intersection point
                if ((intersectionPt1 - intersectionPt3).sqrMagnitude > 0F && (intersectionPt2 - intersectionPt3).sqrMagnitude > 0F) {
                    fpts.Add(intersectionPt3);
                    fptsUV.Add(Triangulator.GenerateUVCoords(ref a, ref b, ref c, ref uvA, ref uvB, ref uvC, ref intersectionPt3));
                }
            }

            return true;
        }

        public void IntersectTriangle(Vector3 a, Vector3 b, Vector3 c, List<Vector3> fpts) {
            IntersectTriangle(ref a, ref b, ref c, fpts);
        }

        /*
         * Performs an intersection on the triangle composed of points a-b-c. This function will store all points and
         * relevant intersection points in the lower and upper Lists. NOTE: Lists will not be cleared in this function
         * lower - Any point (if any) that fall below the NDPlane
         * upper - Any point (if any) that fall above the NDPlane
         * fpts - The intersection points. This is shared between lower and upper hulls
         * This function is useful for generating hulls between cuts of the ND plane, as the upper and lower hulls
         * will belong to different GameObjects
         */
        public void IntersectTriangleHull(ref Vector3 a, 
                                          ref Vector3 b, 
                                          ref Vector3 c, 
                                          List<Vector3> lower, 
                                          List<Vector3> upper, 
                                          List<Vector3> fpts) 
        {
            // quick test, if no intersection, just do a lower/upper addition
            if (!IntersectTriangle(ref a, ref b, ref c, fpts)) {
                if (SideOfPlane(ref a)) {
                    lower.Add(a);
                    lower.Add(b);
                    lower.Add(c);
                } 
                else {
                    upper.Add(a);
                    upper.Add(b);
                    upper.Add(c);
                }

                return;
            }

            // if passed, the intersection points will be in fpts
            // perform a simple filter process to remove duplicate vertices
            if (!Contains(fpts, ref a)) {
                if (SideOfPlane(ref a)) { lower.Add(a); } else { upper.Add(a); }
            }

            if (!Contains(fpts, ref b)) {
                if (SideOfPlane(ref b)) { lower.Add(b); } else { upper.Add(b); }
            }

            if (!Contains(fpts, ref c)) {
                if (SideOfPlane(ref c)) { lower.Add(c); } else { upper.Add(c); }
            }
        }

        /*
         * Performs an intersection on the triangle composed of points a-b-c. This function will store all points and
         * relevant intersection points in the lower and upper Lists. An addition is that this function
         * will also generate relevant UV Coordinates of all the cut points. NOTE: Lists will not be cleared in this function
         * lower - Any point (if any) that fall below the NDPlane
         * lowerUV - Any point (if any) that fall below the NDPlane, This generates UV Coordinates
         * upper - Any point (if any) that fall above the NDPlane
         * upperUV - Any point (if any) that fall above the NDPlane, This generates UV Coordinares
         * fpts - The intersection points. This is shared between lower and upper hulls
         * fpts - The intersection points UV Coordinates. This is shared between lower and upper hulls
         * This function is useful for generating hulls between cuts of the ND plane, as the upper and lower hulls
         * will belong to different GameObjects
         */
        public void IntersectTriangleHull(ref Vector3 a,
                                          ref Vector3 b,
                                          ref Vector3 c,
                                          ref Vector2 uvA,
                                          ref Vector2 uvB,
                                          ref Vector2 uvC,
                                          List<Vector3> lower,
                                          List<Vector2> lowerUV,
                                          List<Vector3> upper,
                                          List<Vector2> upperUV,
                                          List<Vector3> fpts,
                                          List<Vector2> fptsUV) 
        {
            // quick test, if no intersection, just do a lower/upper addition
            if (!IntersectTriangle(ref a, ref b, ref c, ref uvA, ref uvB, ref uvC, fpts, fptsUV)) {
                if (SideOfPlane(ref a)) {
                    lower.Add(a);
                    lower.Add(b);
                    lower.Add(c);
                    lowerUV.Add(uvA);
                    lowerUV.Add(uvB);
                    lowerUV.Add(uvC);
                } else {
                    upper.Add(a);
                    upper.Add(b);
                    upper.Add(c);
                    upper.Add(uvA);
                    upper.Add(uvB);
                    upper.Add(uvC);
                }

                return;
            }

            // if passed, the intersection points will be in fpts
            // perform a simple filter process to remove duplicate vertices
            if (!Contains(fpts, ref a)) {
                if (SideOfPlane(ref a)) {
                    lower.Add(a);
                    lowerUV.Add(uvA);
                } 
                else {
                    upper.Add(a);
                    upperUV.Add(uvA);
                }
            }

            if (!Contains(fpts, ref b)) {
                if (SideOfPlane(ref b)) {
                    lower.Add(b);
                    lowerUV.Add(uvB);
                }
                else {
                    upper.Add(b);
                    upperUV.Add(uvB);
                }
            }

            if (!Contains(fpts, ref c)) {
                if (SideOfPlane(ref c)) {
                    lower.Add(c);
                    lowerUV.Add(uvC);
                } 
                else {
                    upper.Add(c);
                    upperUV.Add(uvC);
                }
            }
        }

        public void IntersectTriangleHull(Vector3 a, Vector3 b, Vector3 c, List<Vector3> lower, List<Vector3> upper, List<Vector3> fpts) {
            IntersectTriangleHull(ref a, ref b, ref c, lower, upper, fpts);
        }

        public void IntersectTriangleHull(Vector3 a, Vector3 b, Vector3 c, Vector2 uvA, Vector2 uvB, Vector2 uvC, List<Vector3> lower, List<Vector2> lowerUV, List<Vector3> upper, List<Vector2> upperUV, List<Vector3> fpts, List<Vector2> fptsUV) {
            IntersectTriangleHull(ref a, ref b, ref c, ref uvA, ref uvB, ref uvC, lower, lowerUV, upper, upperUV, fpts, fptsUV);
        }

        /*
         * Performs a quick overlap test between a Sphere given a center and a radius
         * and this plane. Returns true if there is an overlap, otherwise returns false.
         */
        public bool OverlapSphere(ref Vector3 center, float radius) {
            float dist = Vector3.Dot(center, normal) - distance;

            return Mathf.Abs(dist) <= radius;
        }

        public bool OverlapSphere(Vector3 center, float radius) {
            return OverlapSphere(ref center, radius);
        }

        private bool Contains(List<Vector3> toCheck, ref Vector3 vec) {
            int count = toCheck.Count;

            for (int i = 0; i < count; i++) {
                Vector3 toCheckVec = toCheck[i];

                if ((toCheckVec - vec).sqrMagnitude <= 0.0001f) {
                    return true;
                }
            }

            return false;
        }
    }
}
