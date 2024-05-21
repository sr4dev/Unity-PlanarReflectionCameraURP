# PlanarReflectionCameraURP
Implementing reflections in water or mirrors using two cameras.

This project is a reimplementation of MirrorReflection4 based on the Built-in Render Pipeline, adapted for the Universal Render Pipeline (URP).

https://github.com/sr4dev/Unity-PlanarReflectionCamera/assets/9159336/b2cdc454-23f6-4db6-b90b-27dddcc4c2a7

## How to use?
1. Copy the [Core](https://github.com/sr4dev/Unity-PlanarReflectionCameraURP/tree/main/Assets/Core) folder into your project.
2. Add a [PlanarReflectionCamera](https://github.com/sr4dev/Unity-PlanarReflectionCameraURP/blob/main/Assets/Core/PlanarReflectionCamera.cs) Component to water MeshRenderer.
3. Add or change a Material's shader to ['Shader Graph/PlanarReflectionCamera'](https://github.com/sr4dev/Unity-PlanarReflectionCameraURP/blob/main/Assets/Core/PlanarReflectionCamera.shadergraph).

If you want to know more detailed usage instructions, please check the sample scene.

## References
- http://wiki.unity3d.com/index.php/MirrorReflection4
- https://github.com/fuqunaga/MirrorReflection
