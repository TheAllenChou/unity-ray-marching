/******************************************************************************/
/*
  Project - Unity Ray Marching
            https://github.com/TheAllenChou/unity-ray-marching
  
  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou
*/
/******************************************************************************/

using System;
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

  // actually half surface areas, but it's all relative so it's fine
  public float Area { get { Vector3 e = Max - Min; return e.x * e.y + e.y * e.z + e.z * e.x; } }

  public Vector3 Center { get { return 0.5f * (Min + Max); } }
  public Vector3 HalfExtents { get { return 0.5f * (Max - Min); } }

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

  public bool Contains(Aabb rhs)
  {
    return 
         Min.x <= rhs.Min.x 
      && Min.y <= rhs.Min.y 
      && Min.z <= rhs.Min.z 
      && Max.x >= rhs.Max.x 
      && Max.y >= rhs.Max.y 
      && Max.z >= rhs.Max.z;
  }

  // Real-time Collision Detection, p179.
  public float RayCast(Vector3 from, Vector3 to, float maxFraction = 1.0f)
  {
    float tMin = float.MinValue;
    float tMax = float.MaxValue;

    Vector3 d = to - from;
    Vector3 absD = VectorUtil.Abs(d);

    for (int i = 0; i < 3; ++i)
    {
      float dComp = VectorUtil.GetComopnent(d, i);
      float absDComp = VectorUtil.GetComopnent(absD, i);
      float fromComp = VectorUtil.GetComopnent(from, i);
      float minComp = VectorUtil.GetComopnent(Min, i);
      float maxComp = VectorUtil.GetComopnent(Max, i);

      if (absDComp< float.Epsilon)
      {
        // parallel?
        if (fromComp < minComp || minComp < fromComp)
          return float.MinValue;
      }
      else
      {
        float invD = 1.0f / dComp;
        float t1 = (minComp - fromComp) * invD;
        float t2 = (maxComp - fromComp) * invD;

        if (t1 > t2)
        {
          float temp = t1;
          t1 = t2;
          t2 = temp;
        }

        tMin = Mathf.Max(tMin, t1);
        tMax = Mathf.Min(tMax, t2);

        if (tMin > tMax)
          return float.MinValue;
      }
    }

    // does the ray start inside the box?
    // does the ray intersect beyond the max fraction?
    if (tMin < 0.0f || maxFraction < tMin)
      return float.MinValue;

    // intersection detected
    return tMin;
  }
}

// https://box2d.org/files/ErinCatto_DynamicBVH_GDC2019.pdf
// https://github.com/erincatto/box2d/blob/master/src/collision/b2_dynamic_tree.cpp
public class AabbTree<T> where T : class
{
  public static readonly int Null = -1;

  public delegate bool QueryCallbcak(T userData);
  public delegate float RayCastCallback(Vector3 from, Vector3 to, T userData);

  private static readonly float FatBoundsRadius = 0.5f;

  private struct Node
  {
    public Aabb Bounds; // fat AABB

    public int Parent;
    public int NextFree;
    public int ChildA;
    public int ChildB;
    public int Height; // leaf = 0, free = -1

    public T UserData;
    public bool Moved;

    public bool IsLeaf { get { return ChildA == Null; } }
  }

  private Node[] m_nodes = new Node[16];
  private int m_numNodes = 0;
  private int m_freeList = Null;
  private int m_root = Null;
  private Stack<int> m_stack = new Stack<int>(256);

  private int AllocateNode()
  {
    // no more free nodes?
    if (m_freeList == Null)
    {
      // expand capacity
      var oldNodes = m_nodes;
      m_nodes = new Node[oldNodes.Length * 2];
      oldNodes.CopyTo(m_nodes, 0);

      // set up free list
      for (int i = m_numNodes; i < m_nodes.Length - 1; ++i)
      {
        m_nodes[i].NextFree = i + 1;
        m_nodes[i].Height = -1;
      }
      m_nodes[m_nodes.Length - 1].NextFree = Null;
      m_nodes[m_nodes.Length - 1].Height = -1;
      m_freeList = m_numNodes;
    }

    // take a node from the free list
    int node = m_freeList;
    m_nodes[node].Parent = Null;
    m_nodes[node].ChildA = Null;
    m_nodes[node].ChildB = Null;
    m_nodes[node].Height = 0;
    m_nodes[node].UserData = null;
    m_nodes[node].Moved = false;
    ++m_numNodes;
    return node;
  }

  private void FreeNode(int node)
  {
    m_nodes[node].NextFree = m_freeList;
    m_nodes[node].Height = -1;
    m_freeList = node;
    --m_numNodes;
  }

