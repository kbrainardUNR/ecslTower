﻿Shader "Unlit/Striped"
{

	Properties {
		_Color1 ("Color 1", Color) = (0,0,0,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
		_Tiling ("Tiling", Range(1, 500)) = 10
		_Direction ("Direction", Range(0, 1)) = 0
		_Flipped("Flipped", Range(0, 1)) = 0
		_ColorRatio("Color Ratio", Range(0, 1)) = 0.5
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			fixed4 _Color1;
			fixed4 _Color2;
			int _Tiling;
			float _Direction;
			float _Flipped;
			float _ColorRatio;

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

			fixed4 frag (v2f i) : SV_Target
			{
				fixed flip = sign(_Flipped - i.uv.x);
				float colorRat = _ColorRatio - 0.5;
				float pos = lerp(-1 * flip * i.uv.x, i.uv.y, _Direction) * _Tiling;
				fixed value = floor(frac(pos) + 0.5 + colorRat);
				return lerp(_Color1, _Color2, value);
			}
			ENDCG
		}
	}
}
