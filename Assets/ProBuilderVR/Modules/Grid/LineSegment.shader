Shader "Unlit/pb_LineSegmentShader"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Scale ("Scale", float) = .1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
			};

			sampler2D _MainTex;
			float _Scale;
			float4 _Color;

			v2f vert (appdata v)
			{
				v2f o;

				float4 clip_a = mul(UNITY_MATRIX_MVP, v.vertex);
				float4 clip_b = mul(UNITY_MATRIX_MVP, v.normal);

				float2 dir = clip_b.xy - clip_a.xy;
				float2 perp = float2(-dir.y, dir.x);

				clip_a.xy += normalize(perp) * v.uv.z * _Scale;

				o.vertex = clip_a;
				o.uv.xy = v.uv.xy;

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return _Color;
			}
			ENDCG
		}
	}
}
