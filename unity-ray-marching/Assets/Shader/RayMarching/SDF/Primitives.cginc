/******************************************************************************/
/*
  Project - Unity Ray Marching
            https://github.com/TheAllenChou/unity-ray-marching

  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou

*/
/******************************************************************************/

#ifndef RAY_MARCHING_SDF_PRIMITIVES
#define RAY_MARCHING_SDF_PRIMITIVES

#include "../../Math/Quaternion.cginc"
#include "Util.cginc"

// c: center
// r: radius
float sdf_sphere(float3 p, float3 c, float r)
{
  p -= c;
  return length(p) - r;
}

// c: center
// h: half extents
// r: radius
// q: rotation
float sdf_box(float3 p, float3 c, float3 h, float4 q = kQuatIdentity, float r = 0.0f)
{
  p = quat_rot(quat_inv(q), p - c);
  float3 d = abs(p) - h;
  return length(max(d, 0.0f)) + min(max_comp(d), 0.0f) - r;
}

// a: point A
// b: point B
// r: radius
float sdf_capsule(float3 p, float3 a, float3 b, float r)
{
  float3 ab = b - a;
  float3 ap = p - a;
  p -= a + saturate(dot(ap, ab) / dot(ab, ab)) * ab;
  return length(p) - r;
}

// a: point A
// b: point B
// r: radius
float sdf_cylinder(float3 p, float3 a, float3 b, float r)
{
  float3 ab = b - a;
  float3 ap = p - a;
  float t = dot(ap, ab) / dot(ab, ab);
  float3 q =  a + saturate(t) * ab;

  if (t >= 0.0f && t <= 1.0f)
    return length(p - q) - r;

  float3 c = q + limit_length(project_plane(p - q, ab), r);
  return length(p - c);
}

#endif
