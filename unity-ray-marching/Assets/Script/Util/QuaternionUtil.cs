/******************************************************************************/
/*
  Project - Unity Ray Marching
            https://github.com/TheAllenChou/unity-ray-marching
  
  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou
*/
/******************************************************************************/

using UnityEngine;

public class QuaternionUtil
{
  // basic stuff
  // ------------------------------------------------------------------------

  public static float Magnitude(Quaternion q)
  {
    return Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
  }

  public static float MagnitudeSqr(Quaternion q)
  {
    return q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
  }

  public static Quaternion Normalize(Quaternion q)
  {
    float magInv = 1.0f / Magnitude(q);
    return new Quaternion(magInv * q.x, magInv * q.y, magInv * q.z, magInv * q.w);
  }

  public static Quaternion AngularVector(Vector3 v)
  {
    float len = v.magnitude;
    if (len < MathUtil.Epsilon)
      return Quaternion.identity;

    v /= len;

    float h = 0.5f * len;
    float s = Mathf.Sin(h);
    float c = Mathf.Cos(h);

    return new Quaternion(s * v.x, s * v.y, s * v.z, c);
  }

  // axis must be normalized
  public static Quaternion AxisAngle(Vector3 axis, float angle)
  {
    float h = 0.5f * angle;
    float s = Mathf.Sin(h);
    float c = Mathf.Cos(h);

    return new Quaternion(s * axis.x, s * axis.y, s * axis.z, c);
  }

  public static Vector3 GetAxis(Quaternion q)
  {
    Vector3 v = new Vector3(q.x, q.y, q.z);
    float len = v.magnitude;
    if (len < MathUtil.Epsilon)
      return Vector3.left;

    return v / len;
  }

  public static float GetAngle(Quaternion q)
  {
    return 2.0f * Mathf.Acos(Mathf.Clamp(q.w, -1.0f, 1.0f));
  }

  public static Quaternion Pow(Quaternion q, float exp)
  {
    Vector3 axis = GetAxis(q);
    float angle = GetAngle(q);
    return AxisAngle(axis, angle * exp);
  }

  // v: derivative of q
  public static Quaternion Integrate(Quaternion q, Quaternion v, float dt)
  {
    return Pow(v, dt) * q;
  }

  // omega: angular velocity (direction is axis, magnitude is angle)
  // https://www.ashwinnarayan.com/post/how-to-integrate-quaternions/
  // https://gafferongames.com/post/physics_in_3d/
  public static Quaternion Integrate(Quaternion q, Vector3 omega, float dt)
  {
    omega *= 0.5f;
    Quaternion p = (new Quaternion(omega.x, omega.y, omega.z, 0.0f)) * q;
    return Normalize(new Quaternion(q.x + p.x * dt, q.y + p.y * dt, q.z + p.z * dt, q.w + p.w * dt));
  }

  public static Vector4 ToVector4(Quaternion q)
  {
    return new Vector4(q.x, q.y, q.z, q.w);
  }

  public static Quaternion FromVector4(Vector4 v, bool normalize = true)
  {
    if (normalize)
    {
      float magSqr = v.sqrMagnitude;
      if (magSqr < MathUtil.Epsilon)
        return Quaternion.identity;

      v /= Mathf.Sqrt(magSqr);
    }

    return new Quaternion(v.x, v.y, v.z, v.w);
  }

  // ------------------------------------------------------------------------
  // end: basic stuff


  // swing-twist decomposition & interpolation
  // ------------------------------------------------------------------------

  public static void DecomposeSwingTwist
  (
    Quaternion q, 
    Vector3 twistAxis, 
    out Quaternion swing, 
    out Quaternion twist
  )
  {
    Vector3 r = new Vector3(q.x, q.y, q.z); // (rotaiton axis) * cos(angle / 2)

    // singularity: rotation by 180 degree
    if (r.sqrMagnitude < MathUtil.Epsilon)
    {
      Vector3 rotatedTwistAxis = q * twistAxis;
      Vector3 swingAxis = Vector3.Cross(twistAxis, rotatedTwistAxis);

      if (swingAxis.sqrMagnitude > MathUtil.Epsilon)
      {
        float swingAngle = Vector3.Angle(twistAxis, rotatedTwistAxis);
        swing = Quaternion.AngleAxis(swingAngle, swingAxis);
      }
      else
      {
        // more singularity: rotation axis parallel to twist axis
        swing = Quaternion.identity; // no swing
      }

      // always twist 180 degree on singularity
      twist = Quaternion.AngleAxis(180.0f, twistAxis);
      return;
    }

    // formula & proof: 
    // http://www.euclideanspace.com/maths/geometry/rotations/for/decomposition/
    Vector3 p = Vector3.Project(r, twistAxis);
    twist = new Quaternion(p.x, p.y, p.z, q.w);
    twist = Normalize(twist);
    swing = q * Quaternion.Inverse(twist);
  }

  public enum SterpMode
  {
    // non-constant angular velocity, faster
    // use if interpolating across small angles or constant angular velocity is not important
    Nlerp,

    // constant angular velocity, slower
    // use if interpolating across large angles and constant angular velocity is important
    Slerp, 
  };

  // same swing & twist parameters
  public static Quaternion Sterp
  (
    Quaternion a, 
    Quaternion b, 
    Vector3 twistAxis, 
    float t, 
    SterpMode mode = SterpMode.Slerp
  )
  {
    Quaternion swing;
    Quaternion twist;
    return Sterp(a, b, twistAxis, t, out swing, out twist, mode);
  }

  // same swing & twist parameters with individual interpolated swing & twist outputs
  public static Quaternion Sterp
  (
    Quaternion a, 
    Quaternion b, 
    Vector3 twistAxis, 
    float t, 
    out Quaternion swing, 
    out Quaternion twist, 
    SterpMode mode = SterpMode.Slerp
  )
  {
    return Sterp(a, b, twistAxis, t, t, out swing, out twist, mode);
  }

  // different swing & twist parameters
  public static Quaternion Sterp
  (
    Quaternion a, 
    Quaternion b, 
    Vector3 twistAxis, 
    float tSwing, 
    float tTwist, 
    SterpMode mode = SterpMode.Slerp
  )
  {
    Quaternion swing;
    Quaternion twist;
    return Sterp(a, b, twistAxis, tSwing, tTwist, out swing, out twist, mode);
  }

  // master sterp function
  public static Quaternion Sterp
  (
    Quaternion a, 
    Quaternion b, 
    Vector3 twistAxis, 
    float tSwing, 
    float tTwist, 
    out Quaternion swing, 
    out Quaternion twist, 
    SterpMode mode
  )
  {
    Quaternion q = b * Quaternion.Inverse(a);
    Quaternion swingFull;
    Quaternion twistFull;
    QuaternionUtil.DecomposeSwingTwist(q, twistAxis, out swingFull, out twistFull);

    switch (mode)
    {
      default:
      case SterpMode.Nlerp:
        swing = Quaternion.Lerp(Quaternion.identity, swingFull, tSwing);
        twist = Quaternion.Lerp(Quaternion.identity, twistFull, tTwist);
        break;
      case SterpMode.Slerp:
        swing = Quaternion.Slerp(Quaternion.identity, swingFull, tSwing);
        twist = Quaternion.Slerp(Quaternion.identity, twistFull, tTwist);
        break;
    }

    return twist * swing;
  }

  // ------------------------------------------------------------------------
  // end: swing-twist decomposition & interpolation
}
