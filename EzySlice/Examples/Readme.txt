Readme

Debug Scenes

1) IntersectionDebugScene

Showcases a simple triangle to plane intersection test using Unity's Gizmos. This algorithm
forms the basis for most of the Slicer framework. If you need to make any changes to the
intersection algorithm, please use this scene to validate that your changes will still work.

2) TriangulationDebugScene

Showcases a simple triangulation using Monotone Chain on random 3D points represented as
GameObjects. Use the script to add and remove as many points as you like. Drag the GameObjects
to see the triangulation in play. Triangles are rendered using Unity's Gizmos.

3) SliceDebugScene

Showcases a simple slicing scene with a Cube which is Textured and randomly rotated against
a Plane which is also randomly rotated. Use this scene to test your models to see how they will
get sliced up via the slicer framework. The Example Script is well documented on how it works
and comes with a Unity Editor extension.