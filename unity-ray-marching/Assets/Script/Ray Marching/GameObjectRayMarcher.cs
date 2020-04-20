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
  [Min(1e-5f)]
  public float BlendDistance = 0.5f;

  public Color BackgroundColor = new Color(0.0f, 0.07f, 0.15f);
  public Color MissColor = new Color(1.0f, 0.0f, 0.0f);

  [Min(1)]
  public int MaxRaySteps = 128;

  [Min(1e-10f)]
  public float RayHitThreshold = 0.005f;

  [Min(0.0f)]
  public float MaxRayDistance = 1000.0f;

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

    public int BlendDistance;
    public int RayMarchParams;

    public int BackgroundColor;
    public int MissColor;
  }

  public List<Transform> Shapes;

  private ShaderConstants m_const;
  private ComputeBuffer m_shapes;

  protected override void Init(ComputeShader compute)
  {
    m_const.Kernel = Compute.FindKernel("RayMarch");

    m_const.Src = Shader.PropertyToID("src");
    m_const.Dst = Shader.PropertyToID("dst");

    m_const.CameraInverseProjection = Shader.PropertyToID("cameraInvProj");
    m_const.CameraToWorld = Shader.PropertyToID("cameraToWorld");
    m_const.CameraPosition = Shader.PropertyToID("cameraPos");
    m_const.ScreenSize = Shader.PropertyToID("screenSize");

    m_const.SdfShapes = Shader.PropertyToID("aSdfShape");
    m_const.NumSdfShapes = Shader.PropertyToID("numSdfShapes");

    m_const.RayMarchParams = Shader.PropertyToID("rayMarchParams");
    m_const.BlendDistance = Shader.PropertyToID("blendDist");

    m_const.BackgroundColor = Shader.PropertyToID("backgroundColor");
    m_const.MissColor = Shader.PropertyToID("missColor");
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
    var sdfShapes = RayMarchedShape.GetShapes();
    int numShapes = sdfShapes.Count;
    if (m_shapes == null || m_shapes.count != numShapes)
    {
      if (m_shapes != null)
      {
        m_shapes.Dispose();
      }

      m_shapes = new ComputeBuffer(numShapes, SdfShape.Stride);
    }

    m_shapes.SetData(sdfShapes);

    var camera = GetComponent<Camera>();

    compute.SetTexture(m_const.Kernel, m_const.Src, src);
    compute.SetTexture(m_const.Kernel, m_const.Dst, dst);

    compute.SetMatrix(m_const.CameraInverseProjection, camera.projectionMatrix.inverse);
    compute.SetMatrix(m_const.CameraToWorld, camera.cameraToWorldMatrix);
    compute.SetVector(m_const.CameraPosition, camera.transform.position);
    compute.SetInts(m_const.ScreenSize, new int[] { camera.pixelWidth, camera.pixelHeight });

    compute.SetBuffer(m_const.Kernel, m_const.SdfShapes, m_shapes);
    compute.SetInt(m_const.NumSdfShapes, numShapes);

    compute.SetVector(m_const.RayMarchParams, new Vector4(MaxRaySteps, RayHitThreshold, MaxRayDistance, Time.time));
    compute.SetFloat(m_const.BlendDistance, BlendDistance);

    compute.SetVector(m_const.BackgroundColor, new Vector4(BackgroundColor.r, BackgroundColor.g, BackgroundColor.b, BackgroundColor.a));
    compute.SetVector(m_const.MissColor, new Vector4(MissColor.r, MissColor.g, MissColor.b, MissColor.a));
  }

  protected override void Dispatch(ComputeShader compute, RenderTexture src, RenderTexture dst)
  {
    var camera = GetComponent<Camera>();
    compute.Dispatch(m_const.Kernel, (camera.pixelWidth + 7) / 8, (camera.pixelHeight + 7) / 8, 1);
  }
}
