Shader "Unlit/Guide"
{
	Properties
	{
	}
	
	SubShader
	{
		Tags { "Queue" = "Overlay+5001" "RenderType"="Transparent" "PerformanceChecks"="False" }
		ZTest LEqual
		ZWrite On
		Cull Off
		// Blend OneMinusDstColor One
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
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

		
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = i.color;
				return col;
			}
			ENDCG
		}
	}
}
