/******************************************************************************/
/*
  Project - Unity Ray Marching
            https://github.com/TheAllenChou/unity-ray-marching

  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou

*/
/******************************************************************************/

#ifndef SDF_OPERATORS
#define SDF_OPERATORS

// http://www.iquilezles.org/www/articles/smin/smin.htm

// ops = subtraction operator
// opu = union operator

// raw subtraction (a - b)
float sdf_ops(float a, float b)
{
  return max(a, -b);
}

// raw union (a | b)
float sdf_opu(float a, float b)
{
    return min(a, b);
}

// smooth quadratic polynomial union (C1 continuity, order-dependent concatenation)
// k = 0.1f is a good default
float sdf_opu_quad(float a, float b, float k)
{
  float h = max(k - abs(a - b), 0.0f) / k;
  return min(a, b) - h * h * k * (1.0f / 4.0f);
}

// smooth cubic polynomial union (C2 continuity, order-dependent concatenation)
// k = 0.1f is a good default
float sdf_opu_cubic(float a, float b, float k)
{
  float h = max(k - abs(a - b), 0.0f) / k;
  return min(a, b) - h * h * h * k * (1.0f / 6.0f);
}

// smooth exponential union (infinite continuity, order-independent concatenation)
// k = 30.0f is a good default
#define sdf_opu_exp_concat_term(x, k) (exp2(-(k) * (x)))
#define sdf_opu_exp_concat_res(sum, k) (-log2(sum) / (k))
float sdf_opu_exp(float a, float b, float k) // 2-term concatenation
{
  float sum = sdf_opu_exp_concat_term(a, k) + sdf_opu_exp_concat_term(b, k);
  return sdf_opu_exp_concat_res(sum, k);
}

// smooth power union (infinite continuity, order-independent concatenation)
// k = 8.0f is a good default
#define sdf_opu_pow_concat_term(x, k) (pow((x), (k)))
#define sdf_opu_pow_concat_res(sum, prod, k) pow((prod) / (sum), 1.0f / (k))
float sdf_opu_pow(float a, float b, float k) // 2-term concatenation
{
  a = sdf_opu_pow_concat_term(a, k);
  b = sdf_opu_pow_concat_term(b, k);
  return sdf_opu_pow_concat_res(a + b, a * b, k);
}

// use smooth cubic as default smooth union
inline float sdf_opu_smooth(float a, float b, float h)
{
    return sdf_opu_cubic(a, b, h);
}

#endif
