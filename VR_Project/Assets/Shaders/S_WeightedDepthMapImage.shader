Shader "Custom/S_WeightedDepthMapImage"
{
  SubShader {
    Tags {
      "Queue" = "Transparent"
    }

    Pass {
      CGPROGRAM

      #include "UnityCG.cginc"
      #pragma fragment fragmentShader
      #pragma vertex vertexShader

      uniform sampler2D _CameraDepthTexture;
      uniform sampler2D _MainTex;

      struct Vertex {
        float4 position: POSITION;
        float4 texturePosition: TEXCOORD0;
      };

      struct ProcessedVertex {
        float4 position: SV_POSITION;
        float2 texturePosition: TEXCOORD0;
      };

      ProcessedVertex vertexShader(Vertex vertex) {
        ProcessedVertex processedVertex;

        processedVertex.position = UnityObjectToClipPos(vertex.position);
        processedVertex.texturePosition = ComputeScreenPos(processedVertex.position);

        return processedVertex;
      }

      float4 fragmentShader(ProcessedVertex processedVertex): COLOR {
        float depth = tex2D(_CameraDepthTexture, processedVertex.texturePosition).r;

        return pow(depth * 10, 3) * tex2D(_MainTex, processedVertex.texturePosition);
      }

      ENDCG
    }
  }
}
