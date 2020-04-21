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
  private static AabbTree<RayMarchedShape> s_tree = new AabbTree<RayMarchedShape>();
  private static List<RayMarchedShape> s_shapeComponents = new List<RayMarchedShape>();
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
      var sdfShape = s.Shape;
      sdfShape.Operator = (int) s.Operator;
      s_sdfShapes.Add(sdfShape);
    }

    return s_sdfShapes;
  }

  public static void FillBounds(ComputeBuffer buffer)
  {
    foreach (var s in s_shapeComponents)
    {
      s_tree.UpdateProxy(s.m_iProxy, s.Bounds);
    }

    s_tree.Fill(buffer);
  }

  public enum OperatorEnum
  {
    Union, 
    Subtraction, 
    Intersection
  }

  public OperatorEnum Operator = OperatorEnum.Union;
  private int m_index = -1;
  private int m_iProxy = AabbTree<RayMarchedShape>.Null;

  private void OnEnable()
  {
    m_index = s_shapeComponents.Count;
    s_shapeComponents.Add(this);

    //m_iProxy = s_tree.CreateProxy(Bounds, this);
  }

  private void OnDisable()
  {
    s_shapeComponents[m_index] = s_shapeComponents[s_shapeComponents.Count - 1];
    s_shapeComponents[m_index].m_index = m_index;
    s_shapeComponents.RemoveAt(s_shapeComponents.Count - 1);
    m_index = -1;

    //s_tree.DestroyProxy(m_iProxy);
    m_iProxy = AabbTree<RayMarchedShape>.Null;
  }

  protected virtual void OnValidate() { }

  protected virtual SdfShape Shape { get { return SdfShape.Dummy(); } }
  public virtual Aabb Bounds { get { return Aabb.Empty; } }
}

