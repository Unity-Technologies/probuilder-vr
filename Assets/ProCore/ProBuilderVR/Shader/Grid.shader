Shader "Unlit/Grid"
{
	Properties
	{
		_Falloff ("Falloff", float) = .7
		_Step("Step", float) = 1.
	}
	
	SubShader
	{
		Tags { "Queue" = "Overlay+5000" "RenderType"="Transparent" "PerformanceChecks"="False" }
		// Tags { "RenderType"="Transparent" }
		ZTest LEqual
		ZWrite On
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			float _Falloff;
			float _Step;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = v.color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = i.color;;
				float d = distance(i.uv.xy, fixed2(.5,.5)) * 2;
				col.a = smoothstep(1, _Falloff, d);
				return col;
			}
			ENDCG
		}
	}
}
