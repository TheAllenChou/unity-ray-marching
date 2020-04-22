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

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class GameObjectRayMarcher : PostProcessingCompute
{
  private static readonly int TileSize = 8;

  public enum HeatMapModeEnum
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

  [Header("Bounding Volume Hierarchy")]
  [ConditionalField(Label = "Use BVH")]
  public bool UseBoundingVolumes = true;
  public bool DrawBoundingVolumes = false;
  [ConditionalField(Label = "  Isolate BVH Depth", Min = -1, Max = 16)]
  public int IsolateBoundingVolumeDepth = -1;
  [ConditionalField(Label = "Test BVH Bounds Query")]
  public bool TestBvhBoundsQuery = false;
  [ConditionalField(Label = "Test BVH Ray Cast")]
  public bool TestBvhRayCast = false;

  [Header("Heat Map")]
  public HeatMapModeEnum HeatMapMode = HeatMapModeEnum.None;
  [ConditionalField("HeatMapMode", HeatMapModeEnum.StepCountPerThread, HeatMapModeEnum.StepCountPerTile, Min = 1, Max = 256)]
  public int MaxStepCountBudget = 64;
  [ConditionalField("HeatMapMode", HeatMapModeEnum.ShapeCountPerThread, HeatMapModeEnum.ShapeCountPerTile, Min = 1, Max = 64)]
  public int MaxShapeCountBudget = 16;
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
    public int TempBuffer;

    public int BlendDistance;
    public int RayMarchParams;

    public int BackgroundColor;
    public int MissColor;

    public int UseAabbTree;
    public int AabbTree;
    public int AabbTreeRoot;

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
  private RenderTexture m_tempBuffer;
  private ComputeBuffer m_aabbTree;
  private RenderTexture m_heatMap;

  protected override void OnValidate()
  {
    base.OnValidate();

    BlendDistance = Mathf.Max(0.0f, BlendDistance);
    RayHitThreshold = Mathf.Max(0.0f, RayHitThreshold);
    MaxRayDistance = Mathf.Max(0.0f, MaxRayDistance);

    IsolateBoundingVolumeDepth = Mathf.Max(-1, IsolateBoundingVolumeDepth);
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
    m_const.TempBuffer = Shader.PropertyToID("tempBuffer");

    m_const.RayMarchParams = Shader.PropertyToID("rayMarchParams");
    m_const.BlendDistance = Shader.PropertyToID("blendDist");

    m_const.BackgroundColor = Shader.PropertyToID("backgroundColor");
    m_const.MissColor = Shader.PropertyToID("missColor");

    m_const.UseAabbTree = Shader.PropertyToID("useAabbTree");
    m_const.AabbTree = Shader.PropertyToID("aabbTree");
    m_const.AabbTreeRoot = Shader.PropertyToID("aabbTreeRoot");

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
    // validate SDF shapes buffer
    var sdfShapes = RayMarchedShape.GetShapes();
    int numShapes = sdfShapes.Count;
    if (m_shapes == null 
        || m_shapes.count != numShapes)
    {
      if (m_shapes != null)
      {
        m_shapes.Dispose();
        m_shapes = null;
      }

      m_shapes = new ComputeBuffer(Mathf.Max(1, numShapes), SdfShape.Stride);
    }

    // validate SDF temp buffer
    if (m_tempBuffer == null
        || m_tempBuffer.width != src.width
        || m_tempBuffer.height != src.height)
    {
      if (m_tempBuffer != null)
      {
        DestroyImmediate(m_tempBuffer);
        m_tempBuffer = null;
      }

      m_tempBuffer = new RenderTexture(src.width, src.height, 0, RenderTextureFormat.ARGBFloat);
      m_tempBuffer.enableRandomWrite = true;
      m_tempBuffer.Create();
    }

    // validate AABB tree buffer
    if (m_aabbTree == null 
        || m_aabbTree.count != RayMarchedShape.AabbTreeCapacity)
    {
      if (m_aabbTree != null)
      {
        m_aabbTree.Dispose();
        m_aabbTree = null;
      }

      m_aabbTree = new ComputeBuffer(RayMarchedShape.AabbTreeCapacity, AabbTree<RayMarchedShape>.NodePod.Stride);
    }

    // validate heat map buffer
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

    // fill buffers
    m_shapes.SetData(sdfShapes);
    RayMarchedShape.FillAabbTree(m_aabbTree, AabbTree<RayMarchedShape>.FatBoundsRadius - BlendDistance);

    if (m_const.Kernels == null)
    {
      InitShaderConstants();
    }

    foreach (int kernel in m_const.Kernels)
    {
      compute.SetTexture(kernel, m_const.Src, src);
      compute.SetTexture(kernel, m_const.Dst, dst);

      compute.SetBuffer(kernel, m_const.SdfShapes, m_shapes);
      compute.SetTexture(kernel, m_const.TempBuffer, m_tempBuffer);

      compute.SetBuffer(kernel, m_const.AabbTree, m_aabbTree);

      compute.SetTexture(kernel, m_const.HeatMap, m_heatMap);
    }

    var camera = GetComponent<Camera>();
    compute.SetMatrix(m_const.CameraInverseProjection, camera.projectionMatrix.inverse);
    compute.SetMatrix(m_const.CameraToWorld, camera.cameraToWorldMatrix);
    compute.SetVector(m_const.CameraPosition, camera.transform.position);
    compute.SetInts(m_const.ScreenSize, new int[] { camera.pixelWidth, camera.pixelHeight });

    compute.SetInt(m_const.NumSdfShapes, numShapes);

    compute.SetVector(m_const.RayMarchParams, new Vector4(MaxRaySteps, RayHitThreshold, MaxRayDistance, Time.time));
    compute.SetFloat(m_const.BlendDistance, BlendDistance);

    compute.SetVector(m_const.BackgroundColor, new Vector4(BackgroundColor.r, BackgroundColor.g, BackgroundColor.b, BackgroundColor.a));
    compute.SetVector(m_const.MissColor, new Vector4(MissColor.r, MissColor.g, MissColor.b, MissColor.a));

    compute.SetBool(m_const.UseAabbTree, UseBoundingVolumes);
    compute.SetInt(m_const.AabbTreeRoot, RayMarchedShape.AabbTreeRoot);

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

    switch (HeatMapMode)
    {
      case HeatMapModeEnum.StepCountPerThread:
        compute.SetInt(m_const.MaxCountBudget, MaxStepCountBudget);
        compute.Dispatch(m_const.StepCountKernelPerThread, threadGroupSizeX, threadGroupSizeY, 1);
        break;

      case HeatMapModeEnum.StepCountPerTile:
        compute.SetInt(m_const.MaxCountBudget, MaxStepCountBudget);
        compute.Dispatch(m_const.StepCountKernelPerTile, threadGroupSizeX, threadGroupSizeY, 1);
        break;

      case HeatMapModeEnum.ShapeCountPerThread:
        compute.SetInt(m_const.MaxCountBudget, MaxShapeCountBudget);
        compute.Dispatch(m_const.ShapeCountKernelPerThread, threadGroupSizeX, threadGroupSizeY, 1);
        break;

      case HeatMapModeEnum.ShapeCountPerTile:
        compute.SetInt(m_const.MaxCountBudget, MaxShapeCountBudget);
        compute.Dispatch(m_const.ShapeCountKernelPerTile, threadGroupSizeX, threadGroupSizeY, 1);
        break;
    }
  }

  private void OnDrawGizmos()
  {
    #if UNITY_EDITOR

    var camera = GetComponent<Camera>();

    if (DrawBoundingVolumes)
    {
      RayMarchedShape.DrawBoundingVolumeHierarchyGizmos(IsolateBoundingVolumeDepth);
    }

    if (TestBvhBoundsQuery)
    {
      Color prevColor = Handles.color;

      Aabb queryBounds = 
        new Aabb
        (
          camera.transform.position - 0.5f * Vector3.one, 
          camera.transform.position + 0.5f * Vector3.one
        );

      Handles.color = Color.yellow;
      Handles.DrawWireCube(queryBounds.Center, queryBounds.Extents);

      Handles.color = new Color(1.0f, 1.0f, 0.0f, 0.5f);
      RayMarchedShape.Query
      (
        queryBounds, 
        (RayMarchedShape shape) =>
        {
          Handles.DrawWireCube(shape.Bounds.Center, shape.Bounds.Extents);
          return true;
        }
      );

      Handles.color = prevColor;
    }

    if (TestBvhRayCast)
    {
      Color prevColor = Handles.color;

      Vector3 cameraFrom = camera.transform.position;
      Vector3 cameraTo = cameraFrom + 10.0f * camera.transform.forward;

      Handles.color = Color.yellow;
      Handles.DrawLine(cameraFrom, cameraTo);

      Handles.color = new Color(1.0f, 1.0f, 0.0f, 0.5f);
      RayMarchedShape.RayCast
      (
        cameraFrom, 
        cameraTo, 
        (Vector3 from, Vector3 to, RayMarchedShape shape) => 
        {
          Handles.DrawWireCube(shape.Bounds.Center, shape.Bounds.Extents);
          return 1.0f;
        }
      );

      Handles.color = prevColor;
    }

    #endif
  }
}
