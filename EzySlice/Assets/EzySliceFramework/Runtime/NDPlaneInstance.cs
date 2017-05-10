using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EzySlice {

    /**
     * Simple runtime utility class with an attached Editor GUI for testing mesh cuts
     * in the editor. This is simply meant to demo how to use the cutting classes at
     * runtime.
     */
    public class NDPlaneInstance : MonoBehaviour {

        private NDPlane plane = new NDPlane();
        private List<GameObject> prevCuts;

        public List<GameObject> CutObject(GameObject obj, bool destroyPrevious = false) {
            // represent the NDPlane in obj's reference frame
            Vector3 refUp = obj.transform.InverseTransformDirection(transform.up);
            Vector3 refPt = obj.transform.InverseTransformPoint(transform.position);

            plane.ComputePlane(refPt, refUp);

            prevCuts = MeshSlicer.CutObjectInstantiate(obj, plane, destroyPrevious);

            return prevCuts;
        }

        public List<GameObject> PreviousCuts {
            get { return prevCuts; }
        }

        /**
         * Destroy the previously generated Cuts (if any)
         */
        public void DestroyPreviousCuts() {
            if (prevCuts == null || prevCuts.Count == 0) {
                return;
            }

            foreach (GameObject obj in prevCuts) {
                if (obj != null) {
                    DestroyImmediate(obj);
                }
            }

            prevCuts = null;
        }
    }
}
