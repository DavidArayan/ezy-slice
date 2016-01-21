### EzySlice
An open source mesh slicer framework for Unity3D Game Engine.

The framework does not rely on 3rd party plugins and will run on all platforms supported by Unity3D.

Supports Slicing any 3D Mesh using a Plane. Works on Textures Meshes.

Convex 3D Mesh Supports Retriangulation of the cut cross section.

Concave 3D Mesh Does not support cross section retriangulation.

The algorithm was stripped from a previous DecalFramework (also written by me) project and repurposed.

### Algorithms In Use

The Algorithm Uses Barycentric Coordinates to generate new UV set for the new triangles. This allows the algorithm to cut textured objects.

The Cross Section Retriangulation (for Convex Objects) uses Andrew's Algorithm which is a mapping of 3D vertices into a 2D Plane. The result is a surface with absolute minimal triangles. The Cross Section has generated UV Coordinates and can be re-textured.

The Cutting Algorithm uses Plane to Triangle intersection to cut the individual triangles from the provided Mesh.

The overall system was designed to be as GC (Garbage Collection) friendly as possible. References to variables are used whenever possible.
