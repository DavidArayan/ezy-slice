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
	}
}