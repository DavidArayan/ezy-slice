using UnityEngine;
using System.Collections;

namespace EzySlice {
    public class NDPlaneInstance : MonoBehaviour {

        private NDPlane plane = new NDPlane();

        public void CutObject(GameObject obj, bool destroyPrevious = false) {
            plane.ComputePlane(transform.position, transform.up);

            MeshSlicer.CutObjectInstantiate(obj, plane, destroyPrevious);
        }
    }
}
