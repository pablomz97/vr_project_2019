Shader "Custom/S_DepthShader"
{
  Properties {}

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

      fixed4 fragmentShader(ProcessedVertex processedVertex): COLOR {
        float depth = tex2D(_CameraDepthTexture, processedVertex.texturePosition).r;

        return depth;
      }

      ENDCG
    }
  }
}
