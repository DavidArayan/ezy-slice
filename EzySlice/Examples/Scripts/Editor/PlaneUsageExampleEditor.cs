using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EzySlice;

/**
 * This is a simple Editor helper script for rapid testing/prototyping! 
 */
[CustomEditor(typeof(PlaneUsageExample))]
public class PlaneUsageExampleEditor : Editor {
	public GameObject source;
	public Material crossMat;
	public bool recursiveSlice;

	public override void OnInspectorGUI() {
		PlaneUsageExample plane = (PlaneUsageExample)target;

		source = (GameObject) EditorGUILayout.ObjectField(source, typeof(GameObject), true);

		if (source == null) {
			EditorGUILayout.LabelField("Add a GameObject to Slice.");

			return;
		}

		if (!source.activeInHierarchy) {
			EditorGUILayout.LabelField("Object is Hidden. Cannot Slice.");

			return;
		}

		if (source.GetComponent<MeshFilter>() == null) {
			EditorGUILayout.LabelField("GameObject must have a MeshFilter.");

			return;
		}

		crossMat = (Material) EditorGUILayout.ObjectField(crossMat, typeof(Material), true);
		recursiveSlice = (bool) EditorGUILayout.Toggle("Recursive Slice", recursiveSlice);

		if (GUILayout.Button("Cut Object")) {
			// only slice the parent object
			if (!recursiveSlice) {
				SlicedHull hull = plane.SliceObject(source);

				if (hull != null) {
					hull.CreateLowerHull(source, crossMat);
					hull.CreateUpperHull(source, crossMat);

					source.SetActive(false);
				}
			}
			else {
				// in here we slice both the parent and all child objects
				SliceObjectRecursive(plane, source);

				source.SetActive(false);
			}
		}
	}

	/**
	 * This function will recursively slice the provided object and all it's children.
	 * Returns a list of SlicedHull objects which represents the cuts for the object
	 * and all its children (if any)
	 */
	public GameObject[] SliceObjectRecursive(PlaneUsageExample plane, GameObject obj) {

		// finally slice the requested object and return
		SlicedHull finalHull = plane.SliceObject(obj);

		if (finalHull != null) {
			GameObject lowerParent = finalHull.CreateLowerHull(obj, crossMat);
			GameObject upperParent = finalHull.CreateUpperHull(obj, crossMat);

			if (obj.transform.childCount > 0) {
				foreach (Transform child in obj.transform) {
					if (child != null && child.gameObject != null) {

						// if the child has chilren, we need to recurse deeper
						if (child.childCount > 0) {
							GameObject[] children = SliceObjectRecursive(plane, child.gameObject);

							if (children != null) {
								// add the lower hull of the child if available
								if (children[0] != null && lowerParent != null) {
									children[0].transform.SetParent(lowerParent.transform, false);
								}

								// add the upper hull of this child if available
								if (children[1] != null && upperParent != null) {
									children[1].transform.SetParent(upperParent.transform, false);
								}
							}
						}
						else {
							// otherwise, just slice the child object
							SlicedHull hull = plane.SliceObject(child.gameObject);

							if (hull != null) {
								GameObject childLowerHull = hull.CreateLowerHull(child.gameObject, crossMat);
								GameObject childUpperHull = hull.CreateUpperHull(child.gameObject, crossMat);

								// add the lower hull of the child if available
								if (childLowerHull != null && lowerParent != null) {
									childLowerHull.transform.SetParent(lowerParent.transform, false);
								}

								// add the upper hull of the child if available
								if (childUpperHull != null && upperParent != null) {
									childUpperHull.transform.SetParent(upperParent.transform, false);
								}
							}
						}
					}
				}
			}

			return new GameObject[] {lowerParent, upperParent};
		}

		return null;
	}
}
