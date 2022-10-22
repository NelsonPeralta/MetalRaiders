// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "EGA/Hologram" {
    Properties {
        _TintColor ("Color", Color) = (1,1,1,1)
        _Segments ("Segments", Range(-10, 10)) = -3
        _MainTex ("MainTex", 2D) = "white" {}
        _Powerofwawes ("Power of wawes", Range(0, 3)) = 3
        _Speedofwases ("Speed of wases", Range(0, 3)) = 0.4
        _Noiseposterize ("Noise posterize", Float ) = 70
        _Noisetime ("Noise time", Float ) = 2
        _Powerofnoise ("Power of noise", Range(0, 1)) = 0.1531285
        _Noiseposterize2 ("Noise posterize 2", Float ) = 5
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
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
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _TintColor;
            uniform float _Segments;
            uniform float _Powerofwawes;
            uniform float _Speedofwases;
            uniform float _Noiseposterize;
            uniform float _Noisetime;
            uniform float _Powerofnoise;
            uniform float _Noiseposterize2;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float4 node_8574 = _Time + _TimeEditor;
                float node_9155 = frac((_Noisetime*node_8574.r));
                float2 node_3641 = floor((round((node_9155*_Noisetime*_Noiseposterize2*2.0))+i.uv0) * _Noiseposterize2) / (_Noiseposterize2 - 1);
                float2 node_3610_skew = node_3641 + 0.2127+node_3641.x*0.3713*node_3641.y;
                float2 node_3610_rnd = 4.789*sin(489.123*(node_3610_skew));
                float node_3610 = frac(node_3610_rnd.x*node_3610_rnd.y*(1+node_3610_skew.x));
                float2 node_6505 = floor((round((node_9155*_Noisetime*_Noiseposterize2))+i.uv0) * _Noiseposterize2) / (_Noiseposterize2 - 1);
                float2 node_2575_skew = node_6505 + 0.2127+node_6505.x*0.3713*node_6505.y;
                float2 node_2575_rnd = 4.789*sin(489.123*(node_2575_skew));
                float node_2575 = frac(node_2575_rnd.x*node_2575_rnd.y*(1+node_2575_skew.x));
                float2 node_6558 = floor((round((node_9155*_Noisetime*_Noiseposterize))+i.uv0) * _Noiseposterize) / (_Noiseposterize - 1);
                float2 node_33_skew = node_6558 + 0.2127+node_6558.x*0.3713*node_6558.y;
                float2 node_33_rnd = 4.789*sin(489.123*(node_33_skew));
                float node_33 = frac(node_33_rnd.x*node_33_rnd.y*(1+node_33_skew.x));
                float node_1861 = (_MainTex_var.a*i.vertexColor.a);
                float3 emissive = ((_MainTex_var.rgb*saturate((saturate((saturate((node_3610+node_2575))*_Powerofnoise*node_33))+pow(frac(((i.uv0.g+(node_8574.g*_Speedofwases))*_Segments)),_Powerofwawes)))*_TintColor.rgb*2.0)*node_1861);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,node_1861);
                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalRGBA, fixed4(0,0,0,1));
                return finalRGBA;
            }
            ENDCG
        }
    }
}
