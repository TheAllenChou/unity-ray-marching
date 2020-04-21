/******************************************************************************/
/*
  Project - Unity Ray Marching
            https://github.com/TheAllenChou/unity-ray-marching
  
  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou
*/
/******************************************************************************/

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public struct Aabb
{
  public static readonly int Stride = 6 * sizeof(float);

  public Vector3 Min;
  public Vector3 Max;

  public static Aabb Union(Aabb a, Aabb b)
  {
    return 
      new Aabb
      (
        new Vector3
        (
          Mathf.Min(a.Min.x, b.Min.x), 
          Mathf.Min(a.Min.y, b.Min.y), 
          Mathf.Min(a.Min.z, b.Min.z)
        ), 
        new Vector3
        (
          Mathf.Min(a.Max.x, b.Max.x), 
          Mathf.Min(a.Max.y, b.Max.y), 
          Mathf.Min(a.Max.z, b.Max.z)
        )
      );
  }

  public static bool Intersects(Aabb a, Aabb b)
  {
    return 
         a.Min.x < b.Max.x && a.Max.x > b.Min.x 
      && a.Min.y < b.Max.y && a.Max.y > b.Min.y 
      && a.Min.z < b.Max.z && a.Max.z > b.Min.z;
  }

  private static Aabb s_empty = new Aabb(float.MaxValue * Vector3.one, float.MinValue * Vector3.one);
  public static Aabb Empty { get { return s_empty; } }

  public Vector3 Extents { get { return Max - Min; } }
  public Vector3 HalfExtents { get { return 0.5f * Extents; } }
  public float Area { get { return 2.0f * HalfArea; } }
  public float HalfArea { get { Vector3 e = Extents; return e.x * e.y + e.y * e.z + e.z * e.x; } }

  public Aabb(Vector3 min, Vector3 max)
  {
    Min = min;
    Max = max;
  }

  public void Include(Vector3 p)
  {
    Min.x = Mathf.Min(Min.x, p.x);
    Min.y = Mathf.Min(Min.y, p.y);
    Min.z = Mathf.Min(Min.z, p.z);

    Max.x = Mathf.Max(Max.x, p.x);
    Max.y = Mathf.Max(Max.y, p.y);
    Max.z = Mathf.Max(Max.z, p.z);
  }

  public void Expand(float r)
  {
    Min.x -= r;
    Min.y -= r;
    Min.z -= r;

    Max.x += r;
    Max.y += r;
    Max.z += r;
  }

  public void Expand(Vector3 r)
  {
    Min.x -= r.x;
    Min.y -= r.y;
    Min.z -= r.z;

    Max.x += r.x;
    Max.y += r.y;
    Max.z += r.z;
  }
}
