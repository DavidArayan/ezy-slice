#### Open Source Slicer Framework for the Unity3D Game Engine

* Ability to slice any convex/concave Mesh using a Plane
* Generate cross section for the sliced Mesh with UV Coordinates (Convex only)
* Re-Generates UV Coordinates for sliced Meshes for seamless blending between the original and sliced object
* Recently updated for Unity3D 2017.2
* MIT Open Source License
* No external plugin dependencies, fully written in C#
* Contributions are always welcomed and appreciated!

#### Algorithms in use

* General purpose Monotone Chain for cross section triangulation for Convex slices
* Barycentric Coordinates for re-generating new UV coordinates
* Purpose built Triangle to Plane intersection to cover all general cases for slicing
* Designed with performance in mind

#### The future pipeline

* Add multi-threading support
* Add ability to create cross sections for Concave object slices
* Expand the api to provide greater flexibility and utility
* Integrate with the Unity3D Editor
* Add more examples and scenes
