using UnityEngine;
using System.Collections;

public class RenderTriTest : MonoBehaviour {

    private RenderPlane plane;

    public int splits = 0;

	// Use this for initialization
	void Start () {
        plane = new RenderPlane();

        plane.GenerateUnitQuad();

        for (int i = 0; i < splits; i++) {
            plane.Split();
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnDrawGizmos() {
        if (plane == null) {
            return;
        }

        plane.DebugDraw();
    }
}
