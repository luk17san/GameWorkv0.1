Shader "GameWork/Water/Realistic Ocean URP"
{
    Properties
    {
        [Header(Water Colors)]
        _ShallowColor("Shallow Water", Color) = (0.12, 0.55, 0.58, 0.72)
        _DeepColor("Deep Water", Color) = (0.015, 0.12, 0.22, 0.9)
        _FoamColor("Foam", Color) = (0.82, 0.94, 0.91, 1)
        _DepthRange("Depth Color Range", Range(0.1, 30)) = 8
        _Opacity("Opacity", Range(0.05, 1)) = 0.82
        [Header(Waves)]
        _WaveHeight("Wave Height", Range(0, 1.5)) = 0.22
        _WaveScale("Wave Scale", Range(0.1, 4)) = 1
        _WaveSpeed("Wave Speed", Range(0, 3)) = 0.8
        _WaveDirection("Wave Direction", Vector) = (0.8, 0, 0.6, 0)
        [Header(Foam)]
        _FoamDistance("Shore Foam Width", Range(0.05, 3)) = 0.7
        _FoamStrength("Foam Strength", Range(0, 2)) = 0.75
        [Header(Lighting)]
        _Smoothness("Smoothness", Range(0, 1)) = 0.88
        _FresnelPower("Fresnel Power", Range(0.5, 8)) = 4
        _FresnelStrength("Fresnel Strength", Range(0, 1)) = 0.45
        _SunSpecular("Sun Glint", Range(0, 3)) = 1.2
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ShallowColor, _DeepColor, _FoamColor;
                float _DepthRange, _Opacity, _WaveHeight, _WaveScale, _WaveSpeed;
                float4 _WaveDirection;
                float _FoamDistance, _FoamStrength, _Smoothness, _FresnelPower;
                float _FresnelStrength, _SunSpecular;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            float WaveHeight(float2 p, float t)
            {
                float2 d = normalize(_WaveDirection.xz + float2(0.0001, 0.0002));
                float scale = max(_WaveScale, 0.01);
                float a = dot(p, d) * 0.75 * scale + t * _WaveSpeed;
                float b = dot(p, float2(-d.y, d.x)) * 1.17 * scale - t * _WaveSpeed * 0.73;
                float c = dot(p, normalize(d + float2(0.65, 0.42))) * 1.83 * scale + t * _WaveSpeed * 0.52;
                return _WaveHeight * (0.52 * sin(a) + 0.28 * sin(b) + 0.20 * sin(c));
            }

            float2 WaveGradient(float2 p, float t)
            {
                float2 d = normalize(_WaveDirection.xz + float2(0.0001, 0.0002));
                float2 d2 = float2(-d.y, d.x);
                float2 d3 = normalize(d + float2(0.65, 0.42));
                float scale = max(_WaveScale, 0.01);
                float a = dot(p, d) * 0.75 * scale + t * _WaveSpeed;
                float b = dot(p, d2) * 1.17 * scale - t * _WaveSpeed * 0.73;
                float c = dot(p, d3) * 1.83 * scale + t * _WaveSpeed * 0.52;
                return _WaveHeight * (0.52 * cos(a) * 0.75 * scale * d + 0.28 * cos(b) * 1.17 * scale * d2 + 0.20 * cos(c) * 1.83 * scale * d3);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 world = TransformObjectToWorld(input.positionOS.xyz);
                world.y += WaveHeight(world.xz, _Time.y);
                output.positionWS = world;
                output.positionCS = TransformWorldToHClip(world);
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.viewDirWS = GetWorldSpaceViewDir(world);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.screenPos.xy / max(input.screenPos.w, 0.0001);
                float rawDepth = SampleSceneDepth(uv);
                float sceneEyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float surfaceEyeDepth = -TransformWorldToView(input.positionWS).z;
                float thickness = max(sceneEyeDepth - surfaceEyeDepth, 0.0);
                float depthBlend = saturate(thickness / max(_DepthRange, 0.01));

                float2 grad = WaveGradient(input.positionWS.xz, _Time.y);
                float3 normalWS = normalize(float3(-grad.x, 1.0, -grad.y));
                float3 viewDir = normalize(input.viewDirWS);
                float ndv = saturate(dot(normalWS, viewDir));
                float fresnel = pow(1.0 - ndv, _FresnelPower) * _FresnelStrength;

                half3 color = lerp(_ShallowColor.rgb, _DeepColor.rgb, depthBlend);
                color = lerp(color, _FoamColor.rgb, fresnel * 0.55);

                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                float3 lightDir = normalize(mainLight.direction);
                float3 halfDir = normalize(lightDir + viewDir);
                float diffuse = saturate(dot(normalWS, lightDir));
                float spec = pow(saturate(dot(normalWS, halfDir)), lerp(32.0, 256.0, _Smoothness)) * _SunSpecular;
                color *= (0.62 + 0.38 * diffuse * mainLight.shadowAttenuation);
                color += mainLight.color * spec;
                color += fresnel * _DeepColor.rgb * 1.5;

                float foam = (1.0 - smoothstep(0.02, max(_FoamDistance, 0.03), thickness)) * _FoamStrength;
                float ripple = 0.78 + 0.22 * sin((input.positionWS.x + input.positionWS.z) * 5.0 + _Time.y * 1.7);
                foam *= ripple;
                color = lerp(color, _FoamColor.rgb, saturate(foam));

                half alpha = saturate(_Opacity + foam * 0.12);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
