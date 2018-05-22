// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Portal/PurePortal"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		//The Mask texture for this Renderer. White/black define alpha of the Custom Renderer portion, with darker being more visible and lighter being less.
		_AlphaTexture("Alpha Texture", 2D) = "white"{}
		//The above value is multiplied by _Alpha to get the output alpha value for that fragment for the Custom Renderer portion.
		_Alpha("Custom Renderer Alpha", Range(0,1)) = 1
	}

		SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		ZTest[_ZTest]
		ZWrite On
		Cull Back
		Pass
	{
		CGPROGRAM
#include "Assets/SKStudios/Common/Resources/Materials/Visible Effects/CustomRenderer.cginc"
#pragma vertex vert
#pragma fragment frag
#pragma target 2.0

#include "UnityCG.cginc"

	struct appdata {
		float4 vertex : POSITION;
		float2 uv:TEXCOORD0;
	};

	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
		float4 scrPos : TEXCOORD1;
	};

	v2f vert(appdata v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		o.scrPos = ComputeScreenPos(o.pos);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		i.scrPos.xy /= i.scrPos.w;
	return customRenderResult(i.uv, i.scrPos.xy, 0);
	}
		ENDCG
	}
	}
}
