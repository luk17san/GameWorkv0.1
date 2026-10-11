Shader "GameWork/World/ShipWake"
{
    Properties
    {
        [NoScaleOffset] _FoamMap ("Foam Pattern (Linear Data)", 2D) = "white" {}
        _Color ("Wake Foam", Color) = (0.8,0.97,0.95,1)
        _Turbulence ("Turbulence Speed", Range(0,2)) = 0.45
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+20" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_FoamMap); SAMPLER(sampler_FoamMap);
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _Turbulence;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; half fog : TEXCOORD1; };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                o.color = v.color;
                o.fog = ComputeFogFactor(o.positionCS.z);
                return o;
            }
            half4 Frag(Varyings i) : SV_Target
            {
                float time = _Time.y * _Turbulence;
                half3 foam = SAMPLE_TEXTURE2D(_FoamMap, sampler_FoamMap,
                    i.uv * float2(.22,.18) + float2(time*.012, -time*.018)).rgb;
                half3 bubbles = SAMPLE_TEXTURE2D(_FoamMap, sampler_FoamMap,
                    i.uv * float2(.41,.32) + float2(-time*.019, time*.013)).rgb;
                float edge = smoothstep(0, .24, i.uv.x) * smoothstep(0, .24, 1-i.uv.x);
                float brokenEdge = smoothstep(.06, .7, edge - (foam.g-.5)*.45);
                float pattern = saturate(foam.r*1.9 + bubbles.r*.7 + smoothstep(.38,.72,foam.g)*.33);
                half alpha = saturate(i.color.a * _Color.a * brokenEdge * pattern);
                return half4(MixFog(_Color.rgb, i.fog), alpha);
            }
            ENDHLSL
        }
    }
}
