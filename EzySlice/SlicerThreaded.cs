using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EzySlice {

    /**
     * Designed around the same function prototypes as the Slicer, except this class uses
     * Threading internally to speedup the slicing process of meshes.
     * 
     * NOTE -> This is experimental and some performance benchmarks need to be done and
     * optimizations put into place.
     * 
     * For Simplicity purposes, some functions are copied across. This creates code duplication
     * which will get merged eventually.
     */
    public sealed class SlicerThreaded {
        
        /**
         * An internal class for storing internal submesh values
         */
        internal class SlicedSubmesh {
            public readonly List<Triangle> upperHull = new List<Triangle>();
            public readonly List<Triangle> lowerHull = new List<Triangle>();
        }

        public static GameObject[] SliceInstantiate(GameObject obj, Plane pl, bool genCrossSection = true) {
            SlicedHull slice = Slice(obj, pl, genCrossSection);

            if (slice == null) {
                return null;
            }

            GameObject upperHull = slice.CreateUpperHull();

            if (upperHull != null) {
                // set the positional information
                upperHull.transform.position = obj.transform.position;
                upperHull.transform.rotation = obj.transform.rotation;
                upperHull.transform.localScale = obj.transform.localScale;

                // the the material information
                upperHull.GetComponent<Renderer>().sharedMaterials = obj.GetComponent<MeshRenderer>().sharedMaterials;
            }

            GameObject lowerHull = slice.CreateLowerHull();

            if (lowerHull != null) {
                // set the positional information
                lowerHull.transform.position = obj.transform.position;
                lowerHull.transform.rotation = obj.transform.rotation;
                lowerHull.transform.localScale = obj.transform.localScale;

                // the the material information
                lowerHull.GetComponent<Renderer>().sharedMaterials = obj.GetComponent<MeshRenderer>().sharedMaterials;
            }

            // return both if upper and lower hulls were generated
            if (upperHull != null && lowerHull != null) {
                return new GameObject[] { upperHull, lowerHull };
            }

            // otherwise return only the upper hull
            if (upperHull != null) {
                return new GameObject[] { upperHull };
            }

            // otherwise return null
            return null;
        }

        public static SlicedHull Slice(GameObject obj, Plane pl, bool genCrossSection = true) {
            MeshFilter renderer = obj.GetComponent<MeshFilter>();

            if (renderer == null) {
                return null;
            }

            return Slice(renderer.sharedMesh, pl, genCrossSection);
        }

        public static SlicedHull Slice(Mesh sharedMesh, Plane pl, bool genCrossSection = true) {
            // TO DO Impl
            return null;
        }
    }
}
