using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EzySlice {
    [CustomEditor(typeof(NDPlaneInstance))]
    public class NDPlaneInstanceEditor : Editor {

        public GameObject source;

        public override void OnInspectorGUI() {
            NDPlaneInstance plane = (NDPlaneInstance)target;

            source = (GameObject) EditorGUILayout.ObjectField(source, typeof(GameObject), true);

            if (plane.PreviousCuts != null && plane.PreviousCuts.Count > 0) {
                foreach (GameObject obj in plane.PreviousCuts) {
                    EditorGUILayout.ObjectField(obj, typeof(GameObject), false);
                }

                if (GUILayout.Button("Clear Previous Cuts")) {
                    plane.DestroyPreviousCuts();

                    if (source != null) {
                        source.SetActive(true);
                    }
                }
            }

            if (source == null) {
                EditorGUILayout.LabelField("Add a GameObject to Cut.");

                return;
            }

            if (source.GetComponent<MeshFilter>() == null) {
                EditorGUILayout.LabelField("GameObject must have a MeshFilter.");

                return;
            }

            if (plane.PreviousCuts == null || plane.PreviousCuts.Count == 0) {
                if (GUILayout.Button("Cut Object")) {
                    plane.CutObject(source);

                    source.SetActive(false);
                }
            }
        }
    }
}
