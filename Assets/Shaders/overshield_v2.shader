// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "shield v2"
{
	Properties
	{
		_GlowStrength("Glow Strength", Float) = 1
		_GlowColor("Glow Color", Color) = (1,1,1,0)
		_VertexDisplacement("Vertex Displacement", Float) = 0
		_DisplacementTextureStrength("Displacement Texture Strength", Float) = 0
		_DepthFade("Depth Fade", Range( 0 , 10)) = 0
		_FresnelGradientPosition("Fresnel Gradient Position", Range( 0 , 1)) = 0
		_FresnelOpacity("Fresnel Opacity", 2D) = "white" {}
		_DisplacementTexture("Displacement Texture", 2D) = "black" {}
		_FresnelTexturePanSpeed("Fresnel Texture Pan Speed", Float) = 0
		_Alpha("Alpha", Range( 0 , 1)) = 1
		_DisplacementTexturePanSpeed("Displacement Texture Pan Speed", Float) = 0
		_Contrast("Contrast", Range( 1 , 99)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 4.6
		#pragma multi_compile __ FIRST_PERSON
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			float4 screenPos;
			float2 uv_texcoord;
		};


		float4x4 _CustomProjectionMatrix;

		uniform float4 _GlowColor;
		uniform float _GlowStrength;
		uniform float _FresnelGradientPosition;
		uniform sampler2D _CameraDepthTexture;
		uniform float _DepthFade;
		uniform sampler2D _FresnelOpacity;
		uniform float _FresnelTexturePanSpeed;
		uniform float4 _FresnelOpacity_ST;
		uniform float _Alpha;
		uniform float _Contrast;
		uniform float _VertexDisplacement;
		uniform float _DisplacementTextureStrength;
		uniform sampler2D _DisplacementTexture;
		uniform float _DisplacementTexturePanSpeed;
		uniform float4 _DisplacementTexture_ST;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float2 temp_cast_0 = (_DisplacementTexturePanSpeed).xx;
			float2 uv_DisplacementTexture = v.texcoord.xy * _DisplacementTexture_ST.xy + _DisplacementTexture_ST.zw;
			float2 panner102 = ( uv_DisplacementTexture + 1.0 * _Time.y * temp_cast_0);
			v.vertex.xyz += ( ase_vertexNormal * ( _VertexDisplacement + ( _DisplacementTextureStrength * tex2Dlod( _DisplacementTexture, float4( panner102, 0, 0.0) ).r ) ) );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Emission = ( _GlowColor * _GlowStrength ).rgb;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNDotV61 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode61 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNDotV61, 1.0 ) );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth84 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD(ase_screenPos))));
			float distanceDepth84 = abs( ( screenDepth84 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( max( _DepthFade , 0.0001 ) ) );
			float2 temp_cast_1 = (_FresnelTexturePanSpeed).xx;
			float2 uv_FresnelOpacity = i.uv_texcoord * _FresnelOpacity_ST.xy + _FresnelOpacity_ST.zw;
			float2 panner93 = ( uv_FresnelOpacity + 1.0 * _Time.y * temp_cast_1);
			o.Alpha = pow( saturate( ( pow( ( 1.0 - abs( ( saturate( fresnelNode61 ) - ( 1.0 - _FresnelGradientPosition ) ) ) ) , 5.0 ) * saturate( distanceDepth84 ) * tex2D( _FresnelOpacity, panner93 ).r * _Alpha ) ) , _Contrast );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float4 screenPos : TEXCOORD3;
				float3 worldNormal : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
				return o;
			}
			fixed4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				surfIN.screenPos = IN.screenPos;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=14201
