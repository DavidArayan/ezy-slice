using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EzySlice {

	/**
	 * The final generated data structure from a slice operation. This provides easy access
	 * to utility functions and the final Mesh data for each section of the HULL.
	 */
	public sealed class SlicedHull {
		private Mesh upper_hull;
		private Mesh lower_hull;
		private Mesh upper_cross_section;
		private Mesh lower_cross_section;

		public SlicedHull(Mesh upperHull, Mesh lowerHull) : this(upperHull, lowerHull, null, null) {}

		public SlicedHull(Mesh upperHull, Mesh lowerHull, Mesh upperCrossSection, Mesh lowerCrossSection) {
			this.upper_hull = upperHull;
			this.lower_hull = lowerHull;
			this.upper_cross_section = upperCrossSection;
			this.lower_cross_section = lowerCrossSection;
		}

		public GameObject CreateUpperHull(GameObject original) {
			GameObject newObject = CreateUpperHull();

			if (newObject != null) {
				newObject.transform.position = original.transform.position;
				newObject.transform.rotation = original.transform.rotation;
				newObject.transform.localScale = original.transform.localScale;

				// the the material information
				newObject.GetComponent<Renderer>().sharedMaterials = original.GetComponent<MeshRenderer>().sharedMaterials;
			}

			return newObject;
		}

		/**
		 * Generate a new GameObject from the upper hull of the mesh
		 * This function will return null if upper hull does not exist
		 */
		public GameObject CreateUpperHull() {
			GameObject newObject = CreateEmptyObject("Upper_Hull", upper_hull);

			if (newObject != null) {
				GameObject crossSection = CreateEmptyObject("Cross_Section", upper_cross_section);

				if (crossSection != null) {
					crossSection.transform.parent = newObject.transform;
				}
			}

			return newObject;
		}

		public GameObject CreateLowerHull(GameObject original) {
			GameObject newObject = CreateLowerHull();

			if (newObject != null) {
				newObject.transform.position = original.transform.position;
				newObject.transform.rotation = original.transform.rotation;
				newObject.transform.localScale = original.transform.localScale;

				// the the material information
				newObject.GetComponent<Renderer>().sharedMaterials = original.GetComponent<MeshRenderer>().sharedMaterials;
			}

			return newObject;
		}

		/**
		 * Generate a new GameObject from the Lower hull of the mesh
		 * This function will return null if lower hull does not exist
		 */
		public GameObject CreateLowerHull() {
			GameObject newObject = CreateEmptyObject("Lower_Hull", lower_hull);

			if (newObject != null) {
				GameObject crossSection = CreateEmptyObject("Cross_Section", lower_cross_section);

				if (crossSection != null) {
					crossSection.transform.parent = newObject.transform;
				}
			}

			return newObject;
		}

		public Mesh upperHull {
			get { return this.upper_hull; }
		}

		public Mesh lowerHull {
			get { return this.lower_hull; }
		}

		public Mesh upperHullCrossSection {
			get { return this.upper_cross_section; }
		}

		public Mesh lowerHullCrossSection {
			get { return this.lower_cross_section; }
		}

		/**
		 * Helper function which will create a new GameObject to be able to add
		 * a new mesh for rendering and return.
		 */
		private static GameObject CreateEmptyObject(string name, Mesh hull) {
			if (hull == null) {
				return null;
			}

			GameObject newObject = new GameObject(name);

			newObject.AddComponent<MeshRenderer>();
			MeshFilter filter = newObject.AddComponent<MeshFilter>();

			filter.mesh = hull;

			return newObject;
		}
	}
}