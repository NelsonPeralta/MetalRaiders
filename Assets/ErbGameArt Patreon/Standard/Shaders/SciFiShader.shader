// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:4795,x:33127,y:32678,varname:node_4795,prsc:2|emission-9663-OUT,alpha-5569-OUT,voffset-4519-OUT;n:type:ShaderForge.SFN_Tex2d,id:6074,x:31450,y:32609,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:5600633135d26c34396ec0f36fb536dd,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:2393,x:32337,y:32382,varname:node_2393,prsc:2|A-797-RGB,B-6074-R,C-893-RGB;n:type:ShaderForge.SFN_Color,id:797,x:31944,y:32412,ptovrint:True,ptlb:Color,ptin:_TintColor,varname:_TintColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_NormalVector,id:7479,x:31000,y:33080,prsc:2,pt:False;n:type:ShaderForge.SFN_Transform,id:4301,x:31175,y:33080,varname:node_4301,prsc:2,tffrom:0,tfto:1|IN-7479-OUT;n:type:ShaderForge.SFN_ComponentMask,id:3828,x:31340,y:33080,varname:node_3828,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-4301-XYZ;n:type:ShaderForge.SFN_RemapRange,id:3675,x:31508,y:33080,varname:node_3675,prsc:2,frmn:-1,frmx:1,tomn:0,tomx:1|IN-3828-OUT;n:type:ShaderForge.SFN_Rotator,id:2392,x:31685,y:33080,varname:node_2392,prsc:2|UVIN-3675-OUT,SPD-9955-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9955,x:31508,y:33267,ptovrint:False,ptlb:Noise speed,ptin:_Noisespeed,varname:node_9955,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.2;n:type:ShaderForge.SFN_Tex2d,id:893,x:32069,y:33080,ptovrint:False,ptlb:Noise,ptin:_Noise,varname:node_893,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:4af318d4cce537c4e8b44530efbdaa2c,ntxv:0,isnm:False|UVIN-9636-UVOUT;n:type:ShaderForge.SFN_Multiply,id:4519,x:32280,y:33346,varname:node_4519,prsc:2|A-7479-OUT,B-893-A,C-2075-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2075,x:32069,y:33256,ptovrint:False,ptlb:Vertex offset,ptin:_Vertexoffset,varname:node_2075,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.15;n:type:ShaderForge.SFN_Multiply,id:9033,x:32298,y:32919,varname:node_9033,prsc:2|A-6074-R,B-797-A,C-893-A;n:type:ShaderForge.SFN_Slider,id:2176,x:32504,y:33011,ptovrint:False,ptlb:Opacity,ptin:_Opacity,varname:node_2176,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Add,id:7466,x:32487,y:32872,varname:node_7466,prsc:2|A-5119-OUT,B-9033-OUT;n:type:ShaderForge.SFN_Fresnel,id:4076,x:31944,y:32869,varname:node_4076,prsc:2|EXP-5422-OUT;n:type:ShaderForge.SFN_ValueProperty,id:5422,x:31777,y:32869,ptovrint:False,ptlb:Fresnel,ptin:_Fresnel,varname:node_5422,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_SwitchProperty,id:5119,x:32298,y:32796,ptovrint:False,ptlb:Use fresnel?,ptin:_Usefresnel,varname:node_5119,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True|A-7536-OUT,B-4076-OUT;n:type:ShaderForge.SFN_Vector1,id:7536,x:31944,y:32795,varname:node_7536,prsc:2,v1:0;n:type:ShaderForge.SFN_Clamp01,id:843,x:32661,y:32872,varname:node_843,prsc:2|IN-7466-OUT;n:type:ShaderForge.SFN_Multiply,id:3104,x:32337,y:32509,varname:node_3104,prsc:2|A-2874-RGB,B-4076-OUT;n:type:ShaderForge.SFN_Color,id:2874,x:31944,y:32643,ptovrint:False,ptlb:Fresnel color,ptin:_Fresnelcolor,varname:node_2874,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.25,c2:0.4724138,c3:1,c4:1;n:type:ShaderForge.SFN_Add,id:3598,x:32523,y:32509,varname:node_3598,prsc:2|A-2393-OUT,B-3104-OUT;n:type:ShaderForge.SFN_Multiply,id:9663,x:32695,y:32509,varname:node_9663,prsc:2|A-3598-OUT,B-5786-OUT,C-6668-RGB;n:type:ShaderForge.SFN_ValueProperty,id:5786,x:32523,y:32656,ptovrint:False,ptlb:Emission,ptin:_Emission,varname:node_5786,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2;n:type:ShaderForge.SFN_Multiply,id:5569,x:32843,y:32872,varname:node_5569,prsc:2|A-843-OUT,B-2874-A,C-2176-OUT,D-6668-A;n:type:ShaderForge.SFN_Panner,id:9636,x:31874,y:33080,varname:node_9636,prsc:2,spu:0.5,spv:0.25|UVIN-2392-UVOUT,DIST-4825-OUT;n:type:ShaderForge.SFN_Multiply,id:4825,x:31685,y:33206,varname:node_4825,prsc:2|A-9955-OUT,B-3626-T;n:type:ShaderForge.SFN_Time,id:3626,x:31508,y:33354,varname:node_3626,prsc:2;n:type:ShaderForge.SFN_VertexColor,id:6668,x:32523,y:32722,varname:node_6668,prsc:2;proporder:6074-797-5786-2176-893-9955-2075-5119-5422-2874;pass:END;sub:END;*/

