Shader "EGA/Projector/Additive/Aura" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
		_FalloffTex ("FallOff", 2D) = "white" {}
        _TintColor ("Color", Color) = (0.4779412,0.6975659,1,1)
        _Emission ("Emission", Float ) = 2
        _MoveCirle ("MoveCirle", Range(0.01, 4)) = 4
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
            uniform float4 _MainTex_ST;
            uniform float4 _TintColor;
            uniform float _Emission;
            uniform float _MoveCirle;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
			
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
				float4 uvFalloff : TEXCOORD1;
                float4 vertexColor : COLOR;
                UNITY_FOG_COORDS(2)
            };
			
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;
			
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos(v.vertex );
				o.uv0 = mul (unity_Projector, v.vertex);
				o.uvFalloff = mul (unity_ProjectorClip, v.vertex);
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
			
			sampler2D _MainTex;
			sampler2D _FalloffTex;
			
            float4 frag(VertexOutput i) : COLOR {

                float4 MainTexvar = tex2D(_MainTex,UNITY_PROJ_COORD(i.uv0));
				MainTexvar.a = 1.0-MainTexvar.a;
				fixed4 texF = tex2Dproj (_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));
				fixed4 res = MainTexvar * texF.a;
                float node_4906 = 0.5;
                float2 node_6035 = (i.uv0-node_4906);
                float node_3008 = (1.0 - length(abs((node_6035/(node_4906*_MoveCirle)))));
                float node_8342 = length(abs((node_6035/(_MoveCirle*0.2*_MoveCirle*_MoveCirle))));
                float node_3473 = (saturate(node_3008)*saturate(node_8342));
                float3 emissive = (res.rgb*i.vertexColor.rgb*_TintColor.rgb*_Emission*saturate((node_3473+saturate((1.0 - length(abs((node_6035/(_MoveCirle*0.13)))))))));
				float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalRGBA, fixed4(0,0,0,1));
                return finalRGBA;
            }
            ENDCG
        }
    }
}
