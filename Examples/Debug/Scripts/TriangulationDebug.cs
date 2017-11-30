using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

/**
 * For debugging purposes only. 
 */
public class TriangulationDebug : MonoBehaviour {

	public GameObject[] points;

	void OnDrawGizmos() {
		if (points == null || points.Length < 3) {
			return;
		}

		List<Vector3> pt = new List<Vector3>();

		for (int i = 0; i < points.Length; i++) {
			if (points[i] == null) {
				continue;
			}

			pt.Add(points[i].transform.position);

			Gizmos.DrawSphere(points[i].transform.position, 0.1f);
		}

		if (pt.Count < 3) {
			return;
		}

		Vector3[] verts;
		Vector2[] uvs;
		int[] indices;

		// perform triangulation
		if (Triangulator.MonotoneChain(pt, Vector3.up, out verts, out indices, out uvs)) {
			
			for (int i = 0; i < indices.Length; i+=3) {
				Triangle newTri = new Triangle(verts[indices[i]], verts[indices[i+1]], verts[indices[i+2]]);

				newTri.OnDebugDraw(Color.yellow);
			}
		}
	}
}
