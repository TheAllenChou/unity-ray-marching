/******************************************************************************/
/*
  Project - Unity Ray Marching
            https://github.com/TheAllenChou/unity-ray-marching
  
  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou
*/
/******************************************************************************/

#if UNITY_EDITOR
using System;
using UnityEditor;
#endif

using UnityEngine;

[ExecuteInEditMode]
public class PostProcessing : MonoBehaviour
{
  public bool ApplyToSceneCamera = true;

  public Shader PostProcessingShader;

  private Material m_material;

  protected virtual void PreRenderImage(Shader shader, RenderTexture src, RenderTexture dst) { }
  protected virtual void PostRenderImage(Shader shader, RenderTexture src, RenderTexture dst) { }

  protected void Update()
  {
    #if UNITY_EDITOR
    UpdateSceneCamera();
    #endif
  }

  protected void OnRenderImage(RenderTexture src, RenderTexture dst)
  {
    if (PostProcessingShader == null)
    {
      Debug.LogWarning("No post-processing shader is specified.");
      return;
    }

    if (m_material == null)
    {
      m_material = new Material(PostProcessingShader);
    }

    if (m_material.shader != PostProcessingShader)
    {
      m_material.shader = PostProcessingShader;
    }

    PreRenderImage(PostProcessingShader, src, dst);

    Graphics.Blit(src, dst, m_material);

    PostRenderImage(PostProcessingShader, src, dst);
  }

  #if UNITY_EDITOR
  private Camera m_sceneCamera;
  private PostProcessing m_postProcessing;
  private bool m_appliedToSceneCamera = false;
  private bool m_attachedToSceneCamera = false;

  private void DisposeSceneCamera()
  {
    if (m_postProcessing != null)
    {
      DestroyImmediate(m_postProcessing);
    }

    m_sceneCamera = null;
    m_postProcessing = null;
    m_appliedToSceneCamera = false;
  }

  private void UpdateSceneCamera()
  {
    if (m_attachedToSceneCamera)
      return;

    if (!ApplyToSceneCamera)
    {
      DisposeSceneCamera();
      return;
    }

    if (m_appliedToSceneCamera)
      return;

    var camera = EditorWindow.GetWindow<SceneView>()?.camera;
    if (m_sceneCamera && m_sceneCamera != camera)
    {
      DisposeSceneCamera();
    }
    m_sceneCamera = camera;

    if (camera == null)
      return;

    var pp = (PostProcessing) camera.GetComponent(GetType());
    if (pp && pp != m_postProcessing)
    {
      DestroyImmediate(pp);
      pp = null;
    }

    if (pp == null)
    {
      pp = (PostProcessing) camera.gameObject.AddComponent(GetType());
      pp.m_attachedToSceneCamera = true;
    }

    EditorUtility.CopySerialized(this, pp);
    m_postProcessing = pp;

    m_appliedToSceneCamera = true;
  }
  #endif
}
