using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RenderPlane {
    private List<RenderTriangle> tris = new List<RenderTriangle>();

    public void AddTriangle(RenderTriangle tri) {
        this.tris.Add(tri);
    }

    public void GenerateUnitQuad() {
        Vector3[] vertices = new Vector3[]
         {
             new Vector3( 1, 0,  1),
             new Vector3( 1, 0, -1),
             new Vector3(-1, 0,  1),
             new Vector3(-1, 0, -1),
         };

        Vector2[] uv = new Vector2[]
        {
             new Vector2(1, 1),
             new Vector2(1, 0),
             new Vector2(0, 1),
             new Vector2(0, 0),
        };

        int[] triangles = new int[]
        {
             0, 1, 2,
             2, 1, 3,
        };

        RenderTriangle tri1 = new RenderTriangle();

        tri1.SetPoints(vertices[triangles[0]], vertices[triangles[1]], vertices[triangles[2]]);
        tri1.SetUV(uv[triangles[0]], uv[triangles[1]], uv[triangles[2]]);

        RenderTriangle tri2 = new RenderTriangle();

        tri2.SetPoints(vertices[triangles[3]], vertices[triangles[4]], vertices[triangles[5]]);
        tri2.SetUV(uv[triangles[3]], uv[triangles[4]], uv[triangles[5]]);

        AddTriangle(tri1);
        AddTriangle(tri2);
    }

    public void Split() {
        foreach (RenderTriangle tri in tris) {
            tri.RandomSplit();
        }
    }

    public void DebugDraw() {
        foreach (RenderTriangle tri in tris) {
            List<RenderTriangle> roots = tri.GetRootSplits(null);

            foreach (RenderTriangle root in roots) {
                root.DebugDraw();
            }
        }
    }
}
