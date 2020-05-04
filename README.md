## Unity Ray Marching - My Sandbox for Learning Ray Marching
by **Ming-Lun "Allen" Chou** / [AllenChou.net](http://AllenChou.net) / [@TheAllenChou](http://twitter.com/TheAllenChou) / [Patreon](https://www.patreon.com/TheAllenChou)

![](/img/smooth-sphere-union.gif) ![](/img/interactive-game-objects.gif) ![](/img/heat-maps.gif)

## Features
  * Boolean operations: union, subtraction, intersection (WIP).
  * Heat maps: number of ray steps & number of SDF evaluations.

## Optimization Techniques
  * Bounding volume hierarchy (BVH) using dynamic AABB trees.
  * Per-pixel ray cast against BVH.

## References & Resources
  * [How Ray Marching & Signed Distance Fields (SDF) Work](http://www.michaelwalczyk.com/blog/2017/5/25/ray-marching)
  * [Smooth SDF Unions](http://www.iquilezles.org/www/articles/smin/smin.htm)
  * [SDF Normals](http://www.iquilezles.org/www/articles/normalsSDF/normalsSDF.htm)
  * [Shadertoy](http://shadertoy.com)
  * [Dynamic Bounding Volume Hierarchies (GDC 2019)](https://box2d.org/files/ErinCatto_DynamicBVH_GDC2019.pdf)
  * [Box2D](https://github.com/erincatto/box2d)
