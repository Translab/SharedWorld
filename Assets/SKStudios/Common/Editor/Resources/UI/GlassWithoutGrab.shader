// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Similar to regular FX/Glass/Stained BumpDistort shader
// from standard Effects package, just without grab pass,
// and samples a texture with a different name.

Shader "FX/Glass/Stained BumpDistort (no grab)" {
Properties {
	_TintAmt ("Tint Amount", Range(0,1)) = 0.1
	_TextureAmt("Texture Amount", Range(0,2)) = 0.1
	_TextureBaseline("Texture Baseline", Range(0,1)) = 0.1
	_Color ("Baseline Color (RGB)", Color) = (0, 0, 0, 0)
	_Texture("Glass Texture", 2D) = "white"
}

Category {

	// We must be transparent, so other objects are drawn before this one.
	Tags { "Queue"="Transparent" "RenderType"="Opaque" }

	SubShader {

		Pass {
			Name "BASE"
			Tags { "LightMode" = "Always" }
			
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

struct appdata_t {
	float4 vertex : POSITION;
	float2 uv: TEXCOORD0;
	float2 texcoord: TEXCOORD1;
};

struct v2f {
	float4 vertex : POSITION;
	float2 uv: TEXCOORD0;
	float4 uvgrab : TEXCOORD1;
};



half4 _Texture_ST;
float _YFlipOverride;

v2f vert (appdata_t v)
{
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
    float scale;
#if UNITY_UV_STARTS_AT_TOP
	scale = -1;
#else
	scale = 1;
#endif
	o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
	o.uvgrab.zw = o.vertex.zw;

	o.uv = TRANSFORM_TEX(v.uv, _Texture);
	return o;
}

//Ground glass texture vars
sampler2D _Texture;
half _TextureAmt;
half _TextureBaseline;
float4 _Color;

//Color tint stuff
half _TintAmt;

//Blur and distortion
sampler2D _GrabBlurTexture;
float4 _GrabBlurTexture_TexelSize;
sampler2D _BumpMap;


half4 frag (v2f i) : SV_Target
{
	half4 texSample = tex2D(_Texture, i.uvgrab * (_ScreenParams.xy / 200));
	half4 blurSample = saturate(tex2Dproj(_GrabBlurTexture, UNITY_PROJ_COORD(i.uvgrab)) * 1.5);
	half4 col = lerp(_Color, blurSample, saturate((texSample.r * _TextureAmt) + (1 - _TextureBaseline)));
	//col = saturate(lerp (col, _Color, _TintAmt));
	return col ;
}
ENDCG
		}
	}

}

}
