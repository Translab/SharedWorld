#pragma enable_d3d11_debug_symbols

//Normal map info
uniform sampler2D _NormalMap;
uniform half4 _NormalMap_ST;
uniform float _NormalStrength;

uniform float _CustomRenderInFront;
uniform float _PerfectReflection;

uniform float _YFlipOverride;
uniform float _XFlipOverride;

uniform float _Forward = 0;

//Mask texture, used for cutout effects
uniform sampler2D _AlphaTexture;
uniform float _Alpha;
uniform float _Mask = 1;


//Textures for both eyes. If non-stereo rendering is being employed, only the right channel is used.
uniform sampler2D _LeftEyeTexture;
uniform sampler2D _RightEyeTexture;
uniform float4 _LeftEyeTexture_TexelSize;
uniform float4 _RightEyeTexture_TexelSize;

//Offsets for both eyes. This defines where, in normalized screen-space coordinates, the Custom Renderer will actually render.
//X and Y are used for positioning, while Z and W define the size of the renderer.
uniform float4 _LeftDrawPos;
uniform float4 _RightDrawPos;

uniform float _VR;

//Returns the result of the Custom Renderer portion. 
float4 customRenderResult(float2 alphaUV, float2 sUV, float3 normal) {
	
	fixed4 alpha = tex2D(_AlphaTexture, alphaUV);
	if (!_Mask) {
		alpha = 0;
	}

#if UNITY_SINGLE_PASS_STEREO
	if (_VR) {
		//In single pass stereo rendering, the screenspace coordinates must be divided in half to account for the doublewidth render.
		_LeftDrawPos /= float4(2, 1, 2, 1);
		_RightDrawPos /= float4(2, 1, 2, 1);
		//Likewise, the right eye must be shifted to the right by half the width of the final render so that it is on the right part of the screen.
		_RightDrawPos.x += 0.5;
	}
#endif

	//Fix the coordinates so that they are accurate to screenspace.... space.
	_LeftDrawPos.xy = (_LeftDrawPos.xy) / _LeftDrawPos.zw;
	_RightDrawPos.xy = (_RightDrawPos.xy) / _RightDrawPos.zw;

	//On some systems, for some reason Unity fails to accurately detect if the UV is flipped. This override patches that.
	if (_YFlipOverride)
		sUV.y = 1 - sUV.y;

	//Init the output
	fixed4 col = fixed4(0.0, 0.0, 0.0, 0.0);

	//Determine output distortion from normal map
	sUV.x -= normal.r / 10 * _NormalStrength;
	sUV.y += normal.g / 10 * _NormalStrength;

#if UNITY_SINGLE_PASS_STEREO
	if (_VR) {
		if (unity_StereoEyeIndex == 0)
		{
			sUV = (sUV / _LeftDrawPos.zw) - _LeftDrawPos.xy;
			col = tex2D(_LeftEyeTexture, sUV);
		}
		else {
			
			sUV = (sUV / _RightDrawPos.zw) - _RightDrawPos.xy;
			col = tex2D(_RightEyeTexture, sUV);
		}
	}
	else {
		sUV = (sUV / _RightDrawPos.zw) - _RightDrawPos.xy;
		col = tex2D(_RightEyeTexture, sUV);
	}


#else
	if (_VR) {
		if (unity_StereoEyeIndex == 0)
		{
			sUV = (sUV / _LeftDrawPos.zw) - _LeftDrawPos.xy;
			col = tex2D(_LeftEyeTexture, sUV);
		}
		else
		{
			sUV = (sUV / _RightDrawPos.zw) - _RightDrawPos.xy;
			col = tex2D(_RightEyeTexture, sUV);
		}
	}
	else {
		sUV = (sUV / _RightDrawPos.zw) - _RightDrawPos.xy;
		col = tex2D(_RightEyeTexture, sUV);
	}
#endif
	//Alpha from mask, multiplied by alpha value
	col.a = max(0, (1 - alpha.r) - (1 - _Alpha));

	return col;
}