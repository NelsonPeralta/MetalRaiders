Shader "EGA/Particles/Blend_LinePath"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_Noise("Noise", 2D) = "white" {}
		_Color("Color", Color) = (0.5,0.5,0.5,1)
		_Emission("Emission", Float) = 2
		_Opacity("Opacity", Range( 0 , 1)) = 1
		[Toggle(_USEDEPTH_ON)] _Usedepth("Use depth?", Float) = 0
		_Depthpower("Depth power", Float) = 1
		_LenghtSet1ifyouuseinPS("Lenght(Set 1 if you use in PS)", Range( 0 , 1)) = 0
		_PathSet0ifyouuseinPS("Path(Set 0 if you use in PS)", Range( 0 , 1)) = 0
		[HideInInspector] _tex4coord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back Cull Off
		CGPROGRAM
		#include "UnityCG.cginc"
		//#pragma target 3.0
		#pragma shader_feature _USEDEPTH_ON
		#pragma surface surf Standard alpha:fade keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float4 uv_tex4coord;
			float4 vertexColor : COLOR;
			float4 screenPos;
		};

		uniform sampler2D _MainTex;
		uniform float _PathSet0ifyouuseinPS;
		uniform float _LenghtSet1ifyouuseinPS;
		uniform sampler2D _Noise;
		uniform float4 _Color;
		uniform float _Emission;
		uniform sampler2D _CameraDepthTexture;
		uniform float _Depthpower;
		uniform float _Opacity;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float temp_output_98_0 = (2.5 + (( _PathSet0ifyouuseinPS + i.uv_tex4coord.z ) - 0.0) * (1.0 - 2.5) / (1.0 - 0.0));
			float temp_output_102_0 = (1.0 + (( i.uv_tex4coord.w * _LenghtSet1ifyouuseinPS ) - 0.0) * (0.0 - 1.0) / (1.0 - 0.0));
			float clampResult107 = clamp( ( ( ( temp_output_98_0 * temp_output_98_0 * temp_output_98_0 * temp_output_98_0 ) * i.uv_tex4coord.x ) - temp_output_102_0 ) , 0.0 , 1.0 );
			float2 appendResult109 = (float2(( clampResult107 * rsqrt( (1.0 + (temp_output_102_0 - 0.0) * (0.001 - 1.0) / (1.0 - 0.0)) ) ) , i.uv_tex4coord.y));
			float2 clampResult85 = clamp( appendResult109 , float2( 0,0 ) , float2( 1,1 ) );
			float4 tex2DNode23 = tex2D( _MainTex, clampResult85 );
			float4 tex2DNode24 = tex2D( _Noise, clampResult85 );
			o.Emission = ( ( tex2DNode23 * tex2DNode24 * _Color * i.vertexColor ) * _Emission ).rgb;
			float temp_output_26_0 = ( tex2DNode23.a * tex2DNode24.a * _Color.a * i.vertexColor.a );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth39 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD( ase_screenPos ))));
			float distanceDepth39 = abs( ( screenDepth39 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _Depthpower ) );
			float clampResult38 = clamp( distanceDepth39 , 0.0 , 1.0 );
			#ifdef _USEDEPTH_ON
				float staticSwitch29 = ( temp_output_26_0 * clampResult38 );
			#else
				float staticSwitch29 = temp_output_26_0;
			#endif
			o.Alpha = ( staticSwitch29 * _Opacity );
		}

		ENDCG
	}
}