Shader "EGA/Particles/SciFiShader" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _TintColor ("Color", Color) = (1,0,0,1)
        _Emission ("Emission", Float ) = 2
        _Opacity ("Opacity", Range(0, 1)) = 1
        _Noise ("Noise", 2D) = "white" {}
        _Noisespeed ("Noise speed", Float ) = 0.2
        _Vertexoffset ("Vertex offset", Float ) = 0.15
        [MaterialToggle] _Usefresnel ("Use fresnel?", Float ) = 0
        _Fresnel ("Fresnel", Float ) = 1
        _Fresnelcolor ("Fresnel color", Color) = (0.25,0.4724138,1,1)
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
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _TintColor;
            uniform float _Noisespeed;
            uniform sampler2D _Noise; uniform float4 _Noise_ST;
            uniform float _Vertexoffset;
            uniform float _Opacity;
            uniform float _Fresnel;
            uniform fixed _Usefresnel;
            uniform float4 _Fresnelcolor;
            uniform float _Emission;
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
                UNITY_FOG_COORDS(3)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float node_2392_cos = cos(_Noisespeed*_Time.g);
                float node_2392_sin = sin(_Noisespeed*_Time.g);
                float2 node_2392_piv = float2(0.5,0.5);
                float2 node_2392 = (mul((mul( unity_WorldToObject, float4(v.normal,0) ).xyz.rgb.rg*0.5+0.5)-node_2392_piv,float2x2( node_2392_cos, -node_2392_sin, node_2392_sin, node_2392_cos))+node_2392_piv);
                float2 node_9636 = (node_2392+(_Noisespeed*_Time.g)*float2(0.5,0.25));
                float4 _Noise_var = tex2Dlod(_Noise,float4(TRANSFORM_TEX(node_9636, _Noise),0.0,0));
                v.vertex.xyz += (v.normal*_Noise_var.a*_Vertexoffset);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float node_2392_cos = cos(_Noisespeed*_Time.g);
                float node_2392_sin = sin(_Noisespeed*_Time.g);
                float2 node_2392_piv = float2(0.5,0.5);
                float2 node_2392 = (mul((mul( unity_WorldToObject, float4(i.normalDir,0) ).xyz.rgb.rg*0.5+0.5)-node_2392_piv,float2x2( node_2392_cos, -node_2392_sin, node_2392_sin, node_2392_cos))+node_2392_piv);
                float2 node_9636 = (node_2392+(_Noisespeed*_Time.g)*float2(0.5,0.25));
                float4 _Noise_var = tex2D(_Noise,TRANSFORM_TEX(node_9636, _Noise));
                float node_4076 = pow(1.0-max(0,dot(normalDirection, viewDirection)),_Fresnel);
                float3 emissive = (((_TintColor.rgb*_MainTex_var.r*_Noise_var.rgb)+(_Fresnelcolor.rgb*node_4076))*_Emission*i.vertexColor.rgb);
                fixed4 finalRGBA = fixed4(emissive,(saturate((lerp( 0.0, node_4076, _Usefresnel )+(_MainTex_var.r*_TintColor.a*_Noise_var.a)))*_Fresnelcolor.a*_Opacity*i.vertexColor.a));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
}
