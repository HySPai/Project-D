/*
 Project-D StylizedWaterfall
 Purpose: falling sheet of water that pairs with StylizedWater: saturated blue, white streaks, foam at the lip and the base.
 Pipeline: URP 17 (Unity 6.4), Forward and Forward+. Target: PC, mid-spec.
 Budget: no textures, 3 value-noise taps, opaque.
 Mesh: uv.y must run 0 at the base to 1 at the lip. Keywords: none.
*/
Shader "ProjectD/StylizedWaterfall"
{
    Properties
    {
        [Header(Color)]
        _TopColor ("Top Color", Color) = (0.05, 0.28, 0.98, 1)
        _BottomColor ("Bottom Color", Color) = (0.02, 0.10, 0.82, 1)
        [HDR] _FoamColor ("Foam Color", Color) = (1.5, 1.6, 1.75, 1)

        [Header(Streaks)]
        _StreakScaleX ("Streak Width Scale", Range(0.5, 30)) = 7
        _StreakScaleY ("Streak Length Scale", Range(0.02, 4)) = 0.35
        _FlowSpeed ("Flow Speed", Range(0, 10)) = 2.6
        _StreakThreshold ("Streak Threshold", Range(0, 1)) = 0.8
        _StreakSoftness ("Streak Softness", Range(0.001, 0.2)) = 0.02
        _StreakStrength ("Streak Strength", Range(0, 1)) = 0.75

        [Header(Foam)]
        _LipHeight ("Lip Foam Height (uv)", Range(0, 0.5)) = 0.1
        _BaseHeight ("Base Foam Height (uv)", Range(0, 0.5)) = 0.1
        _FoamNoiseScale ("Foam Noise Scale", Range(0.5, 20)) = 5
        _NightDim ("Night Brightness", Range(0, 1)) = 0.28
        _DayLightLevel ("Full Bright Light Level", Range(0.1, 2)) = 0.8
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _TopColor;
                half4 _BottomColor;
                half4 _FoamColor;
                float _StreakScaleX;
                float _StreakScaleY;
                float _FlowSpeed;
                float _StreakThreshold;
                float _StreakSoftness;
                float _StreakStrength;
                float _LipHeight;
                float _BaseHeight;
                float _FoamNoiseScale;
                float _NightDim;
                float _DayLightLevel;
            CBUFFER_END

            float _WaterTime;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
                output.uv = input.uv;
                return output;
            }

            float Hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 cell = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = Hash(cell);
                float b = Hash(cell + float2(1, 0));
                float c = Hash(cell + float2(0, 1));
                float d = Hash(cell + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float across = input.positionWS.x + input.positionWS.z;
                float flow = input.positionWS.y + _WaterTime * _FlowSpeed;

                half3 color = lerp(_BottomColor.rgb, _TopColor.rgb, input.uv.y);

                float streakNoise = ValueNoise(float2(across * _StreakScaleX, flow * _StreakScaleY));
                float streak = smoothstep(_StreakThreshold, _StreakThreshold + _StreakSoftness, streakNoise) * _StreakStrength;

                float lipNoise = ValueNoise(float2(across * _FoamNoiseScale, flow * 0.5));
                float lip = step(1.0 - _LipHeight * (0.35 + lipNoise * 1.3), input.uv.y);

                float baseNoise = ValueNoise(float2(across * _FoamNoiseScale, _WaterTime * 0.9));
                float baseFoam = step(input.uv.y, _BaseHeight * (0.35 + baseNoise * 1.3));

                float foam = saturate(max(max(streak, lip), baseFoam));

                half lightLevel = saturate(dot(GetMainLight().color, half3(0.3, 0.59, 0.11)) / _DayLightLevel);
                half brightness = lerp(_NightDim, 1.0, lightLevel);

                return half4(lerp(color, _FoamColor.rgb, foam) * brightness, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
