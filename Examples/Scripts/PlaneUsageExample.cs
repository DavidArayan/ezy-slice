using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

/**
 * This class is an example of how to setup a cutting Plane from a GameObject
 * and how to work with coordinate systems.
 * 
 * When a Place slices a Mesh, the Mesh is in local coordinates whilst the Plane
 * is in world coordinates. The first step is to bring the Plane into the coordinate system
 * of the mesh we want to slice. This script shows how to do that.
 */
public class PlaneUsageExample : MonoBehaviour {

	/**
	 * This function will slice the provided object by the plane defined in this
	 * GameObject. We use the GameObject this script is attached to define the position
	 * and direction of our cutting Plane. Results are then returned to the user.
	 */
	public SlicedHull SliceObject(GameObject obj) {
		// ensure to generate an EzySlice version of the Plane instead of the 
		// default Unity.
		EzySlice.Plane cuttingPlane = new EzySlice.Plane();

		// since this GameObject represents our Plane's coordinates, we first need
		// to bring the Plane into the coordinate frame of the object we want to slice
		// this is because the Mesh data is always in local coordinates
		// we need the position of the plane and direction
		Vector3 refUp = obj.transform.InverseTransformDirection(transform.up);
		Vector3 refPt = obj.transform.InverseTransformPoint(transform.position);

		// once we have the coordinates we need, we can initialize our plane with the new
		// coordinates (now in obj's coordinate frame) and safely perform the slice
		// operation
		cuttingPlane.Compute(refPt, refUp);

		// finally, slice the object and return the results. SlicedHull will have all the mesh
		// details which the application can use to do whatever it wants to do
		return Slicer.Slice(obj, cuttingPlane);
	}

	#if UNITY_EDITOR
	/**
	 * This is for Visual debugging purposes in the editor 
	 */
	public void OnDrawGizmos() {
		EzySlice.Plane cuttingPlane = new EzySlice.Plane();

		// the plane will be set to the same coordinates as the object that this
		// script is attached to
		// NOTE -> Debug Gizmo drawing only works if we pass the transform
		cuttingPlane.Compute(transform);

		// draw gizmos for the plane
		// NOTE -> Debug Gizmo drawing is ONLY available in editor mode. Do NOT try
		// to run this in the final build or you'll get crashes (most likey)
		cuttingPlane.OnDebugDraw();
	}

	#endif
}
