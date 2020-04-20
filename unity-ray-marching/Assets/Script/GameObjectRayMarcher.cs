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

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class GameObjectRayMarcher : PostProcessingCompute
{
  public Color MissColor;

  [Min(1)]
  public int MaxRaySteps = 128;

  [Min(0.0f)]
  public float RayHitThreshold = 0.005f;

  [Min(0.0f)]
  public float MaxRayDistance = 1000.0f;


  private struct SdfShape
  {
    public static readonly int Stride = 4 * sizeof(int) + 12 * sizeof(float);

    public enum Type
    {
      Sphere, 
      Box, 
      Capsule, 
      Cylinder, 
    }

    int m_type;
    int m_padding0;
    int m_padding1;
    int m_padding2;

    Vector4 m_data0;
    Vector4 m_data1;
    Vector4 m_data2;

    private static void WarningSuppression()
    {
      SdfShape shape;
      shape.m_type = shape.m_padding2 = 0;
      shape.m_padding2 = shape.m_type = 0;
    }

    public static SdfShape Sphere(Vector3 center, float radius)
    {
      SdfShape shape;
      shape.m_type = 0;
      shape.m_padding0 = shape.m_padding1 = shape.m_padding2 = 0;
      
      shape.m_data0 = new Vector4(center.x, center.y, center.z, radius);
      shape.m_data1 = Vector4.zero;
      shape.m_data2 = Vector4.zero;

      return shape;
    }

    public static SdfShape Box(Vector3 center, Vector3 halfExtents, Quaternion rotation, float radius = 0.0f)
    {
      SdfShape shape;
      shape.m_type = 1;
      shape.m_padding0 = shape.m_padding1 = shape.m_padding2 = 0;

      shape.m_data0 = new Vector4(center.x, center.y, center.z, radius);
      shape.m_data1 = new Vector4(halfExtents.x, halfExtents.y, halfExtents.z);
      shape.m_data2 = new Vector4(rotation.x, rotation.y, rotation.z, rotation.w);

      return shape;
    }

    public static SdfShape Capsule(Vector3 a, Vector3 b, float radius)
    {
      SdfShape shape;
      shape.m_type = 2;
      shape.m_padding0 = shape.m_padding1 = shape.m_padding2 = 0;

      shape.m_data0 = new Vector4(a.x, a.y, a.z, radius);
      shape.m_data1 = new Vector4(b.x, b.y, b.z);
      shape.m_data2 = Vector4.zero;

      return shape;
    }

    public static SdfShape Cylinder(Vector3 a, Vector3 b, float radius)
    {
      SdfShape shape;
      shape.m_type = 3;
      shape.m_padding0 = shape.m_padding1 = shape.m_padding2 = 0;

      shape.m_data0 = new Vector4(a.x, a.y, a.z, radius);
      shape.m_data1 = new Vector4(b.x, b.y, b.z);
      shape.m_data2 = Vector4.zero;

      return shape;
    }
  }

  private struct ShaderConstants
  {
    public int Kernel;
    public int Src;
    public int Dst;
    public int CameraInverseProjection;
    public int CameraToWorld;
    public int CameraPosition;
    public int ScreenSize;
    public int SdfShapes;
    public int NumSdfShapes;
    public int MissColor;
    public int RayMarchParams;
  }

  public List<Transform> Shapes;

  private ShaderConstants m_const;
  private ComputeBuffer m_shapes;

  protected override void Init(ComputeShader compute)
  {
    m_const.Kernel = Compute.FindKernel("RayMarch");
    m_const.Src = Shader.PropertyToID("src");
    m_const.Dst = Shader.PropertyToID("dst");
    m_const.CameraInverseProjection = Shader.PropertyToID("cameraInProj");
    m_const.CameraToWorld = Shader.PropertyToID("cameraToWorld");
    m_const.CameraPosition = Shader.PropertyToID("cameraPos");
    m_const.ScreenSize = Shader.PropertyToID("screenSize");
    m_const.SdfShapes = Shader.PropertyToID("aSdfShape");
    m_const.NumSdfShapes = Shader.PropertyToID("numSdfShapes");
    m_const.MissColor = Shader.PropertyToID("missColor");
    m_const.RayMarchParams = Shader.PropertyToID("rayMarchParams");
  }

  protected override void Dispose(ComputeShader compute)
  {
    if (m_shapes != null)
    {
      m_shapes.Dispose();
      m_shapes = null;
    }
  }

  protected override void OnPreRenderImage(ComputeShader compute, RenderTexture src, RenderTexture dst)
  {
    int numShapes = Shapes != null ? Mathf.Max(1, Shapes.Count) : 1;
    if (m_shapes == null || m_shapes.count != numShapes)
    {
      if (m_shapes != null)
      {
        m_shapes.Dispose();
      }

      m_shapes = new ComputeBuffer(numShapes, SdfShape.Stride);
    }

    var camera = GetComponent<Camera>();

    compute.SetTexture(m_const.Kernel, m_const.Src, src);
    compute.SetTexture(m_const.Kernel, m_const.Dst, dst);
    compute.SetMatrix(m_const.CameraInverseProjection, camera.projectionMatrix.inverse);
    compute.SetMatrix(m_const.CameraToWorld, camera.cameraToWorldMatrix);
    compute.SetVector(m_const.CameraPosition, camera.transform.position);
    compute.SetInts(m_const.ScreenSize, new int[] { camera.pixelWidth, camera.pixelHeight });
    compute.SetBuffer(m_const.Kernel, m_const.SdfShapes, m_shapes);
    compute.SetInt(m_const.NumSdfShapes, numShapes);
    compute.SetVector(m_const.MissColor, new Vector4(MissColor.r, MissColor.g, MissColor.b, MissColor.a));
    compute.SetVector(m_const.RayMarchParams, new Vector4(MaxRaySteps, RayHitThreshold, MaxRayDistance, Time.time));
  }

  protected override void Dispatch(ComputeShader compute, RenderTexture src, RenderTexture dst)
  {
    var camera = GetComponent<Camera>();
    compute.Dispatch(m_const.Kernel, (camera.pixelWidth + 7) / 8, (camera.pixelHeight + 7) / 8, 1);
  }
}
