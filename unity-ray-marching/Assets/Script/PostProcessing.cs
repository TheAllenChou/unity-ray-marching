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

public class PostProcessing : MonoBehaviour
{
  public Shader PostProcessingShader;

  private Material m_material;

  protected virtual void PreRenderImage(Shader shader, RenderTexture src, RenderTexture dst) { }
  protected virtual void PostRenderImate(Shader shader, RenderTexture src, RenderTexture dst) { }

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

    PostRenderImate(PostProcessingShader, src, dst);
  }
}
