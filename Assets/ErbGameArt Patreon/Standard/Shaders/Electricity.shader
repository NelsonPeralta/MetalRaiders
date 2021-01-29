// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.27 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.27;sub:START;pass:START;ps:flbk:Dissolve,iptp:0,cusa:True,bamd:0,lico:1,lgpr:1,limd:3,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:False,qofs:0,qpre:2,rntp:3,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:True,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:3138,x:33130,y:32118,varname:node_3138,prsc:2|emission-2628-OUT,clip-4914-OUT;n:type:ShaderForge.SFN_Tex2d,id:3095,x:30987,y:32751,varname:node_3095,prsc:2,tex:28c7aad1372ff114b90d330f8a2dd938,ntxv:0,isnm:False|UVIN-2641-OUT,TEX-6645-TEX;n:type:ShaderForge.SFN_Slider,id:3858,x:30481,y:32544,ptovrint:False,ptlb:Dissolve amount,ptin:_Dissolveamount,varname:node_3858,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.332,max:1;n:type:ShaderForge.SFN_Add,id:8744,x:31168,y:32544,varname:node_8744,prsc:2|A-7177-OUT,B-3095-R;n:type:ShaderForge.SFN_RemapRange,id:7177,x:30987,y:32544,varname:node_7177,prsc:2,frmn:0,frmx:1,tomn:-0.65,tomx:0.65|IN-9938-OUT;n:type:ShaderForge.SFN_OneMinus,id:9938,x:30813,y:32544,varname:node_9938,prsc:2|IN-3858-OUT;n:type:ShaderForge.SFN_RemapRange,id:4741,x:31560,y:32544,varname:node_4741,prsc:2,frmn:0,frmx:1,tomn:-10,tomx:10|IN-7134-OUT;n:type:ShaderForge.SFN_Clamp01,id:8794,x:31773,y:32544,varname:node_8794,prsc:2|IN-4741-OUT;n:type:ShaderForge.SFN_Append,id:2834,x:32189,y:32544,varname:node_2834,prsc:2|A-5880-OUT,B-7510-OUT;n:type:ShaderForge.SFN_Vector1,id:7510,x:31999,y:32732,varname:node_7510,prsc:2,v1:0;n:type:ShaderForge.SFN_Tex2d,id:8891,x:32397,y:32544,ptovrint:False,ptlb:node_1096,ptin:_node_1096,varname:node_8891,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:e68e4eb07d328524a81ff0706ddc32af,ntxv:0,isnm:False|UVIN-2834-OUT;n:type:ShaderForge.SFN_OneMinus,id:5880,x:31978,y:32544,varname:node_5880,prsc:2|IN-8794-OUT;n:type:ShaderForge.SFN_Append,id:1154,x:30453,y:32751,varname:node_1154,prsc:2|A-3887-OUT,B-9743-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9743,x:30282,y:32903,ptovrint:False,ptlb:V speed,ptin:_Vspeed,varname:node_9743,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.225;n:type:ShaderForge.SFN_ValueProperty,id:3887,x:30282,y:32751,ptovrint:False,ptlb:U speed,ptin:_Uspeed,varname:node_3887,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.189;n:type:ShaderForge.SFN_Time,id:8549,x:30453,y:32903,varname:node_8549,prsc:2;n:type:ShaderForge.SFN_Multiply,id:7614,x:30632,y:32751,varname:node_7614,prsc:2|A-1154-OUT,B-8549-T;n:type:ShaderForge.SFN_TexCoord,id:3447,x:30632,y:32903,varname:node_3447,prsc:2,uv:0;n:type:ShaderForge.SFN_Add,id:2641,x:30813,y:32751,varname:node_2641,prsc:2|A-7614-OUT,B-3447-UVOUT;n:type:ShaderForge.SFN_Add,id:4797,x:31168,y:32751,varname:node_4797,prsc:2|A-7177-OUT,B-5059-R;n:type:ShaderForge.SFN_Multiply,id:7134,x:31364,y:32544,varname:node_7134,prsc:2|A-8744-OUT,B-4797-OUT;n:type:ShaderForge.SFN_Append,id:9473,x:30445,y:33060,varname:node_9473,prsc:2|A-1974-OUT,B-5158-OUT;n:type:ShaderForge.SFN_Time,id:6407,x:30445,y:33223,varname:node_6407,prsc:2;n:type:ShaderForge.SFN_Multiply,id:8506,x:30632,y:33060,varname:node_8506,prsc:2|A-9473-OUT,B-6407-T;n:type:ShaderForge.SFN_TexCoord,id:8561,x:30632,y:33223,varname:node_8561,prsc:2,uv:0;n:type:ShaderForge.SFN_Add,id:5967,x:30813,y:33060,varname:node_5967,prsc:2|A-8506-OUT,B-8561-UVOUT;n:type:ShaderForge.SFN_ValueProperty,id:1974,x:30282,y:33060,ptovrint:False,ptlb:2U speed,ptin:_2Uspeed,varname:node_1974,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:-0.2;n:type:ShaderForge.SFN_ValueProperty,id:5158,x:30282,y:33223,ptovrint:False,ptlb:2V speed,ptin:_2Vspeed,varname:node_5158,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:-0.05;n:type:ShaderForge.SFN_Tex2dAsset,id:6645,x:30813,y:32903,ptovrint:False,ptlb:node_6645,ptin:_node_6645,varname:node_6645,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:28c7aad1372ff114b90d330f8a2dd938,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:5059,x:30990,y:33060,varname:node_5059,prsc:2,tex:28c7aad1372ff114b90d330f8a2dd938,ntxv:0,isnm:False|UVIN-5967-OUT,TEX-6645-TEX;n:type:ShaderForge.SFN_Multiply,id:2628,x:32652,y:32535,varname:node_2628,prsc:2|A-8891-RGB,B-5006-RGB,C-2840-RGB,D-1748-OUT;n:type:ShaderForge.SFN_VertexColor,id:5006,x:32397,y:32732,varname:node_5006,prsc:2;n:type:ShaderForge.SFN_Color,id:2840,x:32397,y:32895,ptovrint:False,ptlb:MainColor,ptin:_MainColor,varname:node_2840,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.3897059,c2:0.4191683,c3:1,c4:1;n:type:ShaderForge.SFN_ValueProperty,id:1748,x:32397,y:33088,ptovrint:False,ptlb:Opacity,ptin:_Opacity,varname:node_1748,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2;n:type:ShaderForge.SFN_Multiply,id:4914,x:32652,y:32389,varname:node_4914,prsc:2|A-6196-OUT,B-8891-R;n:type:ShaderForge.SFN_ValueProperty,id:6196,x:32397,y:32389,ptovrint:False,ptlb:Lightninig strench,ptin:_Lightninigstrench,varname:node_6196,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;proporder:3858-9743-3887-6645-1974-5158-2840-1748-6196-8891;pass:END;sub:END;*/

