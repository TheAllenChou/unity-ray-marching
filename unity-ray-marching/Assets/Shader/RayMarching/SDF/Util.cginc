/******************************************************************************/
/*
  Project - Unity Ray Marching
            https://github.com/TheAllenChou/unity-ray-marching

  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou

*/
/******************************************************************************/

#ifndef RAY_MARCHING_SDF_UTIL
#define RAY_MARCHING_SDF_UTIL

inline float min_comp(float3 v)
{
  return min(v.x, min(v.y, v.z));
}

inline float max_comp(float3 v)
{
  return max(v.x, max(v.y, v.z));
}

#endif
