using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EzySlice {
	public struct Line {
		private readonly Vector3 m_pos_a;
		private readonly Vector3 m_pos_b;

		public Line(Vector3 pta, Vector3 ptb) {
			this.m_pos_a = pta;
			this.m_pos_b = ptb;
		}

		public float dist {
			get { return Vector3.Distance (this.m_pos_a, this.m_pos_b); }
		}

		public float distSq {
			get { return (this.m_pos_a - this.m_pos_b).sqrMagnitude; }
		}

		public Vector3 positionA {
			get { return this.m_pos_a; }
		}

		public Vector3 positionB {
			get { return this.m_pos_b; }
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

            Color prevColor = Gizmos.color;

            Gizmos.color = drawColor;

            Gizmos.DrawLine(positionA, positionB);

            Gizmos.color = prevColor;

            #endif
        }
	}
}