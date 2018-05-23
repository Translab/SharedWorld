Shader "Ehsan/NewUnlitShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Sensitivity ("Sensitivity", Range(0.0,20.0)) = 1.0
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
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			uniform float4 target1;
			uniform float4 target2;
			float _Sensitivity;
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 worldpos : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldpos = mul(unity_ObjectToWorld, v.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float d1 = sqrt(length(target1.xyz - i.worldpos.xyz));

				float d2 = sqrt(length(target2.xyz - i.worldpos.xyz));

				d1 = pow(d1,2.0);
				d2 = pow(d2,2.0);
				//d+= length(target2.xyz - i.worldpos.xyz);
				fixed4 col = tex2D(_MainTex, float2(0.0, 1.0/(d1*_Sensitivity))+float2(0.0, 1.0/(d2*_Sensitivity)));
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