  public int CreateProxy(Aabb bounds, T userData)
  {
    int proxy = AllocateNode();

    // make fat
    m_nodes[proxy].Bounds = bounds;
    m_nodes[proxy].Bounds.Expand(FatBoundsRadius); // make fat
    m_nodes[proxy].Height = 0;
    m_nodes[proxy].UserData = userData;
    m_nodes[proxy].Moved = true;

    InsertLeaf(proxy);

    return proxy;
  }

  public void DestroyProxy(int proxy)
  {
    RemoveLeaf(proxy);
    FreeNode(proxy);
  }

  public void UpdateProxy(int proxy, Aabb bounds)
  {
    if (m_nodes[proxy].Bounds.Contains(bounds))
      return;

    RemoveLeaf(proxy);

    m_nodes[proxy].Bounds = bounds;
    m_nodes[proxy].Bounds.Expand(FatBoundsRadius); // make fat

    InsertLeaf(proxy);

    m_nodes[proxy].Moved = true;
  }

  public Aabb GetBounds(int proxy)
  {
    return m_nodes[proxy].Bounds;
  }

  public bool Query(Aabb bounds, QueryCallbcak callback = null)
  {
    m_stack.Clear();
    m_stack.Push(m_root);

    bool touchedAnyBounds = false;
    while (m_stack.Count > 0)
    {
      int index = m_stack.Pop();
      if (index == Null)
        continue;

      if (!Aabb.Intersects(bounds, m_nodes[index].Bounds))
        continue;

      if (m_nodes[index].IsLeaf)
      {
        touchedAnyBounds = true;

        bool proceed = 
          callback != null 
            ? callback(m_nodes[index].UserData) 
            : true;

        if (!proceed)
          return true;
      }
      else
      {
        m_stack.Push(m_nodes[index].ChildA);
        m_stack.Push(m_nodes[index].ChildB);
      }
    }

    return touchedAnyBounds;
  }

  public bool RayCast(Vector3 from, Vector3 to, RayCastCallback callback = null)
  {
    Vector3 r = to - from;
    r.Normalize();

    float maxFraction = 1.0f;

    // v is perpendicular to the segment.
    Vector3 v = VectorUtil.FindOrthogonal(r);
    Vector3 absV = VectorUtil.Abs(v);

    // build a bounding box for the segment.
    Aabb rayBounds = Aabb.Empty;
    rayBounds.Include(from);
    rayBounds.Include(to);

    m_stack.Clear();
    m_stack.Push(m_root);

    bool hitAnyBounds = false;
    while (m_stack.Count > 0)
    {
      int index = m_stack.Pop();
      if (index == Null)
      {
        continue;
      }

      if (Aabb.Intersects(m_nodes[index].Bounds, rayBounds) == false)
      {
        continue;
      }

      // Separating axis for segment (Gino, p80).
      // |dot(v, a - c)| > dot(|v|, h)
      Vector3 c = m_nodes[index].Bounds.Center;
      Vector3 h = m_nodes[index].Bounds.HalfExtents;
      float separation = Mathf.Abs(Vector3.Dot(v, from - c)) - Vector3.Dot(absV, h);
      if (separation > 0.0f)
        continue;

      if (m_nodes[index].IsLeaf)
      {
        float t = m_nodes[index].Bounds.RayCast(from, to, maxFraction);
        if (t < 0.0f)
          continue;

        hitAnyBounds = true;

        float newMaxFraction = 
          callback != null 
            ? callback(from, to, m_nodes[index].UserData) 
            : maxFraction;

        if (newMaxFraction >= 0.0f)
        {
          // Update segment bounding box.
          maxFraction = newMaxFraction;
          Vector3 newTo = from + maxFraction * (to - from);
          rayBounds.Min = VectorUtil.Min(from, newTo);
          rayBounds.Max = VectorUtil.Max(from, newTo);
        }
      }
      else
      {
        m_stack.Push(m_nodes[index].ChildA);
        m_stack.Push(m_nodes[index].ChildB);
      }
    }

    return hitAnyBounds;
  }

