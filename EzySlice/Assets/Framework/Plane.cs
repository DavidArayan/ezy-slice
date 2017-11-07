using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EzySlice {
	public enum SideOfPlane {
		UP,
		DOWN,
		ON
	}
	/**
	 * Represents a simple 3D Plane structure with a position
	 * and direction which extends infinitely in its axis. This provides
	 * an optimal structure for collision tests for the slicing framework.
	 */
	public struct Plane {
		private readonly Vector3 m_normal;
		private readonly float m_dist;

		public Plane(Vector3 pos, Vector3 norm) {
			this.m_normal = norm;
			this.m_dist = Vector3.Dot(norm, pos);
		}

		public Plane(Vector3 norm, float dot) {
			this.m_normal = norm;
			this.m_dist = dot;
		}

		public Vector3 normal {
			get { return this.m_normal; }
		}

		public float dist {
			get { return this.m_dist; }
		}

		/**
		 * Checks which side of the plane the point lays on.
		 */
		public SideOfPlane SideOf(Vector3 pt) {
			float result = Vector3.Dot(m_normal, pt);

			if (result > m_dist) {
				return SideOfPlane.UP;
			}

			if (result < m_dist) {
				return SideOfPlane.DOWN;
			}

			return SideOfPlane.ON;
		}
	}
}