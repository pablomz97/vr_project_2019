using UnityEngine;

[ExecuteInEditMode]
public class MapImageCamera : MonoBehaviour
{
  public Material material;

  void OnRenderImage(RenderTexture sourceTexture, RenderTexture destinationTexture)
  {
    Graphics.Blit(sourceTexture, destinationTexture, this.material);
  }

  public void UseDepthShader()
  {
    this.material = new Material(Shader.Find("Custom/S_DepthMapImage"));
  }

  public void UseWeightedDepthShader()
  {
    this.material = new Material(Shader.Find("Custom/S_WeightedDepthMapImage"));
  }
}
