using UnityEngine;

[ExecuteInEditMode]
public class DepthCamera : MonoBehaviour
{
  private Material material;

  void Awake()
  {
    this.material = new Material(Shader.Find("Custom/S_DepthShader"));
  }

  void OnRenderImage(RenderTexture sourceTexture, RenderTexture destinationTexture)
  {
    Graphics.Blit(sourceTexture, destinationTexture, this.material);
  }
}
