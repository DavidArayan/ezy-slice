using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

/**
 * For debugging purposes ONLY.  
 */
public class IntersectionDebug : MonoBehaviour {

	public GameObject triPisitionA;
	public GameObject triPositionB;
	public GameObject triPositionC;
	public GameObject plane;

	void OnDrawGizmos() {
		if (triPisitionA == null || triPositionB == null || triPositionC == null || plane == null) {
			return;
		}

		Triangle newTri = new Triangle(triPisitionA.transform.position, triPositionB.transform.position, triPositionC.transform.position);
		EzySlice.Plane newPlane = new EzySlice.Plane();
		newPlane.Compute(plane);

		newTri.OnDebugDraw(Color.yellow);
		newPlane.OnDebugDraw(Color.yellow);

		IntersectionResult newResult = new IntersectionResult();

		bool result = newTri.Split(newPlane, newResult);

		if (result) {
			newResult.OnDebugDraw(Color.green);	
		}
	}
}
