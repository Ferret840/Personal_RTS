Shader "CameraEffect/DimensionChangeEffect"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
    _Oscillater("Oscillation value for PingPong", Float) = 0.0
    _EffectAmplitude("How large the effect moves vertically", Range(-1, 1)) = 10
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
      float _Oscillater;
      float _EffectAmplitude;

			fixed4 frag (v2f inP) : SV_Target
			{
        //Create sin-wave effect
				fixed4 col = tex2D(_MainTex, inP.uv + float2(0, sin( inP.vertex.x/2 + _Time[1] * 10 ) * (_EffectAmplitude) * _Oscillater) );
        //lerp(0, 1, sin(_Time[1] - _StartTime))
				
				return col;
			}
			ENDCG
		}
	}
}
