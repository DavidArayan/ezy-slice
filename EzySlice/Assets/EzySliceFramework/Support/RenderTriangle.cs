using UnityEngine;
using System.Collections.Generic;
using EzySlice;

public class RenderTriangle {

    private Color randClor = new Color(Random.value, Random.value, Random.value);

    private Vector3 mpta = Vector3.zero;
    private Vector3 mptb = Vector3.zero;
    private Vector3 mptc = Vector3.zero;

    private Vector2 muva = Vector2.zero;
    private Vector2 muvb = Vector2.zero;
    private Vector2 muvc = Vector2.zero;

    private List<RenderTriangle> splits = new List<RenderTriangle>();

    public RenderTriangle SetPoints(Vector3 pta, Vector3 ptb, Vector3 ptc) {
        this.mpta = new Vector3(pta.x, pta.y, pta.z);
        this.mptb = new Vector3(ptb.x, ptb.y, ptb.z);
        this.mptc = new Vector3(ptc.x, ptc.y, ptc.z);

        return this;
    }

    public RenderTriangle SetUV(Vector2 uva, Vector2 uvb, Vector2 uvc) {
        this.muva = new Vector2(uva.x, uva.y);
        this.muvb = new Vector2(uvb.x, uvb.y);
        this.muvc = new Vector2(uvc.x, uvc.y);

        return this;
    }

    public Vector3 PointA { get { return this.mpta; } }
    public Vector3 PointB { get { return this.mptb; } }
    public Vector3 PointC { get { return this.mptc; } }

    public Vector2 UvA { get { return this.muva; } }
    public Vector2 UvB { get { return this.muvb; } }
    public Vector2 UvC { get { return this.muvc; } }

    public void RandomSplit() {
        if (splits.Count == 0) {
            // split this object
            Split();
        }
        else {
            // otherwise recurse and split roots
            foreach (RenderTriangle tri in splits) {
                tri.RandomSplit();
            }
        }
    }

    private static Vector3 RandomVector3() {
        float phi = Mathf.PI * 2 * Random.value;
        float theta = Mathf.PI * Random.value;

        float x = Mathf.Cos(phi) * Mathf.Sin(theta);
        float y = Mathf.Cos(theta);
        float z = Mathf.Sin(phi) * Mathf.Sin(theta);
        
        return new Vector3(x, y, z);
    }

    private void Split() {
        NDPlane plane = new NDPlane();

        plane.ComputePlane((mpta + mptb + mptc) / 3, RandomVector3());

        List<Vector3> upper = new List<Vector3>();
        List<Vector3> lower = new List<Vector3>();
        List<Vector3> intersection = new List<Vector3>();

        plane.IntersectTriangleHull(mpta, mptb, mptc, upper, lower, intersection);

        upper.AddRange(intersection);
        lower.AddRange(intersection);

        List<int> indices = new List<int>();

        // UPPER HULL
        Triangulator.TriangulateNDSlice(upper, indices);

        for (int i = 0; i < indices.Count; i += 3) {
            RenderTriangle tri = new RenderTriangle();

            Vector3 pta = upper[indices[i]];
            Vector3 ptb = upper[indices[i+1]];
            Vector3 ptc = upper[indices[i+2]];

            Vector2 uva = Triangulator.GenerateUVCoords(ref mpta, ref mptb, ref mptc, ref muva, ref muvb, ref muvc, ref pta);
            Vector2 uvb = Triangulator.GenerateUVCoords(ref mpta, ref mptb, ref mptc, ref muva, ref muvb, ref muvc, ref ptb);
            Vector2 uvc = Triangulator.GenerateUVCoords(ref mpta, ref mptb, ref mptc, ref muva, ref muvb, ref muvc, ref ptc);

            tri.SetPoints(pta, ptb, ptc);
            tri.SetUV(uva, uvb, uvc);

            splits.Add(tri);
        }

        indices.Clear();

        // LOWER HULL
        Triangulator.TriangulateNDSlice(lower, indices);

        for (int i = 0; i < indices.Count; i += 3) {
            RenderTriangle tri = new RenderTriangle();

            Vector3 pta = lower[indices[i]];
            Vector3 ptb = lower[indices[i + 1]];
            Vector3 ptc = lower[indices[i + 2]];

            Vector2 uva = Triangulator.GenerateUVCoords(ref mpta, ref mptb, ref mptc, ref muva, ref muvb, ref muvc, ref pta);
            Vector2 uvb = Triangulator.GenerateUVCoords(ref mpta, ref mptb, ref mptc, ref muva, ref muvb, ref muvc, ref ptb);
            Vector2 uvc = Triangulator.GenerateUVCoords(ref mpta, ref mptb, ref mptc, ref muva, ref muvb, ref muvc, ref ptc);

            tri.SetPoints(pta, ptb, ptc);
            tri.SetUV(uva, uvb, uvc);

            splits.Add(tri);
        }
    }

    public List<RenderTriangle> GetRootSplits(List<RenderTriangle> tris) {
        if (tris == null) {
            tris = new List<RenderTriangle>();
        }

        if (splits.Count == 0) {
            tris.Add(this);
        }
        else {
            foreach (RenderTriangle tri in splits) {
                tri.GetRootSplits(tris);
            }
        }

        return tris;
    }

    public void DebugDraw() {
        Gizmos.color = randClor;

        Gizmos.DrawLine(mpta, mptb);
        Gizmos.DrawLine(mptb, mptc);
        Gizmos.DrawLine(mptc, mpta);
    }
}
