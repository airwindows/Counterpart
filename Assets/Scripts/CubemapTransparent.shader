﻿Shader "Skybox/CubemapTransparent" {
Properties {
    [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
    _Rotation ("Rotation", Range(0, 360)) = 0
    [NoScaleOffset] _Tex ("Cubemap   (HDR)", Cube) = "grey" {}
}
 
SubShader {
    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
    Cull Off ZWrite Off
    Blend One One
 
    Pass {
     
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
 
        #include "UnityCG.cginc"
 
        samplerCUBE _Tex;
        half4 _Tex_HDR;
        half4 _Tint;
        half _Exposure;
        float _Rotation;
 
        float4 RotateAroundYInDegrees (float4 vertex, float degrees)
        {
            float alpha = degrees * UNITY_PI / 180.0;
            float sina, cosa;
            sincos(alpha, sina, cosa);
            float2x2 m = float2x2(cosa, -sina, sina, cosa);
            return float4(mul(m, vertex.xz), vertex.yw).xzyw;
        }
     
        struct appdata_t {
            float4 vertex : POSITION;
        };
 
        struct v2f {
            float4 vertex : SV_POSITION;
            float3 texcoord : TEXCOORD0;
        };
 
        v2f vert (appdata_t v)
        {
            v2f o;
            o.vertex = mul(UNITY_MATRIX_MVP, RotateAroundYInDegrees(v.vertex, _Rotation));
            o.texcoord = v.vertex;
            return o;
        }
 
        fixed4 frag (v2f i) : SV_Target
        {
            half4 tex = texCUBE (_Tex, i.texcoord);
            tex *= _Exposure;
            return tex;
        }
        ENDCG
    }
}  
 
 
Fallback Off
 
}
 