9;207;1266;523;-201.4146;-76.02249;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;46;-1240.806,259.8137;Float;False;1432.116;1299.619;Opacity;7;109;108;55;89;107;87;47;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;47;-1204.951,308.025;Float;False;916.704;445.1089;Fresnel;8;80;79;78;72;77;74;73;61;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;73;-1169.498,591.5437;Float;False;Property;_FresnelGradientPosition;Fresnel Gradient Position;5;0;Create;0;0.1952019;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;61;-1136.803,368.7332;Float;True;Tangent;4;0;FLOAT3;0,0,0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;3;FLOAT;1.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;77;-877.5938,385.5518;Float;False;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;90;-1218.707,1024.671;Float;False;915.6835;548.0159;Texture;5;93;92;91;96;98;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;87;-1215.12,826.4933;Float;False;900.9595;165.8126;Depth;4;84;86;85;88;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;45;223.1705,680.8651;Float;False;972.3036;844.8813;Vertex Displacement;11;104;103;102;101;100;99;6;7;8;106;105;;1,1,1,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;74;-879.08,459.9442;Float;False;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;91;-1168.901,1079.843;Float;True;Property;_FresnelOpacity;Fresnel Opacity;6;0;Create;None;cd460ee4ac5c1e746b7a734cc7cc64dd;False;white;Auto;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;85;-1176.711,903.5482;Float;False;Property;_DepthFade;Depth Fade;4;0;Create;0;1.000649;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;99;232.7345,1089.518;Float;True;Property;_DisplacementTexture;Displacement Texture;7;0;Create;None;cd460ee4ac5c1e746b7a734cc7cc64dd;False;black;Auto;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;72;-717.395,378.5125;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;78;-579.4205,380.5646;Float;False;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;92;-1160.057,1276.668;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;98;-1155.078,1408.372;Float;False;Property;_FresnelTexturePanSpeed;Fresnel Texture Pan Speed;8;0;Create;0;0.24;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;100;241.5795,1286.343;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;101;246.5585,1418.047;Float;False;Property;_DisplacementTexturePanSpeed;Displacement Texture Pan Speed;10;0;Create;0;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;86;-896.3537,873.4164;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0001;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;84;-764.1315,880.9071;Float;False;True;1;0;FLOAT;1.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;79;-583.5208,450.1723;Float;False;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;102;632.764,1187.134;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;1,1;False;1;FLOAT;1.0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;93;-768.8721,1177.459;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;1,1;False;1;FLOAT;1.0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;96;-586.1685,1102.341;Float;True;Property;_TextureSample0;Texture Sample 0;8;0;Create;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;107;-130.1357,831.4215;Float;False;Property;_Alpha;Alpha;9;0;Create;1;0.9831;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;80;-434.3971,531.99;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;5.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;88;-524.7617,873.3138;Float;False;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;106;241.0876,890.1263;Float;False;Property;_DisplacementTextureStrength;Displacement Texture Strength;3;0;Create;0;0.01;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;103;815.4675,1112.016;Float;True;Property;_TextureSample1;Texture Sample 1;8;0;Create;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;105;508.7133,1028.256;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;89;-171.9467,487.6585;Float;False;4;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;44;-747.2972,-136.4097;Float;False;425.3488;333.6756;Glow;3;4;5;3;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;8;250.8292,984.3762;Float;False;Property;_VertexDisplacement;Vertex Displacement;2;0;Create;0;0.0138418;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;104;591.1094,912.3944;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;55;-120.6897,397.2966;Float;False;1;0;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;108;-197.3092,671.7441;Float;False;Property;_Contrast;Contrast;11;0;Create;1;1.154861;1;99;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-719.3682,98.66587;Float;False;Property;_GlowStrength;Glow Strength;0;0;Create;1;30;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;7;261.6769,736.6121;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;3;-726.7085,-80.32211;Float;False;Property;_GlowColor;Glow Color;1;0;Create;1,1,1,0;1,0.6,0,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;861.6937,754.8698;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0.0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode;109;63.43909,452.4402;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-462.5378,-14.50395;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;966.936,5.140251;Float;False;True;6;Float;ASEMaterialInspector;0;0;Standard;shield v2;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;Back;0;0;False;0;0;Transparent;0.5;True;True;0;False;Transparent;Transparent;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;0;11.05;0;25;True;0;True;2;SrcAlpha;OneMinusSrcAlpha;0;Zero;Zero;OFF;OFF;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;0;-1;0;0;0;False;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;77;0;61;0
WireConnection;74;0;73;0
WireConnection;72;0;77;0
WireConnection;72;1;74;0
WireConnection;78;0;72;0
WireConnection;92;2;91;0
WireConnection;100;2;99;0
WireConnection;86;0;85;0
WireConnection;84;0;86;0
WireConnection;79;0;78;0
WireConnection;102;0;100;0
WireConnection;102;2;101;0
WireConnection;93;0;92;0
WireConnection;93;2;98;0
WireConnection;96;0;91;0
WireConnection;96;1;93;0
WireConnection;80;0;79;0
WireConnection;88;0;84;0
WireConnection;103;0;99;0
WireConnection;103;1;102;0
WireConnection;105;0;106;0
WireConnection;105;1;103;1
WireConnection;89;0;80;0
WireConnection;89;1;88;0
WireConnection;89;2;96;1
WireConnection;89;3;107;0
WireConnection;104;0;8;0
WireConnection;104;1;105;0
WireConnection;55;0;89;0
WireConnection;6;0;7;0
WireConnection;6;1;104;0
WireConnection;109;0;55;0
WireConnection;109;1;108;0
WireConnection;4;0;3;0
WireConnection;4;1;5;0
WireConnection;0;2;4;0
WireConnection;0;9;109;0
WireConnection;0;11;6;0
ASEEND*/
//CHKSM=0746B17C5F6D8898DC783330E1489A976B65F644