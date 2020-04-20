/******************************************************************************/
/*
  Project - Unity Ray Marching
            https://github.com/TheAllenChou/unity-ray-marching
  
  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou
*/
/******************************************************************************/

using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class PostProcessingBlit : PostProcessingBase
{
  public Shader PostProcessingShader;

  private Material m_material;

  private void OnRenderImage(RenderTexture src, RenderTexture dst)
  {
    if (PostProcessingShader == null)
    {
      Debug.LogWarning("Shader is not assigned for post processing.");
      return;
    }

    if (PostProcessingShader != null)
    {
      if (m_material == null)
      {
        m_material = new Material(PostProcessingShader);
      }

      if (m_material.shader != PostProcessingShader)
      {
        m_material.shader = PostProcessingShader;
      }
    }
  }
}