Shader "EGA/Electricity" {
    Properties {
        _Dissolveamount ("Dissolve amount", Range(0, 1)) = 0.332
        _Vspeed ("V speed", Float ) = 0.225
        _Uspeed ("U speed", Float ) = 0.189
        _node_6645 ("node_6645", 2D) = "white" {}
        _2Uspeed ("2U speed", Float ) = -0.2
        _2Vspeed ("2V speed", Float ) = -0.05
        _MainColor ("MainColor", Color) = (0.3897059,0.4191683,1,1)
        _Opacity ("Opacity", Float ) = 2
        _Lightninigstrench ("Lightninig strench", Float ) = 1
        _node_1096 ("node_1096", 2D) = "white" {}
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "Queue"="AlphaTest"
            "RenderType"="TransparentCutout"
            "CanUseSpriteAtlas"="True"
        }
        LOD 3
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma target 2.0
            uniform float4 _TimeEditor;
            uniform float _Dissolveamount;
            uniform sampler2D _node_1096; uniform float4 _node_1096_ST;
            uniform float _Vspeed;
            uniform float _Uspeed;
            uniform float _2Uspeed;
            uniform float _2Vspeed;
            uniform sampler2D _node_6645; uniform float4 _node_6645_ST;
            uniform float4 _MainColor;
            uniform float _Opacity;
            uniform float _Lightninigstrench;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float node_7177 = ((1.0 - _Dissolveamount)*1.3+-0.65);
                float4 node_8549 = _Time + _TimeEditor;
                float2 node_2641 = ((float2(_Uspeed,_Vspeed)*node_8549.g)+i.uv0);
                float4 node_3095 = tex2D(_node_6645,TRANSFORM_TEX(node_2641, _node_6645));
                float4 node_6407 = _Time + _TimeEditor;
                float2 node_5967 = ((float2(_2Uspeed,_2Vspeed)*node_6407.g)+i.uv0);
                float4 node_5059 = tex2D(_node_6645,TRANSFORM_TEX(node_5967, _node_6645));
                float2 node_2834 = float2((1.0 - saturate((((node_7177+node_3095.r)*(node_7177+node_5059.r))*20.0+-10.0))),0.0);
                float4 _node_1096_var = tex2D(_node_1096,TRANSFORM_TEX(node_2834, _node_1096));
                clip((_Lightninigstrench*_node_1096_var.r) - 0.5);
////// Lighting:
////// Emissive:
                float3 emissive = (_node_1096_var.rgb*i.vertexColor.rgb*_MainColor.rgb*_Opacity);
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma exclude_renderers xbox360 xboxone ps3 ps4 psp2 
            #pragma target 2.0
            uniform float4 _TimeEditor;
            uniform float _Dissolveamount;
            uniform sampler2D _node_1096; uniform float4 _node_1096_ST;
            uniform float _Vspeed;
            uniform float _Uspeed;
            uniform float _2Uspeed;
            uniform float _2Vspeed;
            uniform sampler2D _node_6645; uniform float4 _node_6645_ST;
            uniform float _Lightninigstrench;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float node_7177 = ((1.0 - _Dissolveamount)*1.3+-0.65);
                float4 node_8549 = _Time + _TimeEditor;
                float2 node_2641 = ((float2(_Uspeed,_Vspeed)*node_8549.g)+i.uv0);
                float4 node_3095 = tex2D(_node_6645,TRANSFORM_TEX(node_2641, _node_6645));
                float4 node_6407 = _Time + _TimeEditor;
                float2 node_5967 = ((float2(_2Uspeed,_2Vspeed)*node_6407.g)+i.uv0);
                float4 node_5059 = tex2D(_node_6645,TRANSFORM_TEX(node_5967, _node_6645));
                float2 node_2834 = float2((1.0 - saturate((((node_7177+node_3095.r)*(node_7177+node_5059.r))*20.0+-10.0))),0.0);
                float4 _node_1096_var = tex2D(_node_1096,TRANSFORM_TEX(node_2834, _node_1096));
                clip((_Lightninigstrench*_node_1096_var.r) - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Dissolve"
}
