
Shader"Portal/PortalShader" {
	///This shader is fairly simple, but some choices do warrant some explanation. Essentially, the shader is in two parts.
	///One part is the Standard Surface shader portion of the shader. This allows you to define a 'standard' surface shader
	///channel, which is useful when doing some special effects- This is typically more useful for MirrorKit, but it has 
	///uses in PortalKit as well, should you be trying to do any effects such as portals that fade into walls without 
	///requiring a duplicate mesh. In MirrorKit, this is very useful as it allows for surfaces to have dirt on them, have
	///physically-defined properties, use reflective bits as surface details, and more.

	/*
	Essentially, the model typically looks like this.

	   +--------------------+
       |  Custom Renderer   |
     +-------------------+  |
     |                   |  |
     | Surface Portion   |  |
     |                   |  |
     |                   |  |  
     |                   |  |
     |                   +--+
     |                   |
     +-------------------+

	 The Custom Renderer portion can be viewed through the Surface portion of the shader. That is to say, a player can only
	 view the Custom Renderer protion on parts of the Surface that are transparent enough to do so. As such, if the Custom 
	 Renderer portion is not in front, the final output alpha is determined by whichever of these two layers has the maximum
	 alpha value, as the values fall through the first and into the second.
	*/
	Properties{
		///Custom Renderer block

		//Is the Custom Renderer in front of, or behind, the surface shader surface?
		[Toggle] _CustomRenderInFront("Custom Render in front", Int) = 1
		//Is the Custom Renderer portion affected by the Alpha of the surface shader?
		[Toggle] _PerfectReflection("This surface is a pure Renderer", Int) = 1
		//The Mask texture for this Renderer. White/black define alpha of the Custom Renderer portion, with darker being more visible and lighter being less.
		_AlphaTexture("Alpha Texture", 2D) = "white"{}
		//The above value is multiplied by _Alpha to get the output alpha value for that fragment for the Custom Renderer portion.
		_Alpha("Custom Renderer Alpha", Range(0,1)) = 1
	    //This is a little counterintuitive, but bear with me. Portals and mirrors are not simply static images. Rather, they are defined by
		//the light that they are modifying. As such, the output value is not direct, but instead modifies the Emission value of the output
	    //fragment. This allows for sensible blending in later steps.
		_Emission("Custom Renderer Tint", Color) = (1,1,1,1)
		//The normal map distorts the Custom Renderer. _NormalStrength modifies how severely this is done. To disable, set it to 0.
		_NormalStrength("Custom Renderer Distort Strength", Range(-2,2)) = 0
	    
		///Surface Shader block

		//The Albedo for this surface
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		//The Color modifier for this surface
		_Color("Albedo Surface Color", Color) = (1,1,1,1)
	    //Normal Map- also modifies distortion of the output image (see _NormalStrength)
		_NormalMap("Normal Map", 2D) = "white" {}
		//Gloss and metallic maps, standard stuff.
		_Glossiness("Smoothness", Range(0,1)) = 0.0
		_Metallic("Metallic", Range(0,1)) = 0.0

	    //Uncommonent to preview contents
		//_LeftEyeTexture("Left Eye Texture", 2D) = "white" {}
		//_RightEyeTexture("Right Eye Texture", 2D) = "white" {}
	    //If you need them, inputs can be freely added as with any other surface shader
	}

		
	SubShader{
	    //As the Custom Renderers may be transparent, they exist in the transparent queue.
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "PreviewType" = "Plane" }
		//Custom Renderers will always be rendered, regardless of quality settings.
		LOD 100
	    //Typical blending
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite On
	    //Z-testing is typically enabled, but it may be disabled in special cases. The typical example would be if one had their head 
	    //halfway inside of a portal. In this instance, the portal must render infront of everything as to not have the wall behind
		//the portal poke through and be visible.
		ZTest[_ZTest]
		Cull back

		Name"PortalShader"

		CGPROGRAM

//#define UNLIT

// Physically based Standard lighting model, and enable shadows on all light types


//#pragma surface surf NoLighting fullforwardshadows alpha:fade
#include "Assets/SKStudios/Common/Resources/Materials/Visible Effects/CustomRenderer.cginc"
#pragma surface surf Standard fullforwardshadows alpha:fade

//Target 3.0 has some issues with the way that realtime lighting is blended on the surface. If you want 3.0 benefits and don't mind this, enable it.
#pragma target 3.0

	sampler2D _MainTex;

	sampler2D _CameraDepthTexture;

	half _Glossiness;
	half _Metallic;
	fixed4 _Color;
	fixed4 _Emission;


	struct Input
	{
		float2 uv_MainTex;
		float2 uv_AlphaTexture;
		float4 screenPos;
		float eyeDepth;
	};


	//Surface portion of the shader, which calls the Custom Renderer portion and blends them appropriately based on settings.
	void surf(Input IN, inout SurfaceOutputStandard o)
	{
		// Albedo comes from a texture tinted by color
		float2 sUV = IN.screenPos.xy / IN.screenPos.w;
		//The Normal data is handled early to pass to the Custom Render portion.
		o.Normal = UnpackNormal(tex2D(_NormalMap, TRANSFORM_TEX(IN.uv_MainTex, _NormalMap))) /** (1 - customRenderColor.a)*/;
		float4 customRenderColor = customRenderResult(IN.uv_AlphaTexture, sUV, o.Normal);
		float4 Albedo = tex2D(_MainTex, IN.uv_MainTex) * _Color;

		//Alpha blending
		if (_CustomRenderInFront) {
			if (_PerfectReflection)
				o.Albedo = Albedo * (1 - customRenderColor.a);
			else { o.Albedo = Albedo; }
		}
		else { o.Albedo = Albedo.rgb * Albedo.a;}

		if (_CustomRenderInFront) { o.Emission = (customRenderColor * _Emission) * customRenderColor.a;}
		else { o.Emission = (customRenderColor * _Emission) * (1 - Albedo.a); }
			
		//o.Emission = ((customRenderColor * _Emission ) * customRenderColor.a) * (1 - Albedo.a);
		// * (Albedo * (1 -Albedo.a)

		if (_CustomRenderInFront) {
			float alpha = Albedo.a;
			
			if (customRenderColor.a < Albedo.a) { 
				alpha = customRenderColor.a;
			}	
			o.Alpha = alpha;
		}
		else{ 
			o.Alpha = customRenderColor.a;
		}
			

		o.Metallic = _Metallic;
		o.Smoothness = _Glossiness;
	}
	ENDCG

	}



		Fallback"Diffuse"
}

