// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ERB/Particles/VertexNoise"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_MainTex("MainTex", 2D) = "white" {}
		_Noise("Noise", 2D) = "white" {}
		_SpeedMainTexUVNoiseZW("Speed MainTex U/V + Noise Z/W", Vector) = (0,0,0,0)
		_UpColor("Up Color", Color) = (0.1568628,0.1490196,0.145098,1)
		_DownColor("Down Color", Color) = (1,1,1,1)
		_ColorPosition("Color Position", Range( 0 , 1)) = 0.35
		_Vertexoffset("Vertex offset", Float) = 1
		_Power("Power", Float) = 1
		_Color("Color", Color) = (1,1,1,1)
		_Fireemission("Fire emission", Float) = 2
		_Emission("Emission", Float) = 2
		_Dissolve("Dissolve", 2D) = "white" {}
		[HideInInspector] _tex4coord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Off
		ZWrite On
		ZTest LEqual
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow vertex:vertexDataFunc 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float4 uv_tex4coord;
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _MainTex;
		uniform float4 _SpeedMainTexUVNoiseZW;
		uniform float4 _MainTex_ST;
		uniform sampler2D _Noise;
		uniform float4 _Noise_ST;
		uniform float _Vertexoffset;
		uniform float4 _DownColor;
		uniform float4 _UpColor;
		uniform float _ColorPosition;
		uniform float4 _Color;
		uniform float _Power;
		uniform float _Fireemission;
		uniform float _Emission;
		uniform sampler2D _Dissolve;
		uniform float4 _Dissolve_ST;
		uniform float _Cutoff = 0.5;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 appendResult3 = (float2(_SpeedMainTexUVNoiseZW.x , _SpeedMainTexUVNoiseZW.y));
			float2 uv0_MainTex = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 panner7 = ( 1.0 * _Time.y * appendResult3 + uv0_MainTex);
			float4 tex2DNode11 = tex2Dlod( _MainTex, float4( panner7, 0, 0.0) );
			float2 appendResult5 = (float2(_SpeedMainTexUVNoiseZW.z , _SpeedMainTexUVNoiseZW.w));
			float4 uv0_Noise = v.texcoord;
			uv0_Noise.xy = v.texcoord.xy * _Noise_ST.xy + _Noise_ST.zw;
			float2 panner8 = ( 1.0 * _Time.y * appendResult5 + uv0_Noise.xy);
			float3 ase_vertexNormal = v.normal.xyz;
			float W78 = uv0_Noise.z;
			v.vertex.xyz += ( ( tex2DNode11.r * tex2Dlod( _Noise, float4( panner8, 0, 0.0) ).r ) * ase_vertexNormal * _Vertexoffset * (1.0 + (W78 - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _DownColor.rgb;
			float4 uv0_Noise = i.uv_tex4coord;
			uv0_Noise.xy = i.uv_tex4coord.xy * _Noise_ST.xy + _Noise_ST.zw;
			float V77 = uv0_Noise.y;
			float4 lerpResult31 = lerp( _DownColor , _UpColor , saturate( ( (-2.0 + (_ColorPosition - 0.0) * (1.0 - -2.0) / (1.0 - 0.0)) + V77 ) ));
			float2 appendResult3 = (float2(_SpeedMainTexUVNoiseZW.x , _SpeedMainTexUVNoiseZW.y));
			float2 uv0_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 panner7 = ( 1.0 * _Time.y * appendResult3 + uv0_MainTex);
			float4 tex2DNode11 = tex2D( _MainTex, panner7 );
			float smoothstepResult57 = smoothstep( 0.0 , 1.2 , ( 1.0 - tex2DNode11.r ));
			float clampResult62 = clamp( pow( abs( smoothstepResult57 ) , _Power ) , 0.0 , 1.0 );
			o.Emission = ( ( ( lerpResult31 * tex2DNode11.r ) + ( _Color * clampResult62 * _Fireemission * i.vertexColor ) ) * _Emission ).rgb;
			o.Metallic = 1.0;
			o.Alpha = 1;
			float2 uv_Dissolve = i.uv_texcoord * _Dissolve_ST.xy + _Dissolve_ST.zw;
			clip( ( 1.0 - ( ( 1.0 - i.vertexColor.a ) * 2.0 * ( 1.0 - tex2D( _Dissolve, uv_Dissolve ).r ) ) ) - _Cutoff );
		}

		ENDCG
	}
}
/*ASEBEGIN
Version=17000
684;92;918;655;990.2475;444.8212;1.064173;True;False
Node;AmplifyShaderEditor.CommentaryNode;1;-2754.915,-318.2352;Float;False;1037.896;533.6285;Textures movement;9;8;7;6;5;4;3;2;77;78;Textures movement;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector4Node;2;-2704.915,-89.19219;Float;False;Property;_SpeedMainTexUVNoiseZW;Speed MainTex U/V + Noise Z/W;3;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;3;-2297.93,-155.7068;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;4;-2396.206,-276.0006;Float;False;0;11;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;7;-2091.942,-274.1725;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;83;-1228.193,-1805.197;Float;False;1191.727;644.7777;Fire glow;10;56;57;60;59;58;65;70;64;62;63;Fire glow;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;81;-1483.773,-1020.454;Float;False;1056.989;588.2485;Normal color throw UV position;8;32;33;31;30;29;28;79;26;Normal color throw UV position;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;6;-2324.417,-57.9666;Float;False;0;10;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;11;-1509.026,-314.2832;Float;True;Property;_MainTex;MainTex;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;26;-1433.773,-720.6111;Float;False;Property;_ColorPosition;Color Position;6;0;Create;True;0;0;False;0;0.35;0.35;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;56;-1178.193,-1575.422;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;77;-2014.164,-3.903257;Float;False;V;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;28;-1163.131,-705.8463;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-2;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;79;-1159.25,-547.2056;Float;False;77;V;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;57;-983.6824,-1573.034;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;59;-738.2447,-1574.417;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;60;-751.6697,-1455.46;Float;False;Property;_Power;Power;8;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;29;-941.0193,-628.6639;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;5;-2255.066,98.20173;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;32;-905.0773,-793.0245;Float;False;Property;_UpColor;Up Color;4;0;Create;True;0;0;False;0;0.1568628,0.1490196,0.145098,1;0.4575472,0.7381514,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;30;-807.2792,-630.8388;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;58;-595.2752,-1567.85;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;33;-878.7101,-970.4537;Float;False;Property;_DownColor;Down Color;5;0;Create;True;0;0;False;0;1,1,1,1;0.02352941,0.2055747,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;31;-610.7842,-809.6005;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;70;-456.5394,-1362.419;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;105;-528.6909,-99.3111;Float;True;Property;_Dissolve;Dissolve;12;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;78;-2012.36,67.56963;Float;False;W;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;8;-2013.928,-122.526;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;64;-479.4736,-1755.197;Float;False;Property;_Color;Color;9;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;86;-650.8095,322.6706;Float;False;665.5124;506.9377;Vertex offset;5;55;25;54;84;91;Vertex offset;1,1,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;62;-429.0048,-1566.519;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;65;-460.8364,-1448.657;Float;False;Property;_Fireemission;Fire emission;10;0;Create;True;0;0;False;0;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;10;-1510.413,-128.8459;Float;True;Property;_Noise;Noise;2;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;-205.4663,-1623.356;Float;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;84;-622.8257,614.8008;Float;False;78;W;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;103;-179.1894,-26.62088;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-70.95093,-357.6335;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;102;-173.4665,-97.41223;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;55;-415.9966,529.0269;Float;False;Property;_Vertexoffset;Vertex offset;7;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;25;-428.373,391.0247;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;91;-416.6239,620.4464;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;16;96.07044,-358.9449;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;69;86.9547,-249.5509;Float;False;Property;_Emission;Emission;11;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-1173.357,-124.3871;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;22.17835,-78.14967;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;2;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;-154.2971,372.6707;Float;False;4;4;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;101;175.4133,-102.3355;Float;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;80;328.1249,-774.507;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;271.7571,-360.4855;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;71;350.0648,-242.4405;Float;False;Constant;_Float0;Float 0;11;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;89;-1983.911,358.3781;Float;False;T;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;555.4087,-376.143;Float;False;True;2;Float;;0;0;Standard;ERB/Particles/VertexNoise;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;1;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;False;TransparentCutout;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;3;0;2;1
WireConnection;3;1;2;2
WireConnection;7;0;4;0
WireConnection;7;2;3;0
WireConnection;11;1;7;0
WireConnection;56;0;11;1
WireConnection;77;0;6;2
WireConnection;28;0;26;0
WireConnection;57;0;56;0
WireConnection;59;0;57;0
WireConnection;29;0;28;0
WireConnection;29;1;79;0
WireConnection;5;0;2;3
WireConnection;5;1;2;4
WireConnection;30;0;29;0
WireConnection;58;0;59;0
WireConnection;58;1;60;0
WireConnection;31;0;33;0
WireConnection;31;1;32;0
WireConnection;31;2;30;0
WireConnection;78;0;6;3
WireConnection;8;0;6;0
WireConnection;8;2;5;0
WireConnection;62;0;58;0
WireConnection;10;1;8;0
WireConnection;63;0;64;0
WireConnection;63;1;62;0
WireConnection;63;2;65;0
WireConnection;63;3;70;0
WireConnection;103;0;105;1
WireConnection;15;0;31;0
WireConnection;15;1;11;1
WireConnection;102;0;70;4
WireConnection;91;0;84;0
WireConnection;16;0;15;0
WireConnection;16;1;63;0
WireConnection;12;0;11;1
WireConnection;12;1;10;1
WireConnection;95;0;102;0
WireConnection;95;2;103;0
WireConnection;54;0;12;0
WireConnection;54;1;25;0
WireConnection;54;2;55;0
WireConnection;54;3;91;0
WireConnection;101;1;95;0
WireConnection;80;0;33;0
WireConnection;68;0;16;0
WireConnection;68;1;69;0
WireConnection;0;0;80;0
WireConnection;0;2;68;0
WireConnection;0;3;71;0
WireConnection;0;10;101;0
WireConnection;0;11;54;0
ASEEND*/
//CHKSM=B7C80FE1FD94666F29E339B03E9E7A5F38914402