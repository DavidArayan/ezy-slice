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
	public struct IntersectionResult {

		// general tag to check if this structure is valid
		private bool is_success;

		// our intersection points/triangles
		private Triangle[] upper_hull;
		private Triangle[] lower_hull;
		private Vector3[] intersection_pt;

		// our counters. We use raw arrays for performance reasons
		private int upper_hull_count;
		private int lower_hull_count;
		private int intersection_pt_count;

		public IntersectionResult(int maxHulls = 2) {
			this.is_success = false;

			this.upper_hull = new Triangle[maxHulls];
			this.lower_hull = new Triangle[maxHulls];
			this.intersection_pt = new Vector3[maxHulls];

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

		public int upperHullCount {
			get { return upper_hull_count; }
		}

		public int lowerHullCount {
			get { return lower_hull_count; }
		}

		public bool isValid {
			get { return is_success; }
		}

		/**
		 * Used by the intersector, adds a new triangle to the
		 * upper hull section
		 */
		public void AddUpperHull(Triangle tri) {
			upper_hull[upper_hull_count++] = tri;

			is_success = true;
		}

		/**
		 * Used by the intersector, adds a new triangle to the
		 * lower gull section
		 */
		public void AddLowerHull(Triangle tri) {
			lower_hull[lower_hull_count++] = tri;

			is_success = true;
		}

		/**
		 * Used by the intersector, adds a new intersection point
		 * which is shared by both upper->lower hulls
		 */
		public void AddIntersectionPoint(Vector3 pt) {
			intersection_pt[intersection_pt_count++] = pt;
		}

		public void Clear() {
			is_success = false;
			upper_hull_count = 0;
			lower_hull_count = 0;
			intersection_pt_count = 0;
		}
	}
}