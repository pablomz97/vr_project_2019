Shader "Custom/S_EdgeGlow"
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
    _RimColor("Rim Color", Color) = (1, 1, 1, 1)
    _RimPower("Rim Power", Range(1.0, 6.0)) = 3.0
    _RimInt("Rim Intensity", Range(0.0, 64.0)) = 1.0
    _PanSpeed("Panning Speed", Vector) = (0, 0, 0, 0)

  }
    SubShader{

    Tags{ "RenderType" = "Opaque" }

    CGPROGRAM
#pragma surface surf Standard fullforwardshadows

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
  float4 _RimColor;
  float _RimPower;
  float _RimInt;
  float _EmInt;
  float2 _PanSpeed;

  void surf(Input IN, inout SurfaceOutputStandard o)
  {
    float2 uv_d = IN.uv_MainTex.xy + frac(_Time.y * _PanSpeed.xy);
    float2 uv = IN.uv_BumpMap.xy;
    IN.color = _ColorTint;
    o.Smoothness = saturate(tex2D(_SmMap, uv).r);
    o.Albedo = tex2D(_MainTex, uv_d).rgb * IN.color;
    o.Normal = UnpackNormal(tex2D(_BumpMap,uv));
    o.Occlusion = tex2D(_AoMap, uv).rgb;
    half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
    o.Emission = _RimInt * _RimColor.rgb * pow(rim, _RimPower) + _EmInt * tex2D(_EmMap, uv).rgb;


  }
  ENDCG
  }
    FallBack "Diffuse"
}﻿
}
