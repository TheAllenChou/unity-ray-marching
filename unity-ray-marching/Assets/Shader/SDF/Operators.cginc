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
// http://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm


// union
//-----------------------------------------------------------------------------

// raw union
float sdf_uni(float a, float b)
{
  return min(a, b);
}

// smooth quadratic polynomial union (C1 continuity, order-dependent concatenation)
// k = 0.1f is a good default
float sdf_uni_quad(float a, float b, float k)
{
  float h = max(k - abs(a - b), 0.0f) / k;
  return min(a, b) - h * h * k * (1.0f / 4.0f);
}

// smooth cubic polynomial union (C2 continuity, order-dependent concatenation)
// k = 0.1f is a good default
float sdf_uni_cubic(float a, float b, float k)
{
  float h = max(k - abs(a - b), 0.0f) / k;
  return min(a, b) - h * h * h * k * (1.0f / 6.0f);
}

// smooth exponential union (infinite continuity, order-independent concatenation)
// k = 30.0f is a good default
#define sdf_uni_exp_concat_term(x, k) (exp2(-(k) * (x)))
#define sdf_uni_exp_concat_res(sum, k) (-log2(sum) / (k))
float sdf_uni_exp(float a, float b, float k) // 2-term concatenation
{
  float sum = sdf_uni_exp_concat_term(a, k) + sdf_uni_exp_concat_term(b, k);
  return sdf_uni_exp_concat_res(sum, k);
}

// smooth power union (infinite continuity, order-independent concatenation)
// k = 8.0f is a good default
#define sdf_uni_pow_concat_term(x, k) (pow((x), (k)))
#define sdf_uni_pow_concat_res(sum, prod, k) pow((prod) / (sum), 1.0f / (k))
float sdf_uni_pow(float a, float b, float k) // 2-term concatenation
{
  a = sdf_uni_pow_concat_term(a, k);
  b = sdf_uni_pow_concat_term(b, k);
  return sdf_uni_pow_concat_res(a + b, a * b, k);
}

// use cubic polynomial union as default
inline float sdf_uni_smooth(float a, float b, float h)
{
  return sdf_uni_cubic(a, b, h);
}

//-----------------------------------------------------------------------------
// end: union


// intersection
//-----------------------------------------------------------------------------

// raw intersection
float sdf_int(float a, float b)
{
  return max(a, b);
}

// smooth quadratic polynomial intersection (C1 continuity, order-dependent concatenation)
// k = 0.1f is a good default
float sdf_int_quad(float a, float b, float k)
{
  float h = max(k - abs(a - b), 0.0f) / k;
  return max(a, b) + h * h * k * (1.0f / 4.0f);
}

// smooth cubic polynomial intersection (C2 continuity, order-dependent concatenation)
// k = 0.1f is a good default
float sdf_int_cubic(float a, float b, float k)
{
  float h = max(k - abs(a - b), 0.0f) / k;
  return max(a, b) + h * h * h * k * (1.0f / 6.0f);
}

// use cubic polynomial intersection as default
inline float sdf_int_smooth(float a, float b, float h)
{
  return sdf_int_cubic(a, b, h);
}

//-----------------------------------------------------------------------------
// end: intersection


// subtraction
//-----------------------------------------------------------------------------

// raw subtraction
float sdf_sub(float a, float b)
{
  return max(a, -b);
}

// smooth quadratic polynomial subtraction (C1 continuity, order-dependent concatenation)
// k = 0.1f is a good default
float sdf_sub_quad(float a, float b, float k)
{
  float h = max(k - abs(a + b), 0.0f) / k;
  return max(a, -b) + h * h * k * (1.0f / 4.0f);
}

// smooth cubic polynomial subtraction (C2 continuity, order-dependent concatenation)
// k = 0.1f is a good default
float sdf_sub_cubic(float a, float b, float k)
{
  float h = max(k - abs(a + b), 0.0f) / k;
  return max(a, -b) + h * h * h * k * (1.0f / 6.0f);
}

// use cubic polynomial subtraction as default
inline float sdf_sub_smooth(float a, float b, float h)
{
  return sdf_sub_cubic(a, b, h);
}

//-----------------------------------------------------------------------------
// end: subtraction


#endif
