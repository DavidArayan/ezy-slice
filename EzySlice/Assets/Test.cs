using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EzySlice;

public class Test : MonoBehaviour {

    public GameObject pt1;
    public GameObject pt2;
    public GameObject pt3;

    public GameObject nd;

    public bool drawPoints = true;
    public bool drawTriangles = true;

    private NDPlane plane = new NDPlane();

    void OnDrawGizmos() {
        if (nd == null) {
            return;
        }

        plane.ComputePlane(nd.transform.position, nd.transform.forward);

        Vector3 nSize = Vector3.one / 2.0f;

        List<Vector3> upper = new List<Vector3>();
        List<Vector3> lower = new List<Vector3>();
        List<Vector3> intersection = new List<Vector3>();

        plane.IntersectTriangleHull(pt1.transform.position, pt2.transform.position, pt3.transform.position, upper, lower, intersection);

        if (drawPoints) {
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireCube(pt1.transform.position, Vector3.one);
            Gizmos.DrawWireCube(pt2.transform.position, Vector3.one);
            Gizmos.DrawWireCube(pt3.transform.position, Vector3.one);

            Gizmos.color = Color.green;

            foreach (Vector3 v in upper) {
                Gizmos.DrawCube(v, nSize);
            }

            Gizmos.color = Color.blue;

            foreach (Vector3 v in lower) {
                Gizmos.DrawCube(v, nSize);
            }

            Gizmos.color = Color.red;

            foreach (Vector3 v in intersection) {
                Gizmos.DrawCube(v, nSize);
            }
        }

        if (drawTriangles) {
            upper.AddRange(intersection);
            lower.AddRange(intersection);

            List<int> indices = new List<int>();

            Triangulator.TriangulateNDSlice(upper, indices);

            Gizmos.color = Color.green;

            for (int i = 0; i < indices.Count; i+=3) {
                Gizmos.DrawLine(upper[indices[i]],      upper[indices[i + 1]]);
                Gizmos.DrawLine(upper[indices[i + 1]],  upper[indices[i + 2]]);
                Gizmos.DrawLine(upper[indices[i + 2]],  upper[indices[i]]);
            }

            Gizmos.color = Color.blue;
            indices.Clear();

            Triangulator.TriangulateNDSlice(lower, indices);

            for (int i = 0; i < indices.Count; i += 3) {
                Gizmos.DrawLine(lower[indices[i]],      lower[indices[i + 1]]);
                Gizmos.DrawLine(lower[indices[i + 1]],  lower[indices[i + 2]]);
                Gizmos.DrawLine(lower[indices[i + 2]],  lower[indices[i]]);
            }
        }
    }
}
