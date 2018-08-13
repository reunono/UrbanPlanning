Shader "Unlit/Matcap"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_MatcapTex ("Matcap", 2D) = "white" {}
		_LightColor("Light Color", Color) = (1,1,1,1)
		_LightIntensity("Light Intensity", Float) = 1.0
		_MatcapLightTex("Matcap", 2D) = "black" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD0;
				float2 uv : TEXCOORD1;
			};

			float4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _MatcapTex;
			float4 _LightColor;
			float _LightIntensity;
			sampler2D _MatcapLightTex;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = mul(UNITY_MATRIX_MV, float4(v.normal, 0.0)).xyz;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 normal = normalize(i.normal);
				float2 matcap = normal.xy * 0.5 + 0.5;
				return tex2D(_MainTex, i.uv) * tex2D(_MatcapTex, matcap) * _Color + tex2D(_MatcapLightTex, matcap) * _LightColor * _LightIntensity;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
