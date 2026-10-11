// Archipelago Kit ocean revision 0.3. Horizontal XZ surface, URP.
Shader "GameWork/World/StylizedOcean"
{
    Properties
    {
        _DeepColor ("Deep Ocean", Color) = (0.025,0.28,0.45,1)
        _MidColor ("Wave Turquoise", Color) = (0.055,0.58,0.65,1)
        _ShallowColor ("Shallow Lagoon", Color) = (0.15,0.78,0.70,1)
        _FoamColor ("Foam", Color) = (0.82,0.97,0.94,1)
        _SkyColor ("Sky Reflection Tint", Color) = (0.42,0.71,0.83,1)
        [NoScaleOffset] _WaveData ("Wave Slopes RG + Height B (Linear)", 2D) = "gray" {}
        [NoScaleOffset] _FoamMap ("Foam R + Noise G + Caustics B (Linear)", 2D) = "black" {}
        [NoScaleOffset] _VariationMap ("Large Scale Variation (Linear)", 2D) = "gray" {}
        _WaveTile ("Wave Texture Tile (World Metres)", Float) = 96
        _FoamTile ("Foam Texture Tile (World Metres)", Float) = 80
        _MacroTile ("Colour Variation Tile (World Metres)", Float) = 310
        _BandSpacing ("Swell Band Spacing (World Metres)", Float) = 32
        _WaveHeading ("Main Wave Direction (Degrees in XZ)", Range(-180,180)) = 35
        _DirectionSpread ("Cross Wave Angle (Degrees)", Range(0,100)) = 48
        _DirectionPeriod ("Direction Change Period (Seconds)", Float) = 120
        _DirectionVariation ("Direction Variation", Range(0,1)) = 0.65
        _CrestSharpness ("Crest Sharpness", Range(1,5)) = 2.8
        _CrestStrength ("Crest Normal Strength", Range(0,5)) = 1.8
        _FoamBreakup ("Foam Breakup", Range(0,1)) = 0.55
        _FoamPersistence ("Foam Behind Crests", Range(0,1)) = 0.65
        _NormalStrength ("Wave Normal Strength", Range(0,2)) = 0.55
        _FlowSpeed ("Wave Flow Speed", Range(0,3)) = 0.6
        _FoamAmount ("Whitecap Coverage", Range(0,1)) = 0.42
        _FoamBrightness ("Whitecap Brightness", Range(0,1)) = 0.85
        _ShallowDepth ("Shallow Colour Depth (Metres)", Float) = 14
        _ShoreFoamWidth ("Shore Foam Depth (Metres)", Float) = 1.2
        _SpecularStrength ("Sun Glints", Range(0,2)) = 0.75
        _SpecularPower ("Glint Sharpness", Range(8,256)) = 90
        _CausticStrength ("Underwater Light Pattern", Range(0,1)) = 0.18
        _Clarity ("Visible Seabed In Shallows", Range(0,1)) = 0.35
        _RefractionStrength ("Shallow Refraction", Range(0,0.025)) = 0.006
        [Toggle] _UseDepth ("Depth Texture Available", Float) = 1
        [Toggle] _UseSceneColor ("Opaque Texture Available", Float) = 0
        [Toggle(_WATER_MOBILE)] _Mobile ("Mobile (Fewer Texture Samples)", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Name "OceanForward"
            Tags { "LightMode"="UniversalForward" }
            // Solid colour in the transparent queue: opaque scene depth is available.
            Blend One Zero
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma shader_feature_local_fragment _WATER_MOBILE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            TEXTURE2D(_WaveData); SAMPLER(sampler_WaveData);
            TEXTURE2D(_FoamMap); SAMPLER(sampler_FoamMap);
            TEXTURE2D(_VariationMap); SAMPLER(sampler_VariationMap);
            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor, _MidColor, _ShallowColor, _FoamColor, _SkyColor;
                float _WaveTile, _FoamTile, _MacroTile, _BandSpacing;
                float _NormalStrength, _FlowSpeed, _FoamAmount, _FoamBrightness;
                float _ShallowDepth, _ShoreFoamWidth, _SpecularStrength, _SpecularPower;
                float _CausticStrength, _UseDepth, _Mobile;
                float _Clarity, _RefractionStrength, _UseSceneColor;
                float _WaveHeading, _DirectionSpread, _DirectionPeriod, _DirectionVariation;
                float _CrestSharpness, _CrestStrength, _FoamBreakup, _FoamPersistence;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half fog : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            Varyings Vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.fog = ComputeFogFactor(o.positionCS.z);
                return o;
            }
            float OceanDepth(float2 screenUV, float3 surfaceWS)
            {
                if (_UseDepth < 0.5) return 1000.0;
                float rawDepth = SampleSceneDepth(screenUV);
                #if UNITY_REVERSED_Z
                    if (rawDepth < 0.00001) return 1000.0;
                    float ndcDepth = rawDepth;
                #else
                    if (rawDepth > 0.99999) return 1000.0;
                    float ndcDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1.0, rawDepth);
                #endif
                // Inverse VP supports both orthographic and perspective cameras.
                float3 sceneWS = ComputeWorldSpacePosition(screenUV, ndcDepth, UNITY_MATRIX_I_VP);
                return max(0.0, surfaceWS.y - sceneWS.y);
            }
            // Fixed wave trains change relative strength; coordinates never rotate over time.
            float SharpSwell(float phase)
            {
                return pow(saturate(sin(phase) * 0.5 + 0.5), max(1.0, _CrestSharpness));
            }
            float SwellDerivative(float phase)
            {
                float waveBase = max(0.0001, sin(phase) * 0.5 + 0.5);
                return max(1.0, _CrestSharpness) * pow(waveBase, max(0.0, _CrestSharpness - 1.0)) * cos(phase) * 0.5;
            }
            half4 Frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float2 p = i.positionWS.xz;
                float t = _Time.y * _FlowSpeed;
                float waveTile = max(1.0, _WaveTile);
                float heading = radians(_WaveHeading);
                float2 d0 = float2(cos(heading), sin(heading));
                float2 d1 = float2(cos(heading + radians(_DirectionSpread)), sin(heading + radians(_DirectionSpread)));
                float2 d2 = float2(cos(heading - radians(_DirectionSpread) * 0.6), sin(heading - radians(_DirectionSpread) * 0.6));
                half4 a = SAMPLE_TEXTURE2D(_WaveData, sampler_WaveData,
                    p / waveTile - t * d0 * 0.016);
                float2 slope = a.rg * 2.0 - 1.0;
                half height = a.b;
                #if !defined(_WATER_MOBILE)
                    half4 b = SAMPLE_TEXTURE2D(_WaveData, sampler_WaveData,
                        p / (waveTile * 0.43) - t * d1 * 0.021);
                    slope = slope * 0.78 + (b.rg * 2.0 - 1.0) * 0.40;
                    height = height * 0.7 + b.b * 0.3;
                #endif
                half3 macro = SAMPLE_TEXTURE2D(_VariationMap, sampler_VariationMap,
                    p / max(1.0, _MacroTile) + t * float2(0.0009, -0.0005)).rgb;
                float2 foamUV = p / max(1.0, _FoamTile) + slope * 0.025 + -t * d0 * 0.012;
                half3 foamData = SAMPLE_TEXTURE2D(_FoamMap, sampler_FoamMap, foamUV).rgb;
                half lace = foamData.r;
                #if !defined(_WATER_MOBILE)
                    half3 secondFoam = SAMPLE_TEXTURE2D(_FoamMap, sampler_FoamMap,
                        foamUV * 1.61 + -t * d1 * 0.014).rgb;
                    lace = max(lace, secondFoam.r * 0.6);
                #endif
                float k = 6.283185 / max(1.0, _BandSpacing);
                float warp = (macro.g - 0.5) * 5.0 + (foamData.g - 0.5) * 0.65;
                float phase0 = dot(p, d0) * k + warp - t * 0.8;
                float phase1 = dot(p, d1) * k * 0.73 + warp * 0.7 - t * 0.61 + 1.7;
                float phase2 = dot(p, d2) * k * 1.37 - t * 1.12 + 3.1;
                float change = (sin(_Time.y * 6.283185 / max(10.0, _DirectionPeriod)) * 0.5 + 0.5) * _DirectionVariation;
                float3 weights = float3(1.0 - change * 0.5, 0.25 + change * 0.9, 0.18);
                weights /= dot(weights, float3(1,1,1));
                float3 swells = float3(SharpSwell(phase0), SharpSwell(phase1), SharpSwell(phase2));
                float swell = dot(swells, weights);
                float2 crestSlope = -_CrestStrength * k * (
                    d0 * SwellDerivative(phase0) * weights.x +
                    d1 * SwellDerivative(phase1) * weights.y * 0.73 +
                    d2 * SwellDerivative(phase2) * weights.z * 1.37);
                float2 finalSlope = slope * _NormalStrength + crestSlope;
                float3 normalWS = normalize(float3(finalSlope.x, 1.0, finalSlope.y));
                float leading = max(swells.x * weights.x, swells.y * weights.y) / max(weights.x, weights.y);
                float trailing = max(SharpSwell(phase0 + 0.9), SharpSwell(phase1 + 0.9) * 0.7);
                float crest = smoothstep(lerp(0.97, 0.25, _FoamAmount), 1.0, leading);
                float remnant = smoothstep(0.3, 0.95, trailing) * _FoamPersistence * 0.48;
                float breakup = lerp(1.0, smoothstep(0.18, 0.72,
                    foamData.g + sin(t * 0.24 + macro.b * 12.0) * 0.08), _FoamBreakup);
                float cellularFoam = saturate(lace * 1.65 + foamData.b * 0.2);
                float caps = cellularFoam * max(crest, remnant) * breakup
                    * _FoamBrightness * saturate(_FoamAmount * 5.0);
                float2 screenUV = GetNormalizedScreenSpaceUV(i.positionCS);
                float depth = OceanDepth(screenUV, i.positionWS);
                float shallow = 1.0 - smoothstep(0.0, max(0.1, _ShallowDepth), depth);
                half variation = smoothstep(0.20, 0.84, macro.r * 0.40 + height * 0.22 + swell * 0.38);
                half3 col = lerp(_DeepColor.rgb, _MidColor.rgb, variation * 0.9);
                col = lerp(col, _ShallowColor.rgb, shallow * 0.92);
                #if !defined(_WATER_MOBILE)
                    if (_UseSceneColor > 0.5 && _UseDepth > 0.5)
                    {
                        float2 refractedUV = saturate(screenUV + slope * _RefractionStrength * shallow);
                        // Do not smear a foreground island across the water boundary.
                        if (OceanDepth(refractedUV, i.positionWS) < 0.01) refractedUV = screenUV;
                        half3 seabed = SampleSceneColor(refractedUV);
                        col = lerp(col, seabed * half3(0.7, 0.97, 0.88), shallow * _Clarity);
                    }
                #endif
                col *= lerp(0.86, 1.12, height);
                Light sun = GetMainLight();
                float3 viewDir = GetWorldSpaceNormalizeViewDir(i.positionWS);
                float3 halfDir = SafeNormalize(sun.direction + viewDir);
                float diffuse = saturate(dot(normalWS, sun.direction));
                col *= (0.64 + 0.36 * diffuse) * lerp(half3(1,1,1), sun.color, 0.25);
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDir)), 4.0);
                col = lerp(col, _SkyColor.rgb, 0.035 + fresnel * 0.3);
                float spec = pow(saturate(dot(normalWS, halfDir)), _SpecularPower);
                col += sun.color * spec * _SpecularStrength * (1.0 - caps);
                col += _ShallowColor.rgb * foamData.b * shallow * _CausticStrength;
                float shoreMask = 1.0 - smoothstep(0.0, max(0.03, _ShoreFoamWidth), depth);
                float wash = sin(depth / max(0.03, _ShoreFoamWidth) * 9.0 - t * 1.3 + foamData.g * 2.0) * 0.5 + 0.5;
                float shore = shoreMask * lerp(0.12, 1.0, saturate(lace * 1.4 + wash * 0.45)) * breakup;
                float foam = saturate(max(caps, shore));
                col = lerp(col, _FoamColor.rgb, foam);
                return half4(MixFog(col, i.fog), 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
