using UnityEngine;
using System.Collections;

public class SlicerTest : MonoBehaviour {

    public EzySlice.NDPlaneInstance instance;
    public GameObject obj;

	// Use this for initialization
	void Start () {
        instance.CutObject(obj, true);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
