using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EzySlice {
	/**
	 * Represents a simple 3D Triangle structure with position
	 * and UV map. The UV is required if the slicer needs
	 * to recalculate the new UV position for texture mapping.
	 */
	public struct Triangle {
		// the points which represent this triangle
		private readonly Vector3 m_pos_a;
		private readonly Vector3 m_pos_b;
		private readonly Vector3 m_pos_c;

		// the UV coordinates of this triangle
		private readonly Vector2 m_uv_a;
		private readonly Vector2 m_uv_b;
		private readonly Vector2 m_uv_c;

		public Triangle(Vector3 posa, 
						Vector3 posb, 
						Vector3 posc,
						Vector2 uva, 
						Vector2 uvb, 
						Vector2 uvc) 
		{
			this.m_pos_a = posa;
			this.m_pos_b = posb;
			this.m_pos_c = posc;

			this.m_uv_a = uva;
			this.m_uv_b = uvb;
			this.m_uv_c = uvc;
		}

		public Vector3 positionA {
			get { return this.m_pos_a; }
		}

		public Vector3 positionB {
			get { return this.m_pos_b; }
		}

		public Vector3 positionC {
			get { return this.m_pos_c; }
		}

		public Vector2 uvA {
			get { return this.m_uv_a; }
		}

		public Vector2 uvB {
			get { return this.m_uv_b; }
		}

		public Vector2 uvC {
			get { return this.m_uv_c; }
		}

		/**
		 * Calculate the Barycentric coordinate weight values u-v-w for Point p in respect to the provided
		 * triangle. This is useful for computing new UV coordinates for arbitrary points.
		 */
		public Vector3 Barycentric(Vector3 p) {
			Vector3 a = m_pos_a;
			Vector3 b = m_pos_b;
			Vector3 c = m_pos_c;

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
				nu = Intersector.TriArea2D(p.y, p.z, b.y, b.z, c.y, c.z);
				// area of PCA in yz plane
				nv = Intersector.TriArea2D(p.y, p.z, c.y, c.z, a.y, a.z);
				// 1/2*area of ABC in yz plane
				ood = 1.0f / m.x;
			} 
			else if (y >= x && y >= z) {
				// project in xz plane
				nu = Intersector.TriArea2D(p.x, p.z, b.x, b.z, c.x, c.z);
				nv = Intersector.TriArea2D(p.x, p.z, c.x, c.z, a.x, a.z);
				ood = 1.0f / -m.y;
			} 
			else {
				// project in xy plane
				nu = Intersector.TriArea2D(p.x, p.y, b.x, b.y, c.x, c.y);
				nv = Intersector.TriArea2D(p.x, p.y, c.x, c.y, a.x, a.y);
				ood = 1.0f / m.z;
			}

			float u = nu * ood;
			float v = nv * ood;
			float w = 1.0f - u - v;

			return new Vector3(u, v, w);
		}

		/**
		 * Generate a set of new UV coordinates for the provided point pt in respect to Triangle tri.
		 * This can be useful for tessalation/simplification algorithms which need to keep the texture
		 * map the same.
		 */
		public Vector2 GenerateUVCoords(Vector3 pt) {
			Vector3 weights = Barycentric(pt);

			return (weights.x * m_uv_a) + (weights.y * m_uv_b) + (weights.z * m_uv_c);
		}
	}
}