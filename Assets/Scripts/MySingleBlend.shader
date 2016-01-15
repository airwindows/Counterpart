Shader "Hidden/MySingleBlend" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_AccumOrig("AccumOrig", Float) = 1.0
}

    SubShader { 
		ZTest Always Cull Off ZWrite Off
		Pass {
			Blend SrcAlpha One //SrcAlpha OneMinusSrcAlpha //is a smooth blend, One One is bloom city, SrcAlpha One is a hybrid
			ColorMask RGB
		    BindChannels { 
				Bind "vertex", vertex 
				Bind "texcoord", texcoord
			} 
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	
			#include "UnityCG.cginc"
	
			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD;
			};
	
			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD;
			};
			
			float4 _MainTex_ST;
			float _AccumOrig;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}
	
			sampler2D _MainTex;
			
			half4 frag (v2f i) : SV_Target
			{
				return half4(tex2D(_MainTex, i.texcoord).rgb, _AccumOrig );
			}
			ENDCG 
		} 

		Pass {
			Blend One Zero
			ColorMask A
			
		    BindChannels { 
				Bind "vertex", vertex 
				Bind "texcoord", texcoord
			} 
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	
			#include "UnityCG.cginc"
	
			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD;
			};
	
			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD;
			};
			
			float4 _MainTex_ST;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}
	
			sampler2D _MainTex;
			
			half4 frag (v2f i) : SV_Target
			{
				return tex2D(_MainTex, i.texcoord);
			}
			ENDCG 
		}
		
	}

SubShader {
	ZTest Always Cull Off ZWrite Off
	Pass {
		Blend One One //SrcAlpha OneMinusSrcAlpha //is a smooth blend, One One is bloom city, SrcAlpha One is a hybrid
		ColorMask RGB
		SetTexture [_MainTex] {
			ConstantColor (0,0,0,[_AccumOrig])
			Combine texture, constant
		}
	}
	Pass {
		Blend One Zero
		ColorMask A
		SetTexture [_MainTex] {
			Combine texture
		}
	}
}

Fallback off

}
