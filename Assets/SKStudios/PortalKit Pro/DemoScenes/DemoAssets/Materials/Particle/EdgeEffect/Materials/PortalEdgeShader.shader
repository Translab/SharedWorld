// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/PortalEdgeShader"
{
	Properties
	{
		_TintColor("Tint Color", Color) = (0.5, 0.5, 0.5, 0.5)
		_PortalEdgeEffect("Texture", 2DArray) = "white"{}
		_EffectSpeed("Effect Speed", float) = 1
		_EdgeArrayDepth("Edge array depth", int) = 100
	}

		CGINCLUDE

#include "UnityCG.cginc"

	UNITY_DECLARE_TEX2DARRAY(_PortalEdgeEffect);
	float4 _MainTex_ST;
	fixed4 _TintColor;
	float _EffectSpeed;
	float _EdgeArrayDepth;
	struct appdata_t
	{
		float4 position : POSITION;
		float4 texcoord : TEXCOORD0;
		fixed4 color : COLOR;
	};

	struct v2f
	{
		float4 position : SV_POSITION;
		float2 texcoord : TEXCOORD0;
		fixed4 color : COLOR;
		UNITY_FOG_COORDS(1)
	};

	v2f vert(appdata_t v)
	{
		v2f o;
		o.position = UnityObjectToClipPos(v.position);
		o.texcoord = v.texcoord;
		o.color = v.color;
		UNITY_TRANSFER_FOG(o, o.vertex);
		return o;
	}

	
	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 col = i.color * _TintColor * 
			float4(UNITY_SAMPLE_TEX2DARRAY(_PortalEdgeEffect, 
				float3(i.texcoord, _Time.y * 10 * _EffectSpeed) % _EdgeArrayDepth).rgb, 1);
		UNITY_APPLY_FOG_COLOR(i.fogCoord, col, (fixed4)0);
		return col;
	}

		ENDCG

		SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

			Blend SrcAlpha One
			Cull Off Lighting Off ZWrite Off Fog{ Mode Off }

			Pass
		{
			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_particles
#pragma multi_compile_fog
			ENDCG
		}
	}
}