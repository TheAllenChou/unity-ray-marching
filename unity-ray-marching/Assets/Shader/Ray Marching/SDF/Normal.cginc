/******************************************************************************/
/*
  Project - Unity Ray Marching
            https://github.com/TheAllenChou/unity-ray-marching

  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou

*/
/******************************************************************************/

#ifndef RAY_MARCHING_SDF_NORMAL
#define RAY_MARCHING_SDF_NORMAL

// http://iquilezles.org/www/articles/normalsSDF/normalsSDF.htm

// central differences
#define sdf_normal_diff(p, sdf, h)                                                      \
  normalize                                                                             \
  (                                                                                     \
    float3                                                                              \
    (                                                                                   \
      sdf((p) + float3(   (h), 0.0f, 0.0f))) - sdf((p) - float3(   (h), 0.0f, 0.0f))),  \
      sdf((p) + float3(0.0f,    (h), 0.0f))) - sdf((p) - float3(0.0f,    (h), 0.0f))),  \
      sdf((p) + float3(0.0f, 0.0f,    (h)))) - sdf((p) - float3(0.0f, 0.0f,    (h))))   \
    )                                                                                   \
  )

// tetrahedron technique
#define sdf_normal_tetra(p, sdf, h)                                                     \
  normalize                                                                             \
  (                                                                                     \
      float3( 1.0f, -1.0f, -1.0f) * sdf((p) + float3( (h), -(h), -(h)))                 \
    + float3(-1.0f, -1.0f,  1.0f) * sdf((p) + float3(-(h), -(h),  (h)))                 \
    + float3(-1.0f,  1.0f, -1.0f) * sdf((p) + float3(-(h),  (h), -(h)))                 \
    + float3( 1.0f,  1.0f,  1.0f) * sdf((p) + float3( (h),  (h),  (h)))                 \
  )

// use tetrahedron technique as default
#define sdf_normal(p, sdf, h) sdf_normal_tetra(p, sdf, h)

#endif
