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

#include "Primitives.cginc"

#define kSdfSphere   (0)
#define kSdfBox      (1)
#define kSdfCapsule  (2)
#define kSdfCylinder (3)

struct SdfShape
{
  int4   type;   // sphere     box        capsule    cylinder
  float4 data0;  // c.xyz, r   c.xyz, r   a.xyz, r   a.xyz, r
  float4 data1;  //            h.xyz      b.xyz      b.xyz
  float4 data2;  //            q
};

float sdf_shape(float3 p, SdfShape s)
{
  switch (s.type.x)
  {
    case kSdfSphere:
      return sdf_sphere(p, s.data0.xyz, s.data0.w);

    case kSdfBox:
      return sdf_box(p, s.data0.xyz, s.data1.xyz, s.data2, s.data0.w);

    case kSdfCapsule:
      return sdf_capsule(p, s.data0.xyz, s.data1.xyz, s.data0.w);

    case kSdfCylinder:
      return sdf_cylinder(p, s.data0.xyz, s.data1.xyz, s.data0.w);
  }
}

#endif
