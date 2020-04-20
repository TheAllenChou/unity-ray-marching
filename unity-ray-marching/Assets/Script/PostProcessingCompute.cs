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
public class PostProcessingCompute : PostProcessingBase
{
  public ComputeShader Compute;
  private ComputeShader m_compute;
  private RenderTexture m_renderTarget;

  protected virtual void Init(ComputeShader compute) { }
  protected virtual void Dispose(ComputeShader compute) { }
  protected virtual void OnPreRenderImage(ComputeShader compute, RenderTexture src, RenderTexture dst) { }
  protected virtual void Dispatch(ComputeShader compute, RenderTexture src, RenderTexture dst) { }
  protected virtual void OnPostRenderImage(ComputeShader compute, RenderTexture src, RenderTexture dst) { }

  private void OnDisable()
  {
    if (m_compute == null)
      return;

    Dispose(m_compute);
    m_compute = null;
  }

  private void OnRenderImage(RenderTexture src, RenderTexture dst)
  {
    if (Compute != m_compute)
    {
      if (m_compute != null)
      {
        Dispose(m_compute);
        m_compute = null;
      }

      if (Compute != null)
      {
        m_compute = Compute;
        Init(m_compute);
      }
    }

    if (m_compute == null)
    {
      Debug.LogWarning("Compute shader is not assigned for post processing.");
      return;
    }

    if (m_renderTarget == null 
        || m_renderTarget.width != src.width 
        || m_renderTarget.height != src.height)
    {
      if (m_renderTarget != null)
      {
        Destroy(m_renderTarget);
        m_renderTarget = null;
      }

      m_renderTarget = new RenderTexture(src.width, src.height, 0);
      m_renderTarget.enableRandomWrite = true;
      m_renderTarget.Create();
    }

    OnPreRenderImage(m_compute, src, m_renderTarget);
    Dispatch(m_compute, src, m_renderTarget);
    Graphics.Blit(m_renderTarget, dst);
    OnPostRenderImage(m_compute, src, m_renderTarget);
  }
}
