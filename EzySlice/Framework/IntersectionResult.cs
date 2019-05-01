using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EzySlice {

    /**
     * A Basic Structure which contains intersection information
     * for Plane->Triangle Intersection Tests
     * TO-DO -> This structure can be optimized to hold less data
     * via an optional indices array. Could lead for a faster
     * intersection test aswell.
     */
    public sealed class IntersectionResult {

        // general tag to check if this structure is valid
        private bool is_success;

        // our intersection points/triangles
        private readonly Triangle[] upper_hull;
        private readonly Triangle[] lower_hull;
        private readonly Vector3[] intersection_pt;

        // our counters. We use raw arrays for performance reasons
        private int upper_hull_count;
        private int lower_hull_count;
        private int intersection_pt_count;

        public IntersectionResult() {
            this.is_success = false;

            this.upper_hull = new Triangle[2];
            this.lower_hull = new Triangle[2];
            this.intersection_pt = new Vector3[2];

            this.upper_hull_count = 0;
            this.lower_hull_count = 0;
            this.intersection_pt_count = 0;
        }

        public Triangle[] upperHull {
            get { return upper_hull; }
        }

        public Triangle[] lowerHull {
            get { return lower_hull; }
        }

        public Vector3[] intersectionPoints {
            get { return intersection_pt; }
        }

        public int upperHullCount {
            get { return upper_hull_count; }
        }

        public int lowerHullCount {
            get { return lower_hull_count; }
        }

        public int intersectionPointCount {
            get { return intersection_pt_count; }
        }

        public bool isValid {
            get { return is_success; }
        }

        /**
         * Used by the intersector, adds a new triangle to the
         * upper hull section
         */
        public IntersectionResult AddUpperHull(Triangle tri) {
            upper_hull[upper_hull_count++] = tri;

            is_success = true;

            return this;
        }

        /**
         * Used by the intersector, adds a new triangle to the
         * lower gull section
         */
        public IntersectionResult AddLowerHull(Triangle tri) {
            lower_hull[lower_hull_count++] = tri;

            is_success = true;

            return this;
        }

        /**
         * Used by the intersector, adds a new intersection point
         * which is shared by both upper->lower hulls
         */
        public void AddIntersectionPoint(Vector3 pt) {
            intersection_pt[intersection_pt_count++] = pt;
        }

        /**
         * Clear the current state of this object 
         */
        public void Clear() {
            is_success = false;
            upper_hull_count = 0;
            lower_hull_count = 0;
            intersection_pt_count = 0;
        }

        /**
         * Editor only DEBUG functionality. This should not be compiled in the final
         * Version.
         */
        public void OnDebugDraw() {
            OnDebugDraw(Color.white);
        }

        public void OnDebugDraw(Color drawColor) {
#if UNITY_EDITOR

            if (!isValid) {
                return;
            }

            Color prevColor = Gizmos.color;

            Gizmos.color = drawColor;

            // draw the intersection points
            for (int i = 0; i < intersectionPointCount; i++) {
                Gizmos.DrawSphere(intersectionPoints[i], 0.1f);
            }

            // draw the upper hull in RED
            for (int i = 0; i < upperHullCount; i++) {
                upperHull[i].OnDebugDraw(Color.red);
            }

            // draw the lower hull in BLUE
            for (int i = 0; i < lowerHullCount; i++) {
                lowerHull[i].OnDebugDraw(Color.blue);
            }

            Gizmos.color = prevColor;

#endif
        }
    }
}