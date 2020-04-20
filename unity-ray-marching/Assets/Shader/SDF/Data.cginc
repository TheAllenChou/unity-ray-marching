/******************************************************************************/
/*
  Project - Unity Ray Marching
            https://github.com/TheAllenChou/unity-ray-marching

  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou

*/
/******************************************************************************/

#ifndef RAY_MARCHING_SDF_DATA
#define RAY_MARCHING_SDF_DATA

#define kSdfSphere   (0)
#define kSdfBox      (1)
#define kSdfCylinder (2)
#define kSdfCapsule  (3)

struct SdfShape
{
  int    type;
  float4 data0;
};

#endif
