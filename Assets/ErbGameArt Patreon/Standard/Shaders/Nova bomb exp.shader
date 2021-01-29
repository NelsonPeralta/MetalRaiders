Shader "EGA/Nova bomb exp" {
    Properties {
        _Dissolveamount ("Dissolve amount", Range(0, 1)) = 0.3
        _2Uspeed ("2U speed", Float ) = -0.2
        _2Vspeed ("2V speed", Float ) = -0.05
        _MainTex ("MainTex", 2D) = "white" {}
        _MainColor ("MainColor", Color) = (0.4,0.4,1,1)
        _Opacity ("Opacity", Float ) = 2
        _Fresnel ("Fresnel", Float ) = 0
        _Vspeed ("V speed", Float ) = 0.225
        _Uspeed ("U speed", Float ) = 0.189
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
            //pragma target 3.0
            uniform float4 _TimeEditor;
            uniform float _Dissolveamount;
            uniform float _Vspeed;
            uniform float _Uspeed;
            uniform float _2Uspeed;
            uniform float _2Vspeed;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _MainColor;
            uniform float _Opacity;
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
                UNITY_FOG_COORDS(3)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float node_5672 = ((1.0 - _Dissolveamount)*1.3+-0.65);
                float4 node_9883 = _Time + _TimeEditor;
                float2 node_3670 = ((float2(_Uspeed,_Vspeed)*node_9883.g)+i.uv0);
                float4 node_3095 = tex2D(_MainTex,TRANSFORM_TEX(node_3670, _MainTex));
                float2 node_1260 = ((float2(_2Uspeed,_2Vspeed)*node_9883.g)+i.uv0);
                float4 node_5059 = tex2D(_MainTex,TRANSFORM_TEX(node_1260, _MainTex));
                float3 emissive = (pow(1.0-max(0,dot(normalDirection, viewDirection)),_Fresnel)*(saturate((((node_5672+node_3095.r)*(node_5672+node_5059.r))*6.0+-3.0))*i.vertexColor.rgb*_MainColor.rgb*_Opacity));
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalRGBA, fixed4(0,0,0,1));
                return finalRGBA;
            }
            ENDCG
        }
    }
}
