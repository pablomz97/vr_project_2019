Shader "Custom/S_Emissive"
{
  Properties
  {
    _ColorTint("Color Tint", Color) = (1, 1, 1, 1)
    _MainTex("Base (RGB)", 2D) = "white" {}
    _BumpMap("Normal Map", 2D) = "bump" {}
    _EmMap("Emissive Map", 2D) = "black" {}
    _SmMap("Smoothness Map", 2D) = "black" {}
    _AoMap("Occlusion Map", 2D) = "white" {}
    _EmInt("Emissive Intensity", Range(0.0, 32.0)) = 1.0
    _EmColor("Emissive Color", Color) = (1, 1, 1, 1)

  }
    SubShader{

      // Extracts information for lightmapping, GI (emission, albedo, ...)
      // This pass is not used during regular rendering.
      Pass
      {
        Name "META"
        Tags{ "LightMode" = "Meta" }

        Cull Off

        CGPROGRAM
        #pragma vertex vert_meta
        #pragma fragment frag_meta

        #pragma shader_feature _EMISSION
        #pragma shader_feature _METALLICGLOSSMAP

        #include "UnityStandardMeta.cginc"
        ENDCG
      }

    Tags{ "RenderType" = "Opaque" }

    CGPROGRAM
#pragma surface surf Standard fullforwardshadows nometa

    struct Input {

    float4 color : Color;
    float2 uv_MainTex;
    float2 uv_BumpMap;
    float3 viewDir;

  };

  float4 _ColorTint;
  sampler2D _MainTex;
  sampler2D _BumpMap;
  sampler2D _EmMap;
  sampler2D _SmMap;
  sampler2D _AoMap;
  float4 _EmColor;
  float _EmInt;

  void surf(Input IN, inout SurfaceOutputStandard o)
  {
    float2 uv_d = IN.uv_MainTex.xy;
    float2 uv = IN.uv_BumpMap.xy;
    IN.color = _ColorTint;
    o.Smoothness = saturate(tex2D(_SmMap, uv).r);
    o.Albedo = tex2D(_MainTex, uv_d).rgb * IN.color;
    o.Normal = UnpackNormal(tex2D(_BumpMap,uv));
    o.Occlusion = tex2D(_AoMap, uv).rgb;
    o.Emission = _EmInt * _EmColor.rgb * tex2D(_EmMap, uv).rgb;


  }

  ENDCG
  }
    FallBack "Diffuse"
}﻿
}
