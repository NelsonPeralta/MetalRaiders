Shader "EGA/Nova bomb" {
    Properties {
        _Dissolveamount ("Dissolve amount", Range(0, 1)) = 0.3
        _Vspeed ("V speed", Float ) = 0.225
        _Uspeed ("U speed", Float ) = 0.189
        _MainTex ("MainTex", 2D) = "white" {}
        _2Uspeed ("2U speed", Float ) = -0.2
        _2Vspeed ("2V speed", Float ) = -0.05
        _MainColor ("MainColor", Color) = (0.4,0.4,1,1)
        _Opacity ("Opacity", Float ) = 2
        _Lightninigstrench ("Lightninig strench", Float ) = 1
        _Mask ("Mask", 2D) = "white" {}
        _Fresnel ("Fresnel", Float ) = 0.4
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "Queue"="Transparent"
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
            //#pragma target 2.0
            uniform float4 _TimeEditor;
            uniform float _Dissolveamount;
            uniform sampler2D _Mask; uniform float4 _Mask_ST;
            uniform float _Vspeed;
            uniform float _Uspeed;
            uniform float _2Uspeed;
            uniform float _2Vspeed;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _MainColor;
            uniform float _Opacity;
            uniform float _Lightninigstrench;
            uniform float _Fresnel;
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
                float4 node_3095 = tex2D(_MainTex,TRANSFORM_TEX(node_2641, _MainTex));
                float4 node_6407 = _Time + _TimeEditor;
                float2 node_5967 = ((float2(_2Uspeed,_2Vspeed)*node_6407.g)+i.uv0);
                float4 node_5059 = tex2D(_MainTex,TRANSFORM_TEX(node_5967, _MainTex));
                float2 node_2834 = float2((1.0 - saturate((((node_7177+node_3095.r)*(node_7177+node_5059.r))*20.0+-10.0))),0.0);
                float4 _Mask_var = tex2D(_Mask,TRANSFORM_TEX(node_2834, _Mask));
                clip((pow(1.0-max(0,dot(normalDirection, viewDirection)),_Fresnel)*(_Lightninigstrench*_Mask_var.r)) - 0.5);
                float3 emissive = (_Mask_var.rgb*i.vertexColor.rgb*_MainColor.rgb*_Opacity);
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Dissolve"
}
