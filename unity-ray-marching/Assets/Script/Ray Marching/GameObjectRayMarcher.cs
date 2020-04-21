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
  private static readonly int TileSize = 8;

  public enum DebugModeEnum
  {
    None, 
    StepCountPerThread, 
    StepCountPerTile, 
    ShapeCountPerThread, 
    ShapeCountPerTile, 
  }

  [Header("Ray Marching")]  
  public float BlendDistance = 0.5f;
  public Color BackgroundColor = new Color(0.0f, 0.07f, 0.15f);
  public Color MissColor = new Color(1.0f, 0.0f, 0.0f);
  [Range(1, 256)]
  public int MaxRaySteps = 128;
  public float RayHitThreshold = 0.005f;
  public float MaxRayDistance = 1000.0f;

  [Header("Debug")]
  public DebugModeEnum DebugMode = DebugModeEnum.None;
  [ConditionalField("DebugMode", DebugModeEnum.StepCountPerThread, DebugModeEnum.StepCountPerTile, Min = 1, Max = 256)]
  public int MaxStepCountBudget = 128;
  [ConditionalField("DebugMode", DebugModeEnum.ShapeCountPerThread, DebugModeEnum.ShapeCountPerTile, Min = 1, Max = 256)]
  public int MaxShapeCountBudget = 64;
  public Color HeatColorCool = new Color(0.0f, 1.0f, 0.0f);
  public Color HeatColorMedium = new Color(1.0f, 1.0f, 0.0f);
  public Color HeatColorHot = new Color(1.0f, 0.0f, 0.0f);
  [Range(0.0f, 1.0f)]
  public float HeatAlpha = 0.5f;

  private struct ShaderConstants
  {
    public ICollection<int> Kernels;
    public int MainKernel;
    public int StepCountKernelPerThread;
    public int StepCountKernelPerTile;
    public int ShapeCountKernelPerThread;
    public int ShapeCountKernelPerTile;

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

    public int HeatMap;
    public int HeatColorCool;
    public int HeatColorMedium;
    public int HeatColorHot;
    public int HeatAlpha;

    public int MaxCountBudget;
  }

  public List<Transform> Shapes;

  private ShaderConstants m_const;
  private ComputeBuffer m_shapes;
  private RenderTexture m_heatMap;

  protected override void OnValidate()
  {
    base.OnValidate();

    BlendDistance = Mathf.Max(0.0f, BlendDistance);
    RayHitThreshold = Mathf.Max(0.0f, RayHitThreshold);
    MaxRayDistance = Mathf.Max(0.0f, MaxRayDistance);
  }

  protected override void Init(ComputeShader compute)
  {
    InitShaderConstants();
  }

  private void InitShaderConstants()
  {
    m_const.Kernels = 
      new int[]
      {
        m_const.MainKernel = Compute.FindKernel("Main"), 
        m_const.StepCountKernelPerThread = Compute.FindKernel("StepCountPerThread"), 
        m_const.StepCountKernelPerTile = Compute.FindKernel("StepCountPerTile"), 
        m_const.ShapeCountKernelPerThread = Compute.FindKernel("ShapeCountPerThread"), 
        m_const.ShapeCountKernelPerTile = Compute.FindKernel("ShapeCountPerTile"), 
      };

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

    m_const.HeatMap = Shader.PropertyToID("heatMap");
    m_const.HeatColorCool = Shader.PropertyToID("heatColorCool");
    m_const.HeatColorMedium = Shader.PropertyToID("heatColorMedium");
    m_const.HeatColorHot = Shader.PropertyToID("heatColorHot");
    m_const.HeatAlpha = Shader.PropertyToID("heatAlpha");

    m_const.MaxCountBudget = Shader.PropertyToID("maxCountBudget");
  }

  protected override void Dispose(ComputeShader compute)
  {
    if (m_shapes != null)
    {
      m_shapes.Dispose();
      m_shapes = null;
    }

    if (m_heatMap != null)
    {
      DestroyImmediate(m_heatMap);
      m_heatMap = null;
    }
  }

  protected override void OnPreRenderImage(ComputeShader compute, RenderTexture src, RenderTexture dst)
  {
    var sdfShapes = RayMarchedShape.GetShapes();
    int numShapes = sdfShapes.Count;
    if (m_shapes == null 
        || m_shapes.count != numShapes)
    {
      if (m_shapes != null)
      {
        m_shapes.Dispose();
      }

      m_shapes = new ComputeBuffer(Mathf.Max(1, numShapes), SdfShape.Stride);
    }

    if (m_heatMap == null 
        || m_heatMap.width != src.width 
        || m_heatMap.height != src.height)
    {
      if (m_heatMap != null)
      {
        DestroyImmediate(m_heatMap);
        m_heatMap = null;
      }

      m_heatMap = new RenderTexture(src.width, src.height, 0, RenderTextureFormat.RFloat);
      m_heatMap.enableRandomWrite = true;
      m_heatMap.Create();
    }

    m_shapes.SetData(sdfShapes);

    var camera = GetComponent<Camera>();

    if (m_const.Kernels == null)
    {
      InitShaderConstants();
    }

    foreach (int kernel in m_const.Kernels)
    {
      compute.SetTexture(kernel, m_const.Src, src);
      compute.SetTexture(kernel, m_const.Dst, dst);
      compute.SetTexture(kernel, m_const.HeatMap, m_heatMap);

      compute.SetBuffer(kernel, m_const.SdfShapes, m_shapes);
    }

    compute.SetMatrix(m_const.CameraInverseProjection, camera.projectionMatrix.inverse);
    compute.SetMatrix(m_const.CameraToWorld, camera.cameraToWorldMatrix);
    compute.SetVector(m_const.CameraPosition, camera.transform.position);
    compute.SetInts(m_const.ScreenSize, new int[] { camera.pixelWidth, camera.pixelHeight });

    compute.SetInt(m_const.NumSdfShapes, numShapes);

    compute.SetVector(m_const.RayMarchParams, new Vector4(MaxRaySteps, RayHitThreshold, MaxRayDistance, Time.time));
    compute.SetFloat(m_const.BlendDistance, BlendDistance);

    compute.SetVector(m_const.BackgroundColor, new Vector4(BackgroundColor.r, BackgroundColor.g, BackgroundColor.b, BackgroundColor.a));
    compute.SetVector(m_const.MissColor, new Vector4(MissColor.r, MissColor.g, MissColor.b, MissColor.a));

    compute.SetVector(m_const.HeatColorCool, new Vector4(HeatColorCool.r, HeatColorCool.g, HeatColorCool.b, HeatColorCool.a));
    compute.SetVector(m_const.HeatColorMedium, new Vector4(HeatColorMedium.r, HeatColorMedium.g, HeatColorMedium.b, HeatColorMedium.a));
    compute.SetVector(m_const.HeatColorHot, new Vector4(HeatColorHot.r, HeatColorHot.g, HeatColorHot.b, HeatColorHot.a));
    compute.SetFloat(m_const.HeatAlpha, HeatAlpha);
  }

  protected override void Dispatch(ComputeShader compute, RenderTexture src, RenderTexture dst)
  {
    var camera = GetComponent<Camera>();

    int threadGroupSizeX = (camera.pixelWidth + TileSize - 1) / TileSize;
    int threadGroupSizeY = (camera.pixelHeight + TileSize - 1) / TileSize;

    compute.Dispatch(m_const.MainKernel, threadGroupSizeX, threadGroupSizeY, 1);

    switch (DebugMode)
    {
      case DebugModeEnum.StepCountPerThread:
        compute.SetInt(m_const.MaxCountBudget, MaxStepCountBudget);
        compute.Dispatch(m_const.StepCountKernelPerThread, threadGroupSizeX, threadGroupSizeY, 1);
        break;

      case DebugModeEnum.StepCountPerTile:
        compute.SetInt(m_const.MaxCountBudget, MaxStepCountBudget);
        compute.Dispatch(m_const.StepCountKernelPerTile, threadGroupSizeX, threadGroupSizeY, 1);
        break;

      case DebugModeEnum.ShapeCountPerThread:
        compute.SetInt(m_const.MaxCountBudget, MaxShapeCountBudget);
        compute.Dispatch(m_const.StepCountKernelPerThread, threadGroupSizeX, threadGroupSizeY, 1);
        break;

      case DebugModeEnum.ShapeCountPerTile:
        compute.SetInt(m_const.MaxCountBudget, MaxShapeCountBudget);
        compute.Dispatch(m_const.StepCountKernelPerTile, threadGroupSizeX, threadGroupSizeY, 1);
        break;
    }
  }
}
