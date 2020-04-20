/******************************************************************************/
/*
  Project - Unity Ray Marching
            https://github.com/TheAllenChou/unity-ray-marching
  
  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou
*/
/******************************************************************************/

#ifndef RAY_MARCHING_VECTOR
#define RAY_MARCHING_VECTOR

#include "Math.cginc"

#define kUnitX (float3(1.0f, 0.0f, 0.0f))
#define kUnitY (float3(0.0f, 1.0f, 0.0f))
#define kUnitZ (float3(0.0f, 0.0f, 1.0f))

inline float3 normalize_safe(float3 v, float3 fallback)
{
  float vv = dot(v, v);
  return vv > kEpsilon ? v / sqrt(vv) : fallback;
}

inline float3 normalize_safe(float3 v)
{
  return normalize_safe(v, kUnitZ);
}

inline float3 project_vec(float3 v, float3 onto)
{
  onto = normalize(onto);
  return dot(v, onto) * onto;
}

inline float3 project_plane(float3 v, float3 n)
{
  return v - project_vec(v, n);
}

inline float3 limit_length(float3 v, float maxLen)
{
  return min(maxLen, length(v)) * normalize_safe(v, 0.0f);
}

inline float3 find_ortho(float3 v)
{
  if (v.x >= kSqrt3Inv)
    return float3(v.y, -v.x, 0.0);
  else
    return float3(0.0, v.z, -v.y);
}

inline float3 slerp(float3 a, float3 b, float t)
{
  float d = dot(normalize(a), normalize(b));
  if (d > kEpsilonComp)
  {
    return lerp(a, b, t);
  }

  float r = acos(clamp(d, -1.0f, 1.0f));
  return (sin((1.0 - t) * r) * a + sin(t * r) * b) / sin(r);
}

inline float3 nlerp(float3 a, float b, float t)
{
  return normalize(lerp(a, b, t));
}

inline float3x3 mat_basis(float3 xAxis, float3 yAxis, float3 zAxis)
{
  return transpose(float3x3(xAxis, yAxis, zAxis));
}

inline float3x3 mat_look_at(float3 dir, float3 up)
{
  float3 zAxis = normalize_safe(dir, kUnitZ);
  float3 xAxis = normalize_safe(cross(up, zAxis), kUnitX);
  float3 yAxis = cross(zAxis, xAxis);
  return mat_basis(xAxis, yAxis, zAxis);
}

#endif
