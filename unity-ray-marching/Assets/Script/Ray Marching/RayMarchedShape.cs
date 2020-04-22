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

  public static void Query(Aabb bounds, AabbTree<RayMarchedShape>.QueryCallbcak callback)
  {
    s_tree.Query(bounds, callback);
  }

  public static void RayCast(Vector3 from, Vector3 to, AabbTree<RayMarchedShape>.RayCastCallback callback)
  {
    s_tree.RayCast(from, to, callback);
  }

  public static int AabbTreeCapacity { get { return s_tree.Capacity; } }
  public static int AabbTreeRoot { get { return s_tree.Root; } }
  public static int FillAabbTree(ComputeBuffer buffer, float aabbTightenRadius = 0.0f)
  {
    SyncBounds();

    int root = s_tree.Fill(buffer, aabbTightenRadius);
    return root;
  }

  public static void SyncBounds()
  {
    foreach (var s in s_shapeComponents)
    {
      s_tree.UpdateProxy(s.m_iProxy, s.Bounds);
    }
  }

  public static void DrawBoundingVolumeHierarchyGizmos(int isolateDepth = -1)
  {
    SyncBounds();
    s_tree.DrwaGizmos(isolateDepth);
  }

  public enum OperatorEnum
  {
    Union, 
    Subtraction, 
    Intersection
  }

  public OperatorEnum Operator = OperatorEnum.Union;
  private int m_shapeIndex = -1;
  private int m_iProxy = AabbTree<RayMarchedShape>.Null;

  public int ShapeIndex { get { return m_shapeIndex; } }

  private void OnEnable()
  {
    m_shapeIndex = s_shapeComponents.Count;
    s_shapeComponents.Add(this);

    m_iProxy = s_tree.CreateProxy(Bounds, this);
  }

  private void OnDisable()
  {
    s_shapeComponents[m_shapeIndex] = s_shapeComponents[s_shapeComponents.Count - 1];
    s_shapeComponents[m_shapeIndex].m_shapeIndex = m_shapeIndex;
    s_shapeComponents.RemoveAt(s_shapeComponents.Count - 1);
    m_shapeIndex = -1;

    s_tree.DestroyProxy(m_iProxy);
    m_iProxy = AabbTree<RayMarchedShape>.Null;
  }

  protected virtual void OnValidate() { }

  protected virtual SdfShape Shape { get { return SdfShape.Dummy(); } }
  public virtual Aabb Bounds { get { return Aabb.Empty; } }
}

