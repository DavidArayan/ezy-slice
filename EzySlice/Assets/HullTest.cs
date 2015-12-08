using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EzySlice;

public class HullTest : MonoBehaviour {

    public List<GameObject> pts = new List<GameObject>();

    public bool drawPoints = true;
    public bool drawTriangles = true;
    public bool run = false;

    void OnDrawGizmos() {
        if (!run) {
            return;
        }

        if (pts == null || pts.Count == 0) {
            return;
        }

        List<Vector3> points = new List<Vector3>();
        List<int> indices = new List<int>();
        List<Vector2> uv = new List<Vector2>();
        List<Vector3> pointsOut = new List<Vector3>();
        List<Vector3> normalOut = new List<Vector3>();

        foreach (GameObject obj in pts) {
            if (obj == null) {
                continue;
            }

            points.Add(obj.transform.position);
        }

        if (drawPoints) {
            Gizmos.color = Color.yellow;

            foreach (Vector3 point in points) {
                Gizmos.DrawWireCube(point, Vector3.one);
            }
        }

        Triangulator.TriangulateHullPt(points, pointsOut, indices, uv, normalOut);

        if (drawPoints) {
            Gizmos.color = Color.blue;

            foreach (Vector3 point in pointsOut) {
                Gizmos.DrawWireCube(point, Vector3.one / 2.0f);
            }
        }

        if (drawTriangles) {
            Gizmos.color = Color.green;

            for (int i = 0; i < indices.Count; i += 3) {
                Gizmos.DrawLine(pointsOut[indices[i]], pointsOut[indices[i + 1]]);
                Gizmos.DrawLine(pointsOut[indices[i + 1]], pointsOut[indices[i + 2]]);
                Gizmos.DrawLine(pointsOut[indices[i + 2]], pointsOut[indices[i]]);
            }
        }
    }
}
