/******************************************************************************/
/*
  Project - Unity Ray Marching
            https://github.com/TheAllenChou/unity-ray-marching
  
  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou
*/
/******************************************************************************/

#ifndef RAY_MARCHING_MATH
#define RAY_MARCHING_MATH

#define kPi          (3.1415926535)
#define kTwoPi       (6.2831853071)
#define kHalfPi      (1.5707963267)
#define kThirdPi     (1.0471975511)
#define kQuarterPi   (0.7853981633)
#define kFifthPi     (0.6283185307)
#define kSixthPi     (0.5235987755)

#define kSqrt2       (1.4142135623)
#define kSqrt3       (1.7320508075)
#define kSqrt2Inv    (0.7071067811)
#define kSqrt3Inv    (0.5773502691)

#define kEpsilon     (1e-16f)
#define kEpsilonComp (1.0f - kEpsilon)

#define kRad2Deg     (57.295779513)
#define kDeg2Rad     (0.0174532925)

#include "Color.cginc"
#include "Vector.cginc"
#include "Quaternion.cginc"

#endif
