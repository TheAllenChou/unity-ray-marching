/******************************************************************************/
/*
  Project - Unity Ray Marching
            https://github.com/TheAllenChou/unity-ray-marching
  
  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou
*/
/******************************************************************************/

using System.Collections.Generic;

using UnityEngine;

[ExecuteInEditMode]
public class RayMarchedShape : MonoBehaviour
{
  private static HashSet<RayMarchedShape> s_shapeComponents = new HashSet<RayMarchedShape>();
  private static List<SdfShape> s_sdfShapes = new List<SdfShape>();
  public static List<SdfShape> GetShapes()
  {
    if (s_sdfShapes.Capacity < s_shapeComponents.Count)
    {
      s_sdfShapes.Capacity = s_shapeComponents.Count;
    }
    s_sdfShapes.Clear();

    foreach (var s in s_shapeComponents)
    {
      var sdfShape = s.GetShape();
      sdfShape.Operator = (int) s.Operator;
      s_sdfShapes.Add(sdfShape);
    }

    return s_sdfShapes;
  }

  public enum OperatorEnum
  {
    Union, 
    Subtraction, 
    Intersection
  }

  public OperatorEnum Operator = OperatorEnum.Union;

  private void OnEnable()
  {
    s_shapeComponents.Add(this);
  }

  private void OnDisable()
  {
    s_shapeComponents.Remove(this);
  }

  protected virtual SdfShape GetShape() { return SdfShape.Dummy(); }
}

