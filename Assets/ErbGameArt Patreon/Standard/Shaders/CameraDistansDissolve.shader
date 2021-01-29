// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "EGA/CameraDistansDissolve"
{
	Properties
	{
		_MainTexture("Main Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_NormalMap("Normal Map", 2D) = "bump" {}
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Gloss("Gloss", Range( 0 , 1)) = 0
		_DissolveTexture("Dissolve Texture", 2D) = "white" {}
		_Vertexpower("Vertex power", Float) = 0.1
		_Maxdistance("Max distance", Float) = -0.2
		[Toggle]_Inversedissolve("Inverse dissolve", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform sampler2D _MainTexture;
		uniform float4 _MainTexture_ST;
		uniform float4 _Color;
		uniform float _Metallic;
		uniform float _Gloss;
		uniform sampler2D _DissolveTexture;
		uniform float4 _DissolveTexture_ST;
		uniform float _Inversedissolve;
		uniform float _Vertexpower;
		uniform float _Maxdistance;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			o.Normal = UnpackNormal( tex2D( _NormalMap, uv_NormalMap ) );
			float2 uv_MainTexture = i.uv_texcoord * _MainTexture_ST.xy + _MainTexture_ST.zw;
			o.Albedo = ( tex2D( _MainTexture, uv_MainTexture ) * _Color ).rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Gloss;
			o.Alpha = 1;
			float2 uv_DissolveTexture = i.uv_texcoord * _DissolveTexture_ST.xy + _DissolveTexture_ST.zw;
			float4 transform54 = mul(unity_ObjectToWorld,float4( 0,0,0,1 ));
			float temp_output_57_0 = ( _Vertexpower * distance( _WorldSpaceCameraPos , (transform54).xyz ) );
			float ifLocalVar9 = 0;
			if( temp_output_57_0 <= abs( _Maxdistance ) )
				ifLocalVar9 = 0.0;
			else
				ifLocalVar9 = ( temp_output_57_0 + _Maxdistance );
			float clampResult11 = clamp( ifLocalVar9 , 0.0 , 1.0 );
			float clampResult22 = clamp( (-4.0 + (( tex2D( _DissolveTexture, uv_DissolveTexture ).r + (-0.65 + (lerp(clampResult11,(1.0 + (clampResult11 - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)),_Inversedissolve) - 0.0) * (0.65 - -0.65) / (1.0 - 0.0)) ) - 0.0) * (4.0 - -4.0) / (1.0 - 0.0)) , 0.0 , 1.0 );
			clip( ( 1.0 - clampResult22 ) - 0.5 );
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=16200
785;92;856;655;2924.367;-66.19403;1;True;False
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;54;-3017.129,482.3939;Float;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldSpaceCameraPos;40;-2846.945,326.6788;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ComponentMaskNode;45;-2793.193,477.1133;Float;False;True;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-2550.473,295.4844;Float;False;Property;_Vertexpower;Vertex power;7;0;Create;True;0;0;False;0;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;3;-2528.64,390.355;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;57;-2337.387,390.0685;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-2341.392,506.6833;Float;False;Property;_Maxdistance;Max distance;8;0;Create;True;0;0;False;0;-0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;8;-2132.008,460.2925;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-2133.114,556.2243;Float;False;Constant;_Float0;Float 0;2;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;7;-2132.094,364.4588;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;9;-1947.114,400.2247;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;11;-1775.48,400.6122;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;14;-1607.416,511.3759;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;12;-1424.095,386.6075;Float;False;Property;_Inversedissolve;Inverse dissolve;9;0;Create;True;0;0;False;0;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;15;-1158.271,392.9073;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-0.65;False;4;FLOAT;0.65;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;18;-1270.674,177.7632;Float;True;Property;_DissolveTexture;Dissolve Texture;5;0;Create;True;0;0;False;0;e28dc97a9541e3642a48c0e3886688c5;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;16;-923.1845,299.3291;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;19;-747.8679,300.8724;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-4;False;4;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;17;-659.3397,-424.9921;Float;True;Property;_MainTexture;Main Texture;0;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;25;-578.8288,-229.029;Float;False;Property;_Color;Color;1;0;Create;True;0;0;False;0;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;22;-527.8411,298.7791;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;27;-640.2072,-57.36061;Float;True;Property;_NormalMap;Normal Map;2;0;Create;True;0;0;False;0;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-280.5026,-205.8679;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-337.4709,79.72838;Float;False;Property;_Metallic;Metallic;3;0;Create;True;0;0;False;0;0;0.27;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-328.223,152.3255;Float;False;Property;_Gloss;Gloss;4;0;Create;True;0;0;False;0;0;0.27;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;24;-355.3539,300.6151;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;Float;;0;0;Standard;EGA/CameraDistansDissolve;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;1;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;6;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;6;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;45;0;54;0
WireConnection;3;0;40;0
WireConnection;3;1;45;0
WireConnection;57;0;4;0
WireConnection;57;1;3;0
WireConnection;8;0;57;0
WireConnection;8;1;6;0
WireConnection;7;0;6;0
WireConnection;9;0;57;0
WireConnection;9;1;7;0
WireConnection;9;2;8;0
WireConnection;9;3;10;0
WireConnection;9;4;10;0
WireConnection;11;0;9;0
WireConnection;14;0;11;0
WireConnection;12;0;11;0
WireConnection;12;1;14;0
WireConnection;15;0;12;0
WireConnection;16;0;18;1
WireConnection;16;1;15;0
WireConnection;19;0;16;0
WireConnection;22;0;19;0
WireConnection;26;0;17;0
WireConnection;26;1;25;0
WireConnection;24;0;22;0
WireConnection;0;0;26;0
WireConnection;0;1;27;0
WireConnection;0;3;28;0
WireConnection;0;4;29;0
WireConnection;0;10;24;0
ASEEND*/
//CHKSM=455BD8E4B5E837007DCD242AF16FE12FAAB0D8E2