/******************************************************************************/
/*
  Project - Unity Ray Marching
            https://github.com/TheAllenChou/unity-ray-marching

  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou

*/
/******************************************************************************/

#ifndef SDF_PRIMITIVES
#define SDF_PRIMITIVES

float sdf_sphere(float3 p, float3 c, float r)
{
  return length(p - c) - r;
}

#endif
