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

		if (GUILayout.Button("Cut Object")) {
			SlicedHull hull = plane.SliceObject(source);

			if (hull != null) {
				hull.CreateLowerHull(source);
				hull.CreateUpperHull(source);

				source.SetActive(false);
			}
		}
	}
}
