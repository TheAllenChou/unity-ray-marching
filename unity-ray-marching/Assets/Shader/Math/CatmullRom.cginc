/******************************************************************************/
/*
  Project - Unity Ray Marching
            https://github.com/TheAllenChou/unity-ray-marching
  
  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou
*/
/******************************************************************************/

#ifndef RAY_MARCHING_CATMULL_ROM
#define RAY_MARCHING_CATMULL_ROM

inline float catmullRom(float p0, float p1, float p2, float p3, float t)
{
  float tt = t * t;
  return
    0.5f
    * ((2.0f *  p1)
      + (-p0 + p2) * t
      + (2.0f *  p0 - 5.0f * p1 + 4.0f * p2 - p3) * tt
      + (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * tt * t
      );
}

inline float2 catmullRom(float2 p0, float2 p1, float2 p2, float2 p3, float t)
{
  float tt = t * t;
  return
    0.5f
    * ((2.0f *  p1)
      + (-p0 + p2) * t
      + (2.0f *  p0 - 5.0f * p1 + 4.0f * p2 - p3) * tt
      + (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * tt * t
      );
}

inline float3 catmullRom(float3 p0, float3 p1, float3 p2, float3 p3, float t)
{
  float tt = t * t;
  return 
    0.5f
    * ((2.0f *  p1) 
        + (-p0 + p2) * t 
        + (2.0f *  p0 - 5.0f * p1 + 4.0f * p2 - p3) * tt 
        + (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * tt * t
      );
}

inline float4 catmullRom(float4 p0, float4 p1, float4 p2, float4 p3, float t)
{
  float tt = t * t;
  return
    0.5f
    * ((2.0f *  p1)
      + (-p0 + p2) * t
      + (2.0f *  p0 - 5.0f * p1 + 4.0f * p2 - p3) * tt
      + (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * tt * t
      );
}

#endif
