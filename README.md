<h3 align="center">
  <img src="Graphics/icon.png?raw=true" alt="EzySlice Logo" width="600">
</h3>

[![Twitter: @DavidArayan](https://img.shields.io/badge/contact-DavidArayan-blue.svg?style=flat)](https://twitter.com/DavidArayan)
[![Join the chat at https://gitter.im/ezyframeworks/ezyslice](https://img.shields.io/badge/chat-gitter/ezyslice-green.svg?style=flat)](https://gitter.im/ezyframeworks/ezyslice)
[![License](https://img.shields.io/badge/license-MIT-orange.svg?style=flat)](LICENSE)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/76175609cae14f4b93feef579c630324)](https://app.codacy.com/app/DavidArayan/ezy-slice?utm_source=github.com&utm_medium=referral&utm_content=DavidArayan/ezy-slice&utm_campaign=Badge_Grade_Dashboard)

* * *

#### Open Source Slicer Framework for the Unity3D Game Engine

-   Ability to slice any convex Mesh using a Plane
-   UV/Normal/Tangent Space Interpolation for seamless cuts
-   Flexible and Documented API
-   No external plugin dependencies, fully written in C#
-   Updated for Unity3D 2018
-   MIT Open Source [License](LICENSE)

#### Algorithms in use

-   General purpose Monotone Chain for cross section triangulation for Convex slices
-   Barycentric Coordinates for UV/Normal/Tangent space Interpolation 
-   Purpose built Triangle to Plane intersection to cover all general cases for slicing
-   Designed with performance in mind

#### Contributions and Bug Reports

-   Contributions are always welcome and highly appreciated! please use pull request.
-   Bugs, Comments, General Enquiries and Feature Requests please use the Issue Tracker

#### Example Projects

-   Visit <https://github.com/DavidArayan/EzySlice-Example-Scenes> for example/debug scenes using the Slicer Framework. Example Repository is kept up to date with the latest changes on the main framework.
-   More Example Projects coming soon!

* * *

#### Usage Examples

Getting started with EzySlice is easy. Below you will find sample usage functions. EzySlice uses extension methods to hide most of the internal complexity.

-   The examples below will slice a GameObject and return SlicedHull object. SlicedHull has functionality for generating the final GameObjects for rendering. An additional API exists to generate the GameObjects automatically without any additional work required.
-   All functions will return null if slicing fails.

##### SlicedHull Example

```C#
public GameObject objectToSlice; // non-null

/**
 * Example on how to slice a GameObject in world coordinates.
 */
public SlicedHull Slice(Vector3 planeWorldPosition, Vector3 planeWorldDirection) {
	return objectToSlice.Slice(planeWorldPosition, planeWorldDirection);
}
```

##### Direct Instantiated Example

```C#
public GameObject objectToSlice; // non-null

/**
 * Example on how to slice a GameObject in world coordinates.
 */
public GameObject[] Slice(Vector3 planeWorldPosition, Vector3 planeWorldDirection) {
	return objectToSlice.SliceInstantiate(planeWorldPosition, planeWorldDirection);
}
```

-   A custom TextureRegion can be defined to map the final UV coordinates of the cross-section. TextureRegion is essentially a reference to a specific part of a texture in UV coordinate space. This can be useful if single materials are used repeatedly as atlasses.

##### SlicedHull Example

```C#
public GameObject objectToSlice; // non-null

/**
 * Example on how to slice a GameObject in world coordinates.
 * Uses a custom TextureRegion to offset the UV coordinates of the cross-section
 */
public SlicedHull Slice(Vector3 planeWorldPosition, Vector3 planeWorldDirection, TextureRegion region) {
	return objectToSlice.Slice(planeWorldPosition, planeWorldDirection, region);
}
```

##### Direct Instantiated Example

```C#
public GameObject objectToSlice; // non-null

/**
 * Example on how to slice a GameObject in world coordinates.
 * Uses a custom TextureRegion to offset the UV coordinates of the cross-section
 */
public GameObject[] Slice(Vector3 planeWorldPosition, Vector3 planeWorldDirection, TextureRegion region) {
	return objectToSlice.SliceInstantiate(planeWorldPosition, planeWorldDirection, region);
}
```

-   There are cases where supplying the material directly can have performance benefits. In the previous examples, the Slicer will simply create the cross section as a submesh which will allow adding the Material externally when the GameObject is created. By supplying the Material directly, this will allow the Slicer to potentially batch the final results instead of creating repeated submeshes.

##### SlicedHull Example

```C#
public GameObject objectToSlice; // non-null
public Material crossSectionMaterial; // non-null

/**
 * Example on how to slice a GameObject in world coordinates.
 * Uses a custom TextureRegion to offset the UV coordinates of the cross-section
 * Uses a custom Material
 */
public SlicedHull Slice(Vector3 planeWorldPosition, Vector3 planeWorldDirection, TextureRegion region) {
	return objectToSlice.Slice(planeWorldPosition, planeWorldDirection, region, crossSectionMaterial);
}
```

##### Direct Instantiated Example

```C#
public GameObject objectToSlice; // non-null
public Material crossSectionMaterial; // non-null

/**
 * Example on how to slice a GameObject in world coordinates.
 * Uses a custom TextureRegion to offset the UV coordinates of the cross-section
 * Uses a custom Material
 */
public GameObject[] Slice(Vector3 planeWorldPosition, Vector3 planeWorldDirection, TextureRegion region) {
	return objectToSlice.SliceInstantiate(planeWorldPosition, planeWorldDirection, region, crossSectionMaterial);
}
```

-   Below is a sample on how to generate a TextureRegion. TextureRegion is stored in UV Coordinate space and is a reference to a specific region of a texture.

##### Using a Texture

```C#
/**
 * Example on how to calculate a custom TextureRegion to reference a different part of a texture
 * 
 * px -> The start X Position in Pixel Coordinates
 * py -> The start Y Position in Pixel Coordinates
 * width -> The width of the texture in Pixel Coordinates
 * height -> The height of the texture in Pixel Coordinates
 */
public TextureRegion CalculateCustomRegion(Texture myTexture, int px, int py, int width, int height) {
	return myTexture.GetTextureRegion(px, py, width, height);
}
```

##### Using a Material

```C#
/**
 * Example on how to calculate a custom TextureRegion to reference a different part of a texture
 * This example will use the mainTexture component of a Material
 * 
 * px -> The start X Position in Pixel Coordinates
 * py -> The start Y Position in Pixel Coordinates
 * width -> The width of the texture in Pixel Coordinates
 * height -> The height of the texture in Pixel Coordinates
 */
public TextureRegion CalculateCustomRegion(Material myMaterial, int px, int py, int width, int height) {
	return myMaterial.GetTextureRegion(px, py, width, height);
}
```
