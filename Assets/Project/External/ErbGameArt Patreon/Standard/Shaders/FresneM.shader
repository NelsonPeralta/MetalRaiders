Shader "EGA/Additive/FresneM" {
    Properties {
        _Color ("Color", Color) = (0,0,0,1)
        _Borderfresnel ("Border fresnel", Range(0, 10)) = 1
        _Borderfresnelcolor ("Border fresnel color", Color) = (0,1,0,1)
        _NoiseUspeed ("Noise U speed", Float ) = 0.3
        _NoiseVspeed ("Noise V speed", Float ) = 0.2
        _Fresnelstrench ("Fresnel strench", Float ) = 2
        _Noise ("Noise", 2D) = "white" {}
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            uniform float4 _Color;
            uniform float _Borderfresnel;
            uniform float4 _Borderfresnelcolor;
            uniform sampler2D _Noise; uniform float4 _Noise_ST;
            uniform float _NoiseUspeed;
            uniform float _NoiseVspeed;
            uniform float _Fresnelstrench;
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
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float2 rrr = (i.uv0+(_Time.g*float2(_NoiseUspeed,_NoiseVspeed)));
                float4 _Noise_var = tex2D(_Noise,TRANSFORM_TEX(rrr, _Noise));
                float3 emissive = (_Color.rgb+saturate((_Borderfresnelcolor.rgb*(_Fresnelstrench*pow(1.0-max(0,dot(normalDirection, viewDirection)),_Borderfresnel))*_Noise_var.rgb*i.vertexColor.rgb)));
                return fixed4(emissive,1);
            }
            ENDCG
        }
    }
}
