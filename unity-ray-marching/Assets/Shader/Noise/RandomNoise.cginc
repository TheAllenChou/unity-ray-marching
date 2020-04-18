/******************************************************************************/
/*
  Project - Unity Ray Marching
            https://github.com/TheAllenChou/unity-ray-marching

  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou

    Based on an Andy Gryc's version of the common one-line shader random noise
    http://byteblacksmith.com/improvements-to-the-canonical-one-liner-glsl-rand/

*/
/******************************************************************************/


#ifndef RAY_MARCHING_RANDOM_NOISE
#define RAY_MARCHING_RANDOM_NOISE

#include "NoiseCommon.cginc"

float rand(float s)
{
  return frac(sin(mod(s, 6.2831853)) * 43758.5453123);
}


float rand(float2 s)
{
  float d = dot(s + 0.1234567, float2(1111112.9819837, 78.237173));
  float m = mod(d, 6.2831853);
  return frac(sin(m) * 43758.5453123);
}

float rand(float3 s)
{
  float d = dot(s + 0.1234567, float3(11112.9819837, 378.237173, 3971977.9173179));
  float m = mod(d, 6.2831853);
  return frac(sin(m) * 43758.5453123);
}

float rand_range(float s, float a, float b)
{
  return a + (b - a) * rand(s);
}

float2 rand_range(float2 s, float2 a, float2 b)
{
  return a + (b - a) * rand(s);
}

float3 rand_range(float3 s, float3 a, float3 b)
{
  return a + (b - a) * rand(s);
}

float2 rand_uvec(float2 s)
{
  return normalize(float2(rand(s), rand(s * 1.23456789)) - 0.5);
}

float3 rand_uvec(float3 s)
{
  return normalize(float3(rand(s), rand(s * 1.23456789), rand(s * 9876.54321)) - 0.5);
}

float2 rand_vec(float2 s)
{
  return rand_uvec(s) * rand(s * 9876.54321);
}

float3 rand_vec(float3 s)
{
  return rand_uvec(s) * rand(s * 1357975.31313);
}

#endif
