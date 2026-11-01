/*
 Project-D StylizedPainterly
 Purpose: soft painterly toon look (no outline) shared by characters and environment.
 Pipeline: URP 17 (Unity 6.4), Forward and Forward+ (_CLUSTER_LIGHT_LOOP). Target: PC, mid-spec.
 Budget: 1 texture sample per pass, 2 value-noise taps (UV) or 6 (world), no dynamic branches.
 Keywords: _ALPHATEST_ON (foliage cutout), _REVEAL_ON (weapon draw/sheathe cut) plus standard URP light, shadow and fog variants.
 Reveal: object-space cut along _RevealAxis between _RevealRange.x and .y, glowing edge, 2 value-noise taps; driven per renderer by WeaponRevealer.
 Wind: vertex sway driven by object-space height, keep _WindStrength 0 for characters and rigid props.
*/
Shader "ProjectD/StylizedPainterly"
{
    Properties
    {
        [Header(Surface)]
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Saturation ("Saturation", Range(0, 2)) = 1.08
        [Toggle(_ALPHATEST_ON)] _AlphaClip ("Alpha Clip", Float) = 0
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2

        [Header(Painterly Light)]
        _ShadowColor ("Shadow Tint", Color) = (0.70, 0.64, 0.84, 1)
        _MidColor ("Mid Tint", Color) = (1, 1, 1, 1)
        _HighlightColor ("Highlight Tint", Color) = (1.10, 1.05, 0.92, 1)
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.42
        _HighlightThreshold ("Highlight Threshold", Range(0, 1)) = 0.82
        _Softness ("Band Softness", Range(0.01, 0.5)) = 0.2
        _HighlightStrength ("Highlight Strength", Range(0, 1)) = 0.55
        _LightInfluence ("Light Color Influence", Range(0, 1)) = 0.6
        _AmbientStrength ("Ambient Strength", Range(0, 1.5)) = 0.45
        _ShadowReceive ("Cast Shadow Strength", Range(0, 1)) = 0.85
        _AdditionalLightStrength ("Additional Light Strength", Range(0, 2)) = 0.8

        [Header(Brush)]
        _PaintScale ("Brush Scale", Range(0.5, 60)) = 14
        _PaintStrength ("Brush Strength", Range(0, 0.5)) = 0
        _PaintColorVariation ("Brush Color Variation", Range(0, 0.4)) = 0
        _PaintWorld ("Brush Space (0 UV, 1 World)", Range(0, 1)) = 0

        [Header(Rim and Fuzz)]
        _RimColor ("Rim Color", Color) = (1, 0.88, 0.62, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 2.6
        _RimStrength ("Rim Strength", Range(0, 1.5)) = 0.35
        _RimSoftness ("Rim Softness", Range(0.01, 0.6)) = 0.3
        _RimLightMask ("Rim Lit-Side Mask", Range(0, 1)) = 0.6

        [Header(Specular)]
        _SpecColor ("Specular Color", Color) = (1, 0.97, 0.88, 1)
        _SpecSize ("Specular Size", Range(0.01, 1)) = 0.12
        _SpecSoftness ("Specular Softness", Range(0.01, 0.5)) = 0.18
        _SpecStrength ("Specular Strength", Range(0, 1)) = 0

        [Header(Emission)]
        [HDR] _EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)

        [Header(Wind)]
        _WindStrength ("Wind Strength", Range(0, 0.5)) = 0
        _WindSpeed ("Wind Speed", Range(0, 8)) = 1.6
        _WindScale ("Wind Scale", Range(0, 3)) = 0.5

        [Header(Reveal)]
        [Toggle(_REVEAL_ON)] _RevealOn ("Reveal Cut", Float) = 0
        _RevealProgress ("Reveal Progress", Range(0, 1)) = 1
        _RevealAxis ("Reveal Axis (Object)", Vector) = (0, 1, 0, 0)
        _RevealRange ("Reveal Range (Min, Max)", Vector) = (0, 1, 0, 0)
        _RevealEdgeWidth ("Reveal Edge Width", Range(0.01, 0.4)) = 0.08
        [HDR] _RevealEdgeColor ("Reveal Edge Color", Color) = (2.4, 1.6, 0.6, 1)
        _RevealNoiseScale ("Reveal Noise Scale", Range(1, 60)) = 22
        _RevealNoiseStrength ("Reveal Noise Strength", Range(0, 0.3)) = 0.06
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
            "UniversalMaterialType" = "SimpleLit"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half _Saturation;
            half _Cutoff;
            half4 _ShadowColor;
            half4 _MidColor;
            half4 _HighlightColor;
            half _ShadowThreshold;
            half _HighlightThreshold;
            half _Softness;
            half _HighlightStrength;
            half _LightInfluence;
            half _AmbientStrength;
            half _ShadowReceive;
            half _AdditionalLightStrength;
            half _PaintScale;
            half _PaintStrength;
            half _PaintColorVariation;
            half _PaintWorld;
            half4 _RimColor;
            half _RimPower;
            half _RimStrength;
            half _RimSoftness;
            half _RimLightMask;
            half4 _SpecColor;
            half _SpecSize;
            half _SpecSoftness;
            half _SpecStrength;
            half4 _EmissionColor;
            half _WindStrength;
            half _WindSpeed;
            half _WindScale;
            half _RevealProgress;
            float4 _RevealAxis;
            float4 _RevealRange;
            half _RevealEdgeWidth;
            half4 _RevealEdgeColor;
            half _RevealNoiseScale;
            half _RevealNoiseStrength;
        CBUFFER_END

        float Hash21(float2 p)
        {
            p = frac(p * float2(123.34, 456.21));
            p += dot(p, p + 45.32);
            return frac(p.x * p.y);
        }

        float ValueNoise(float2 p)
        {
            float2 i = floor(p);
            float2 f = frac(p);
            f = f * f * (3.0 - 2.0 * f);
            float a = Hash21(i);
            float b = Hash21(i + float2(1, 0));
            float c = Hash21(i + float2(0, 1));
            float d = Hash21(i + float2(1, 1));
            return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
        }

        /*
         Signed distance (in 0..1 units of the reveal range) from the reveal front to this fragment.
         along: 0 at _RevealRange.x, 1 at _RevealRange.y. noise jitters the front by +-strength/2.
         front sweeps from just below the lowest jittered point (progress 0, nothing visible)
         to past the highest point plus the edge band (progress 1, fully visible with no glow).
         Positive = visible; values below _RevealEdgeWidth fall inside the glowing edge.
        */
        float RevealDistance(float3 positionOS)
        {
            float span = max(_RevealRange.y - _RevealRange.x, 1e-4);
            float along = (dot(positionOS, _RevealAxis.xyz) - _RevealRange.x) / span;
            float jitter = (ValueNoise(positionOS.xy * _RevealNoiseScale) * 0.5
                          + ValueNoise(positionOS.yz * _RevealNoiseScale + 7.1) * 0.5 - 0.5) * _RevealNoiseStrength;
            float halfJitter = 0.5 * _RevealNoiseStrength;
            float front = lerp(-halfJitter - 1e-3, 1.0 + halfJitter + _RevealEdgeWidth, _RevealProgress);
            return front - (along + jitter);
        }

        float3 ApplyWind(float3 positionOS)
        {
            float mask = saturate(positionOS.y);
            float3 positionWS = TransformObjectToWorld(positionOS);
            float phase = _Time.y * _WindSpeed + (positionWS.x + positionWS.z) * _WindScale;
            float sway = sin(phase) + 0.5 * sin(phase * 2.3 + 1.7);
            positionOS.x += sway * _WindStrength * mask * mask;
            positionOS.z += cos(phase * 0.8) * _WindStrength * 0.6 * mask * mask;
            return positionOS;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull [_Cull]
            ZWrite On

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local _REVEAL_ON

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                half fogFactor : TEXCOORD3;
                #if defined(_REVEAL_ON)
                    float3 positionOS : TEXCOORD4;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float BrushNoise(float2 p)
            {
                return ValueNoise(p) * 0.6 + ValueNoise(p * 2.7 + 11.3) * 0.4;
            }

            float PaintSample(float2 uv, float3 positionWS, float3 normalWS)
            {
                float uvNoise = BrushNoise(uv * _PaintScale);
                float3 w = abs(normalWS);
                w /= max(w.x + w.y + w.z, 1e-4);
                float s = _PaintScale * 0.35;
                float worldNoise = BrushNoise(positionWS.zy * s) * w.x
                                 + BrushNoise(positionWS.xz * s) * w.y
                                 + BrushNoise(positionWS.xy * s) * w.z;
                return lerp(uvNoise, worldNoise, _PaintWorld) - 0.5;
            }

            half3 AdjustSaturation(half3 c, half s)
            {
                half l = dot(c, half3(0.299, 0.587, 0.114));
                return lerp(l.xxx, c, s);
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 positionOS = ApplyWind(input.positionOS.xyz);
                VertexPositionInputs vp = GetVertexPositionInputs(positionOS);
                output.positionCS = vp.positionCS;
                output.positionWS = vp.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogFactor = ComputeFogFactor(vp.positionCS.z);
                #if defined(_REVEAL_ON)
                    output.positionOS = input.positionOS.xyz;
                #endif
                return output;
            }

            half3 ToneRamp(half3 albedo, half lit)
            {
                half s = _Softness;
                half t1 = smoothstep(_ShadowThreshold - s, _ShadowThreshold + s, lit);
                half t2 = smoothstep(_HighlightThreshold - s, _HighlightThreshold + s, lit);
                half3 c = lerp(albedo * _ShadowColor.rgb, albedo * _MidColor.rgb, t1);
                c = lerp(c, albedo * _HighlightColor.rgb, t2 * _HighlightStrength);
                return c;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                #if defined(_ALPHATEST_ON)
                    clip(tex.a * _BaseColor.a - _Cutoff);
                #endif

                half revealGlow = 0;
                #if defined(_REVEAL_ON)
                    float reveal = RevealDistance(input.positionOS);
                    clip(reveal);
                    revealGlow = 1.0 - saturate(reveal / _RevealEdgeWidth);
                #endif

                float3 normalWS = normalize(input.normalWS);
                float3 viewWS = GetWorldSpaceNormalizeViewDir(input.positionWS);

                float paint = PaintSample(input.uv, input.positionWS, normalWS);

                half3 albedo = tex.rgb * _BaseColor.rgb;
                albedo = AdjustSaturation(albedo, _Saturation);
                albedo *= 1.0 + paint * 2.0 * _PaintColorVariation;

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewWS;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);

                half4 shadowMask = half4(1, 1, 1, 1);
                Light mainLight = GetMainLight(inputData.shadowCoord, input.positionWS, shadowMask);

                half ndl = dot(normalWS, mainLight.direction);
                half wrapped = ndl * 0.5 + 0.5;
                half shadowTerm = lerp(1.0, mainLight.shadowAttenuation, _ShadowReceive);
                half lit = (wrapped + paint * _PaintStrength) * shadowTerm;

                half3 lightTint = lerp(half3(1, 1, 1), saturate(mainLight.color), _LightInfluence);
                half3 color = ToneRamp(albedo, lit) * lightTint;

                half3 ambient = SampleSH(normalWS);
                color += albedo * ambient * _AmbientStrength;

                half3 extra = 0;
                #if defined(_ADDITIONAL_LIGHTS)
                    uint pixelLightCount = GetAdditionalLightsCount();
                    #if USE_CLUSTER_LIGHT_LOOP
                    [loop] for (uint dirIndex = 0; dirIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); dirIndex++)
                    {
                        Light dl = GetAdditionalLight(dirIndex, input.positionWS, shadowMask);
                        half dndl = saturate(dot(normalWS, dl.direction) * 0.5 + 0.5);
                        extra += albedo * dl.color * dl.distanceAttenuation * dl.shadowAttenuation * smoothstep(0.3, 0.85, dndl);
                    }
                    #endif
                    LIGHT_LOOP_BEGIN(pixelLightCount)
                        Light al = GetAdditionalLight(lightIndex, input.positionWS, shadowMask);
                        half andl = saturate(dot(normalWS, al.direction) * 0.5 + 0.5);
                        half band = smoothstep(0.3, 0.85, andl);
                        extra += albedo * al.color * al.distanceAttenuation * al.shadowAttenuation * band;
                    LIGHT_LOOP_END
                #endif
                color += extra * _AdditionalLightStrength;

                half fres = pow(1.0 - saturate(dot(normalWS, viewWS)), _RimPower);
                half rim = smoothstep(0.5 - _RimSoftness, 0.5 + _RimSoftness, fres);
                half litSide = lerp(1.0, saturate(wrapped * shadowTerm), _RimLightMask);
                color += _RimColor.rgb * rim * _RimStrength * litSide * lightTint;

                float3 halfDir = normalize(mainLight.direction + viewWS);
                half nh = saturate(dot(normalWS, halfDir));
                half spec = smoothstep(1.0 - _SpecSize - _SpecSoftness, 1.0 - _SpecSize + _SpecSoftness, nh);
                color += _SpecColor.rgb * spec * _SpecStrength * shadowTerm * lightTint;

                color += _EmissionColor.rgb;
                color += _RevealEdgeColor.rgb * revealGlow;
                color = MixFog(color, input.fogFactor);
                return half4(color, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local _REVEAL_ON
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                #if defined(_REVEAL_ON)
                    float3 positionOS : TEXCOORD1;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings ShadowVert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 positionOS = ApplyWind(input.positionOS.xyz);
                float3 positionWS = TransformObjectToWorld(positionOS);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                output.positionCS = positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                #if defined(_REVEAL_ON)
                    output.positionOS = input.positionOS.xyz;
                #endif
                return output;
            }

            half4 ShadowFrag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                #if defined(_REVEAL_ON)
                    clip(RevealDistance(input.positionOS));
                #endif
                #if defined(_ALPHATEST_ON)
                    half a = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a * _BaseColor.a;
                    clip(a - _Cutoff);
                #endif
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local _REVEAL_ON
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                #if defined(_REVEAL_ON)
                    float3 positionOS : TEXCOORD1;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings DepthVert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionCS = TransformObjectToHClip(ApplyWind(input.positionOS.xyz));
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                #if defined(_REVEAL_ON)
                    output.positionOS = input.positionOS.xyz;
                #endif
                return output;
            }

            half DepthFrag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                #if defined(_REVEAL_ON)
                    clip(RevealDistance(input.positionOS));
                #endif
                #if defined(_ALPHATEST_ON)
                    half a = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a * _BaseColor.a;
                    clip(a - _Cutoff);
                #endif
                return input.positionCS.z;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex NormVert
            #pragma fragment NormFrag
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local _REVEAL_ON
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                #if defined(_REVEAL_ON)
                    float3 positionOS : TEXCOORD2;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings NormVert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionCS = TransformObjectToHClip(ApplyWind(input.positionOS.xyz));
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                #if defined(_REVEAL_ON)
                    output.positionOS = input.positionOS.xyz;
                #endif
                return output;
            }

            half4 NormFrag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                #if defined(_REVEAL_ON)
                    clip(RevealDistance(input.positionOS));
                #endif
                #if defined(_ALPHATEST_ON)
                    half a = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a * _BaseColor.a;
                    clip(a - _Cutoff);
                #endif
                return half4(NormalizeNormalPerPixel(input.normalWS), 0.0);
            }
            ENDHLSL
        }
    }

    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}