  private void InsertLeaf(int leaf)
  {
    if (m_root == Null)
    {
      m_root = leaf;
      m_nodes[m_root].Parent = Null;
      return;
    }

    // find best sibling
    Aabb leafBounds = m_nodes[leaf].Bounds;
    int index = m_root;
    while (!m_nodes[index].IsLeaf)
    {
      int childA = m_nodes[index].ChildA;
      int childB = m_nodes[index].ChildB;

      float area = m_nodes[index].Bounds.Area;

      Aabb combinedBounds = Aabb.Union(m_nodes[index].Bounds, leafBounds);
      float combinedArea = combinedBounds.Area;

      // cost of creating a new parent for this node and the new leaf
      float cost = 2.0f * combinedArea;

      // minimum cost of pushing the leaf further down the tree
      float inheritanceCost = 2.0f * (combinedArea - area);

      // cost of descending into child A
      float costA;
      if (m_nodes[childA].IsLeaf)
      {
        Aabb bounds;
        bounds = Aabb.Union(leafBounds, m_nodes[childA].Bounds);
        costA = bounds.Area + inheritanceCost;
      }
      else
      {
        Aabb bounds;
        bounds = Aabb.Union(leafBounds, m_nodes[childA].Bounds);
        float oldArea = m_nodes[childA].Bounds.Area;
        float newArea = bounds.Area;
        costA = (newArea - oldArea) + inheritanceCost;
      }

      // cost of descending into child B
      float costB;
      if (m_nodes[childB].IsLeaf)
      {
        Aabb bounds;
        bounds = Aabb.Union(leafBounds, m_nodes[childB].Bounds);
        costB = bounds.Area + inheritanceCost;
      }
      else
      {
        Aabb bounds;
        bounds = Aabb.Union(leafBounds, m_nodes[childB].Bounds);
        float oldArea = m_nodes[childB].Bounds.Area;
        float newArea = bounds.Area;
        costB = (newArea - oldArea) + inheritanceCost;
      }

      // descend according to the minimum cost
      if (cost < costA && cost < costB)
        break;

      //descend
      index = (costA < costB) ? childA : childB;
    }

    int sibling = index;

    // create a new parent
    int oldParent = m_nodes[sibling].Parent;
    int newParent = AllocateNode();
    m_nodes[newParent].Parent = oldParent;
    m_nodes[newParent].Bounds = Aabb.Union(leafBounds, m_nodes[sibling].Bounds);
    m_nodes[newParent].Height = m_nodes[sibling].Height + 1;

    if (oldParent != Null)
    {
      // sibling was not the root
      if (m_nodes[oldParent].ChildA == sibling)
      {
        m_nodes[oldParent].ChildA = newParent;
      }
      else
      {
        m_nodes[oldParent].ChildB = newParent;
      }

      m_nodes[newParent].ChildA = sibling;
      m_nodes[newParent].ChildB = leaf;
      m_nodes[sibling].Parent = newParent;
      m_nodes[leaf].Parent = newParent;
    }
    else
    {
      // sibling was the root
      m_nodes[newParent].ChildA = sibling;
      m_nodes[newParent].ChildB = leaf;
      m_nodes[sibling].Parent = newParent;
      m_nodes[leaf].Parent = newParent;
      m_root = newParent;
    }

    // walk back up to re-balance heights
    index = m_nodes[leaf].Parent;
    while (index != Null)
    {
      index = Balance(index);

      int childA = m_nodes[index].ChildA;
      int childB = m_nodes[index].ChildB;
      m_nodes[index].Height = 1 + Mathf.Max(m_nodes[childA].Height, m_nodes[childB].Height);
      m_nodes[index].Bounds = Aabb.Union(m_nodes[childA].Bounds, m_nodes[childB].Bounds);

      index = m_nodes[index].Parent;
    }
  }

  private void RemoveLeaf(int leaf)
  {
    if (leaf == m_root)
    {
      m_root = Null;
      return;
    }

    int parent = m_nodes[leaf].Parent;
    int grandParent = m_nodes[parent].Parent;
    int sibling = 
      m_nodes[parent].ChildA == leaf 
        ? m_nodes[parent].ChildB 
        : m_nodes[parent].ChildA;

    if (grandParent != Null)
    {
      // destroy parent and connect sibling to grand parent
      if (m_nodes[grandParent].ChildA == parent)
      {
        m_nodes[grandParent].ChildA = sibling;
      }
      else
      {
        m_nodes[grandParent].ChildB = sibling;
      }
      m_nodes[sibling].Parent = grandParent;
      FreeNode(parent);

      // adjust ancestor bounds
      int index = grandParent;
      while (index != Null)
      {
        index = Balance(index);

        int childA = m_nodes[index].ChildA;
        int childB = m_nodes[index].ChildB;

        m_nodes[index].Bounds = Aabb.Union(m_nodes[childA].Bounds, m_nodes[childB].Bounds);
        m_nodes[index].Height = 1 + Mathf.Max(m_nodes[childA].Height, m_nodes[childB].Height);

        index = m_nodes[index].Parent;
      }
    }
    else
    {
      m_root = sibling;
      m_nodes[sibling].Parent = Null;
      FreeNode(parent);
    }
  }

