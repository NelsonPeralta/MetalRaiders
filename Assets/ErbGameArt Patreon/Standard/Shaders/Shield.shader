// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:3,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:False,rprd:True,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:2865,x:34298,y:32468,varname:node_2865,prsc:2|emission-7847-OUT;n:type:ShaderForge.SFN_Tex2d,id:6621,x:31874,y:32759,ptovrint:False,ptlb:Texture,ptin:_Texture,varname:node_6621,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:238fd6255cc449b479da542c05bea3c7,ntxv:0,isnm:False|UVIN-9127-OUT;n:type:ShaderForge.SFN_Fresnel,id:9391,x:31874,y:32517,varname:node_9391,prsc:2|EXP-9330-OUT;n:type:ShaderForge.SFN_Multiply,id:7609,x:32053,y:32759,varname:node_7609,prsc:2|A-6621-RGB,B-3425-OUT;n:type:ShaderForge.SFN_Clamp01,id:4209,x:32415,y:32922,varname:node_4209,prsc:2|IN-2951-OUT;n:type:ShaderForge.SFN_Slider,id:9330,x:31525,y:32533,ptovrint:False,ptlb:Fresnel strench,ptin:_Fresnelstrench,varname:node_9330,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1.5,max:3;n:type:ShaderForge.SFN_TexCoord,id:250,x:31516,y:32932,varname:node_250,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Multiply,id:9910,x:31516,y:32759,varname:node_9910,prsc:2|A-9-OUT,B-326-T;n:type:ShaderForge.SFN_Time,id:326,x:31337,y:32932,varname:node_326,prsc:2;n:type:ShaderForge.SFN_Color,id:2495,x:32415,y:32764,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_2495,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:0.5019608,c3:0,c4:1;n:type:ShaderForge.SFN_Append,id:9,x:31337,y:32759,varname:node_9,prsc:2|A-1979-OUT,B-6507-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1979,x:31168,y:32759,ptovrint:False,ptlb:U speed,ptin:_Uspeed,varname:node_1979,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:6507,x:31168,y:32841,ptovrint:False,ptlb:V speed,ptin:_Vspeed,varname:node_6507,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:-0.1;n:type:ShaderForge.SFN_Add,id:9127,x:31687,y:32759,varname:node_9127,prsc:2|A-9910-OUT,B-250-UVOUT;n:type:ShaderForge.SFN_DepthBlend,id:5917,x:32706,y:32919,varname:node_5917,prsc:2|DIST-2654-OUT;n:type:ShaderForge.SFN_Lerp,id:2670,x:32899,y:32596,varname:node_2670,prsc:2|A-4467-OUT,B-7524-OUT,T-5917-OUT;n:type:ShaderForge.SFN_Color,id:9361,x:32415,y:32363,ptovrint:False,ptlb:Depth color,ptin:_Depthcolor,varname:node_9361,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0.2666667,c3:1,c4:1;n:type:ShaderForge.SFN_ValueProperty,id:2654,x:32415,y:33081,ptovrint:False,ptlb:Depth dist,ptin:_Depthdist,varname:node_2654,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.15;n:type:ShaderForge.SFN_Multiply,id:7524,x:32706,y:32761,varname:node_7524,prsc:2|A-2495-RGB,B-4209-OUT;n:type:ShaderForge.SFN_Multiply,id:4467,x:32706,y:32596,varname:node_4467,prsc:2|A-1242-RGB,B-9361-RGB,C-91-OUT;n:type:ShaderForge.SFN_ValueProperty,id:91,x:32415,y:32681,ptovrint:False,ptlb:Depth color power,ptin:_Depthcolorpower,varname:node_91,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.7;n:type:ShaderForge.SFN_ValueProperty,id:3425,x:31874,y:32942,ptovrint:False,ptlb:Emission,ptin:_Emission,varname:node_3425,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.15;n:type:ShaderForge.SFN_Tex2d,id:1242,x:32415,y:32165,ptovrint:False,ptlb:Depth texture,ptin:_Depthtexture,varname:node_1242,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:154896557ac31e24b9b8a10e61458c22,ntxv:0,isnm:False|UVIN-3106-OUT;n:type:ShaderForge.SFN_Add,id:2951,x:32242,y:32764,varname:node_2951,prsc:2|A-9210-OUT,B-7609-OUT;n:type:ShaderForge.SFN_Multiply,id:9210,x:32053,y:32517,varname:node_9210,prsc:2|A-9391-OUT,B-6740-OUT;n:type:ShaderForge.SFN_ValueProperty,id:6740,x:31874,y:32665,ptovrint:False,ptlb:Fresnel power,ptin:_Fresnelpower,varname:node_6740,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.5;n:type:ShaderForge.SFN_TexCoord,id:1175,x:32053,y:32320,varname:node_1175,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Multiply,id:8351,x:32053,y:32165,varname:node_8351,prsc:2|A-8052-OUT,B-7736-T;n:type:ShaderForge.SFN_Time,id:7736,x:31874,y:32320,varname:node_7736,prsc:2;n:type:ShaderForge.SFN_Append,id:8052,x:31874,y:32165,varname:node_8052,prsc:2|A-9444-OUT,B-9620-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9444,x:31705,y:32165,ptovrint:False,ptlb:U depth speed,ptin:_Udepthspeed,varname:_Uspeed_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:9620,x:31705,y:32247,ptovrint:False,ptlb:V depth speed,ptin:_Vdepthspeed,varname:_Vspeed_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:-0.4;n:type:ShaderForge.SFN_Add,id:3106,x:32233,y:32165,varname:node_3106,prsc:2|A-8351-OUT,B-1175-UVOUT;n:type:ShaderForge.SFN_FaceSign,id:4476,x:32899,y:32761,varname:node_4476,prsc:2,fstp:0;n:type:ShaderForge.SFN_Lerp,id:5876,x:33085,y:32596,varname:node_5876,prsc:2|A-1282-OUT,B-2670-OUT,T-4476-VFACE;n:type:ShaderForge.SFN_Clamp01,id:7306,x:32415,y:32523,varname:node_7306,prsc:2|IN-9210-OUT;n:type:ShaderForge.SFN_Multiply,id:7855,x:32706,y:32293,varname:node_7855,prsc:2|A-1242-RGB,B-9361-RGB,C-91-OUT;n:type:ShaderForge.SFN_Multiply,id:6511,x:32706,y:32437,varname:node_6511,prsc:2|A-2495-RGB,B-7306-OUT;n:type:ShaderForge.SFN_Lerp,id:1282,x:32888,y:32293,varname:node_1282,prsc:2|A-7855-OUT,B-6511-OUT,T-5917-OUT;n:type:ShaderForge.SFN_Step,id:2917,x:33310,y:32761,varname:node_2917,prsc:2|A-1707-V,B-8087-OUT;n:type:ShaderForge.SFN_TexCoord,id:1707,x:33085,y:32761,varname:node_1707,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Step,id:2916,x:33310,y:32933,varname:node_2916,prsc:2|A-1505-OUT,B-1707-V;n:type:ShaderForge.SFN_Clamp01,id:7073,x:33694,y:32819,varname:node_7073,prsc:2|IN-7823-OUT;n:type:ShaderForge.SFN_Multiply,id:7847,x:33869,y:32606,varname:node_7847,prsc:2|A-5876-OUT,B-7073-OUT;n:type:ShaderForge.SFN_Slider,id:8087,x:32928,y:32929,ptovrint:False,ptlb:Step up,ptin:_Stepup,varname:node_8087,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Slider,id:1505,x:32928,y:33023,ptovrint:False,ptlb:Step down,ptin:_Stepdown,varname:node_1505,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Multiply,id:7823,x:33502,y:32819,varname:node_7823,prsc:2|A-2917-OUT,B-2916-OUT;proporder:6621-9330-2495-1979-6507-9361-2654-91-3425-6740-1242-9444-9620-8087-1505;pass:END;sub:END;*/

