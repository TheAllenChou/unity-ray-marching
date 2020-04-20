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

public struct SdfShape
{
  public static readonly int Stride = 4 * sizeof(int) + 12 * sizeof(float);

  public enum TypeEnum
  {
    Sphere,
    Box,
    Capsule,
    Cylinder,
  }

  public int Type;
  public int Operator;
  public int Padding1;
  public int Padding2;

  public Vector4 Data0;
  public Vector4 Data1;
  public Vector4 Data2;

  private static void WarningSuppression()
  {
    SdfShape shape;
    shape.Type = shape.Padding2 = 0;
    shape.Padding2 = shape.Type = 0;
  }

  public static SdfShape Dummy()
  {
    SdfShape shape;
    shape.Type = 0;
    shape.Operator = 0;
    shape.Padding1 = 0; 
    shape.Padding2 = 0;

    shape.Data0 = Vector4.zero;
    shape.Data1 = Vector4.zero;
    shape.Data2 = Vector4.zero;

    return shape;
  }

  public static SdfShape Sphere(Vector3 center, float radius)
  {
    SdfShape shape;
    shape.Type = 0;
    shape.Operator = shape.Padding1 = shape.Padding2 = 0;

    shape.Data0 = new Vector4(center.x, center.y, center.z, radius);
    shape.Data1 = Vector4.zero;
    shape.Data2 = Vector4.zero;

    return shape;
  }

  public static SdfShape Box(Vector3 center, Vector3 halfExtents, Quaternion rotation, float radius = 0.0f)
  {
    SdfShape shape;
    shape.Type = 1;
    shape.Operator = shape.Padding1 = shape.Padding2 = 0;

    shape.Data0 = new Vector4(center.x, center.y, center.z, radius);
    shape.Data1 = new Vector4(halfExtents.x, halfExtents.y, halfExtents.z);
    shape.Data2 = new Vector4(rotation.x, rotation.y, rotation.z, rotation.w);

    return shape;
  }

  public static SdfShape Capsule(Vector3 a, Vector3 b, float radius)
  {
    SdfShape shape;
    shape.Type = 2;
    shape.Operator = shape.Padding1 = shape.Padding2 = 0;

    shape.Data0 = new Vector4(a.x, a.y, a.z, radius);
    shape.Data1 = new Vector4(b.x, b.y, b.z);
    shape.Data2 = Vector4.zero;

    return shape;
  }

  public static SdfShape Cylinder(Vector3 a, Vector3 b, float radius)
  {
    SdfShape shape;
    shape.Type = 3;
    shape.Operator = shape.Padding1 = shape.Padding2 = 0;

    shape.Data0 = new Vector4(a.x, a.y, a.z, radius);
    shape.Data1 = new Vector4(b.x, b.y, b.z);
    shape.Data2 = Vector4.zero;

    return shape;
  }
}