  private int Balance(int a)
  {
    if (m_nodes[a].IsLeaf || m_nodes[a].Height < 2)
    {
      return a;
    }

    int b = m_nodes[a].ChildA;
    int c = m_nodes[a].ChildB;

    int balance = m_nodes[c].Height - m_nodes[b].Height;

    // rotate C up
    if (balance > 1)
    {
      int f = m_nodes[c].ChildA;
      int g = m_nodes[c].ChildB;

      // swap A and C
      m_nodes[c].ChildA = a;
      m_nodes[c].Parent = m_nodes[a].Parent;
      m_nodes[a].Parent = c;

      // A's old parent should point to C
      if (m_nodes[c].Parent != Null)
      {
        if (m_nodes[m_nodes[c].Parent].ChildA == a)
        {
          m_nodes[m_nodes[c].Parent].ChildA = c;
        }
        else
        {
          m_nodes[m_nodes[c].Parent].ChildB = c;
        }
      }
      else
      {
        m_root = c;
      }

      // rotate
      if (m_nodes[f].Height > m_nodes[g].Height)
      {
        m_nodes[c].ChildB = f;
        m_nodes[a].ChildB = g;
        m_nodes[g].Parent = a;
        m_nodes[a].Bounds = Aabb.Union(m_nodes[b].Bounds, m_nodes[g].Bounds);
        m_nodes[c].Bounds = Aabb.Union(m_nodes[a].Bounds, m_nodes[f].Bounds);

        m_nodes[a].Height = 1 + Mathf.Max(m_nodes[b].Height, m_nodes[g].Height);
        m_nodes[c].Height = 1 + Mathf.Max(m_nodes[a].Height, m_nodes[f].Height);
      }
      else
      {
        m_nodes[c].ChildB = g;
        m_nodes[a].ChildB = f;
        m_nodes[f].Parent = a;
        m_nodes[a].Bounds = Aabb.Union(m_nodes[b].Bounds, m_nodes[f].Bounds);
        m_nodes[c].Bounds = Aabb.Union(m_nodes[a].Bounds, m_nodes[g].Bounds);

        m_nodes[a].Height = 1 + Mathf.Max(m_nodes[b].Height, m_nodes[f].Height);
        m_nodes[c].Height = 1 + Mathf.Max(m_nodes[a].Height, m_nodes[g].Height);
      }

      return c;
    }

    // rotate B up
    if (balance < -1)
    {
      int d = m_nodes[b].ChildA;
      int e = m_nodes[b].ChildB;

      // swap A and B
      m_nodes[b].ChildA = a;
      m_nodes[b].Parent = m_nodes[a].Parent;
      m_nodes[a].Parent = b;

      // A's old parent should point to B
      if (m_nodes[b].Parent != Null)
      {
        if (m_nodes[m_nodes[b].Parent].ChildA == a)
        {
          m_nodes[m_nodes[b].Parent].ChildA = b;
        }
        else
        {
          m_nodes[m_nodes[b].Parent].ChildB = b;
        }
      }
      else
      {
        m_root = b;
      }

      // rotate
      if (m_nodes[d].Height > m_nodes[e].Height)
      {
        m_nodes[b].ChildB = d;
        m_nodes[a].ChildA = e;
        m_nodes[e].Parent = a;
        m_nodes[a].Bounds = Aabb.Union(m_nodes[c].Bounds, m_nodes[e].Bounds);
        m_nodes[b].Bounds = Aabb.Union(m_nodes[a].Bounds, m_nodes[d].Bounds);

        m_nodes[a].Height = 1 + Mathf.Max(m_nodes[c].Height, m_nodes[e].Height);
        m_nodes[b].Height = 1 + Mathf.Max(m_nodes[a].Height, m_nodes[d].Height);
      }
      else
      {
        m_nodes[b].ChildB = e;
        m_nodes[a].ChildA = d;
        m_nodes[d].Parent = a;
        m_nodes[a].Bounds = Aabb.Union(m_nodes[c].Bounds, m_nodes[d].Bounds);
        m_nodes[b].Bounds = Aabb.Union(m_nodes[a].Bounds, m_nodes[e].Bounds);

        m_nodes[a].Height = 1 + Mathf.Max(m_nodes[c].Height, m_nodes[d].Height);
        m_nodes[b].Height = 1 + Mathf.Max(m_nodes[a].Height, m_nodes[e].Height);
      }

      return b;
    }

    return a;
  }


}