Shader "EGA/shield" {
    Properties {
        _Texture ("Texture", 2D) = "white" {}
        _Fresnelstrench ("Fresnel strench", Range(0, 3)) = 1.5
        _Color ("Color", Color) = (1,0.5019608,0,1)
        _Uspeed ("U speed", Float ) = 0
        _Vspeed ("V speed", Float ) = -0.1
        _Depthcolor ("Depth color", Color) = (0,0.2666667,1,1)
        _Depthdist ("Depth dist", Float ) = 0.15
        _Depthcolorpower ("Depth color power", Float ) = 0.7
        _Emission ("Emission", Float ) = 0.15
        _Fresnelpower ("Fresnel power", Float ) = 0.5
        _Depthtexture ("Depth texture", 2D) = "white" {}
        _Udepthspeed ("U depth speed", Float ) = 0
        _Vdepthspeed ("V depth speed", Float ) = -0.4
        _Stepup ("Step up", Range(0, 1)) = 1
        _Stepdown ("Step down", Range(0, 1)) = 0
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma target 3.0
            uniform sampler2D _CameraDepthTexture;
            uniform sampler2D _Texture; uniform float4 _Texture_ST;
            uniform float _Fresnelstrench;
            uniform float4 _Color;
            uniform float _Uspeed;
            uniform float _Vspeed;
            uniform float4 _Depthcolor;
            uniform float _Depthdist;
            uniform float _Depthcolorpower;
            uniform float _Emission;
            uniform sampler2D _Depthtexture; uniform float4 _Depthtexture_ST;
            uniform float _Fresnelpower;
            uniform float _Udepthspeed;
            uniform float _Vdepthspeed;
            uniform float _Stepup;
            uniform float _Stepdown;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float4 projPos : TEXCOORD3;
                UNITY_FOG_COORDS(4)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float sceneZ = max(0,LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)))) - _ProjectionParams.g);
                float partZ = max(0,i.projPos.z - _ProjectionParams.g);
                float2 node_3106 = ((float2(_Udepthspeed,_Vdepthspeed)*_Time.g)+i.uv0);
                float4 _Depthtexture_var = tex2D(_Depthtexture,TRANSFORM_TEX(node_3106, _Depthtexture));
                float dd = (pow(1.0-max(0,dot(normalDirection, viewDirection)),_Fresnelstrench)*_Fresnelpower);
                float tt = saturate((sceneZ-partZ)/_Depthdist);
                float2 ccc = ((float2(_Uspeed,_Vspeed)*_Time.g)+i.uv0);
                float4 _Texture_var = tex2D(_Texture,TRANSFORM_TEX(ccc, _Texture));
                float3 emissive = (lerp(lerp((_Depthtexture_var.rgb*_Depthcolor.rgb*_Depthcolorpower),(_Color.rgb*saturate(dd)),tt),lerp((_Depthtexture_var.rgb*_Depthcolor.rgb*_Depthcolorpower),(_Color.rgb*saturate((dd+(_Texture_var.rgb*_Emission)))),tt),isFrontFace)*saturate((step(i.uv0.g,_Stepup)*step(_Stepdown,i.uv0.g))));
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        
    }
    FallBack "Diffuse"
}
