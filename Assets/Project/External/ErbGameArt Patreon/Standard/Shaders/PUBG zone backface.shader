Shader "EGA/PUBG zone/Backface culling" {
    Properties {
        _Dissolveamount ("Dissolve amount", Range(0, 1)) = 0.5080765
        _Mask ("Mask", 2D) = "white" {}
        _2Uspeed ("2U speed", Float ) = -0.2
        _2Vspeed ("2V speed", Float ) = -0.05
        _MainColor ("MainColor", Color) = (0.3897059,0.4191683,1,1)
        _Opacity ("Opacity", Float ) = 3
        _Lightninigstrench ("Lightninig strench", Float ) = 1
        _Vspeed ("V speed", Float ) = 0.225
        _Uspeed ("U speed", Float ) = 0.189
        _Backcolor ("Back color", Range(0, 20)) = 0.1
        _MainTex ("MainTex", 2D) = "white" {}
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
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma target 3.0
            uniform float _Dissolveamount;
            uniform sampler2D _Mask; uniform float4 _Mask_ST;
            uniform float _Vspeed;
            uniform float _Uspeed;
            uniform float _2Uspeed;
            uniform float _2Vspeed;
            uniform float4 _MainColor;
            uniform float _Opacity;
            uniform float _Backcolor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
                float4 projPos : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float2 sceneUVs = (i.projPos.xy / i.projPos.w);
                float node_1607 = ((1.0 - _Dissolveamount)*1.3+-0.65);
                float4 node_9566 = _Time;
                float2 node_2556 = ((float2(_Uspeed,_Vspeed)*node_9566.g)+i.uv0+(sceneUVs * 2 - 1).rg);
                float4 node_3095 = tex2D(_MainTex,TRANSFORM_TEX(node_2556, _MainTex));
                float4 node_3795 = _Time;
                float2 node_6739 = ((float2(_2Uspeed,_2Vspeed)*node_3795.g)+i.uv0+(sceneUVs * 2 - 1).rg);
                float4 node_5059 = tex2D(_MainTex,TRANSFORM_TEX(node_6739, _MainTex));
                float2 node_422 = float2((1.0 - saturate(((node_1607+node_3095.r)*(node_1607+node_5059.r)))),0.0);
                float4 _Mask_var = tex2D(_Mask,TRANSFORM_TEX(node_422, _Mask));
                float3 emissive = ((_Mask_var.rgb*i.vertexColor.rgb*_MainColor.rgb*_Opacity)+(_MainColor.rgb*_Backcolor));
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,(_Mask_var.a*i.vertexColor.a*_MainColor.a));
                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalRGBA, fixed4(0,0,0,1));
                return finalRGBA;
            }
            ENDCG
        }
    }
}
