/*
 Project-D StylizedWater
 Purpose: flat saturated water surface with glowing contour lines around everything that stands in it.
 Pipeline: URP 17 (Unity 6.4), Forward and Forward+. Target: PC, mid-spec.
 Budget: 1 texture sample, 6 value-noise taps outside loops (swell, wobble, 4 shore strokes), loop over 16 ripples (2 brush rings x 3 taps each) and 8 bodies per pixel (no texture reads in the loops).
 Keywords: standard URP main light shadow variants only.
 Inputs: _WaterSdfTex is the distance to the nearest solid on the water plane, baked per pool by WaterVolume (0..1 maps to 0.._WaterSdfMax metres).
         _WaterRipples, _WaterRippleData, _WaterRippleParams, _WaterBodies and _WaterTime are globals pushed by WaterController.
 Contact effects: one small ring per entry; a tight ring around whatever stands in the water, which becomes a crescent (thick behind, fading in front,
 plus one tapered stroke ahead) while it walks (_WaterBodyMotion); and a sparse trail of hand-brushed rings with their own shorter lifetime (_WaterRippleParams.z).
 Brush ring (BrushRing, shared by trail and entry rings): a slightly irregular circle of ~5-7 tapered strokes with uneven gaps and lumpy thickness, one side more eroded
 into specks. Pattern is seeded per patch and stable over its life; it dissolves from the stroke tips as the patch ages (erode threshold rises from the start value to _TrailErodeEnd).
 Entry rings use it with _EntryWidthScale and _EntryErodeStart (fresher, ~80% covered); strong splashes add a thinner, more eroded inner brush ring.
 Shore lines: the two contour lines are brush strokes too. World-space noise (drifting slowly) sets where each line is present, TaperedBand gives pointed tips and lumpy thickness;
 the inner line is mostly continuous (_ShoreInnerErode), the outer line is broken into short strokes and specks (_ShoreOuterErode).
*/
Shader "ProjectD/StylizedWater"
{
    Properties
    {
        [Header(Color)]
        _DeepColor ("Deep Color", Color) = (0.02, 0.09, 0.86, 1)
        _ShallowColor ("Shallow Color", Color) = (0.05, 0.30, 0.95, 1)
        _InnerColor ("Inside Contour Color", Color) = (0.12, 0.42, 1.0, 1)
        _ShadowTint ("Cast Shadow Tint", Color) = (0.16, 0.10, 0.52, 1)
        _ShallowRange ("Shallow Range (m)", Range(0.05, 6)) = 1.8
        _InnerStrength ("Inside Contour Strength", Range(0, 1)) = 0.55

        [Header(Opacity)]
        _ShallowAlpha ("Shallow Alpha", Range(0, 1)) = 0.72
        _DeepAlpha ("Deep Alpha", Range(0, 1)) = 0.97

        [Header(Swell)]
        _SwellColor ("Swell Color", Color) = (0.03, 0.20, 0.98, 1)
        _SwellScale ("Swell Scale", Range(0.02, 2)) = 0.28
        _SwellSpeed ("Swell Speed", Range(0, 1)) = 0.07
        _SwellStrength ("Swell Strength", Range(0, 1)) = 0.45

        [Header(Contour Lines)]
        [HDR] _LineColor ("Line Color", Color) = (0.55, 1.15, 2.1, 1)
        _LineOffset ("Line Offset (m)", Range(0.02, 1.5)) = 0.26
        _LineWidth ("Line Width (m)", Range(0.005, 0.2)) = 0.05
        _Line2Offset ("Second Line Offset (m)", Range(0.05, 3)) = 0.62
        _Line2Strength ("Second Line Strength", Range(0, 1)) = 0.32
        _LinePulseSpeed ("Line Pulse Speed", Range(0, 6)) = 0.9
        _LinePulseAmount ("Line Pulse Amount (m)", Range(0, 0.3)) = 0.05

        [Header(Wobble)]
        _WobbleScale ("Wobble Scale", Range(0.05, 6)) = 1.1
        _WobbleSpeed ("Wobble Speed", Range(0, 2)) = 0.22
        _WobbleAmount ("Wobble Amount (m)", Range(0, 0.4)) = 0.09

        [Header(Contact)]
        [HDR] _ContactColor ("Contact Color", Color) = (1.25, 1.55, 1.9, 1)
        _ContactGap ("Contact Ring Gap (m)", Range(0, 0.4)) = 0.05
        _ContactWidth ("Contact Ring Width (m)", Range(0.005, 0.2)) = 0.045
        _ContactBreathSpeed ("Ring Breath Speed", Range(0, 6)) = 1.6
        _ContactBreathAmount ("Ring Breath Amount (m)", Range(0, 0.1)) = 0.025
        _HeightMatch ("Same Surface Height Tolerance (m)", Range(0.05, 2)) = 0.5

        [Header(Entry Ring)]
        _EntryWidthScale ("Entry Line Width Scale", Range(0.2, 2)) = 1.0
        _EntryErodeStart ("Entry Erode Threshold At Birth", Range(0, 1)) = 0.2
        _EntryInnerErodeBoost ("Inner Ring Extra Erosion", Range(0, 0.5)) = 0.15

        [Header(Shore Strokes)]
        _ShoreStrokeScale ("Stroke Noise Scale (1/m)", Range(0.2, 8)) = 1.6
        _ShoreLumpScale ("Lump Noise Scale (1/m)", Range(1, 24)) = 6
        _ShoreStrokeDrift ("Stroke Drift Speed", Range(0, 0.5)) = 0.08
        _ShoreInnerErode ("Inner Line Erode Threshold", Range(0, 1)) = 0.18
        _ShoreOuterErode ("Outer Line Erode Threshold", Range(0, 1)) = 0.42
        _ShoreLumpAmount ("Shore Lump Amount", Range(0, 1)) = 0.2
        _ShoreStrokeSoftness ("Shore Stroke Softness (tip length)", Range(0.01, 0.5)) = 0.2

        [Header(Moving Crescent)]
        _CrescentShift ("Crescent Shift Behind (m)", Range(0, 0.3)) = 0.07
        _CrescentBackWidth ("Crescent Back Width Scale", Range(1, 4)) = 2.2
        _CrescentFrontFade ("Crescent Front Fade", Range(0, 1)) = 0.85
        _FrontArcGap ("Front Arc Gap (m)", Range(0, 0.4)) = 0.08
        _FrontArcSpan ("Front Arc Span (0 wide, 1 none)", Range(0, 1)) = 0.55
        _FrontArcStrength ("Front Arc Strength", Range(0, 1)) = 0.85

        [Header(Moving Trail)]
        _TrailWidthScale ("Trail Line Width Scale", Range(0.2, 2)) = 1.2
        _TrailFadePower ("Fade Out Power", Range(1, 4)) = 1.5
        _TrailStrokeFrequency ("Stroke Frequency (around ring)", Range(0.5, 8)) = 2.2
        _TrailLumpFrequency ("Lump Frequency", Range(1, 16)) = 7
        _TrailLumpAmount ("Lump Amount", Range(0, 1)) = 0.35
        _TrailErodeStart ("Erode Threshold At Birth", Range(0, 1)) = 0.38
        _TrailErodeEnd ("Erode Threshold At Death", Range(0, 1)) = 0.75
        _TrailStrokeSoftness ("Stroke Softness (tip length)", Range(0.01, 0.5)) = 0.14
        _TrailSideErode ("Eroded Side Strength", Range(0, 1)) = 0.25
        _TrailRadiusWobble ("Radius Wobble (m)", Range(0, 0.1)) = 0.015

        [Header(Light)]
        _NightDim ("Night Brightness", Range(0, 1)) = 0.28
        _DayLightLevel ("Full Bright Light Level", Range(0.1, 2)) = 0.8

        [Header(Baked Distance)]
        [NoScaleOffset] _WaterSdfTex ("Distance Field (set by WaterVolume)", 2D) = "white" {}
        _WaterSdfRect ("Distance Rect (min x, min z, size x, size z)", Vector) = (0, 0, 1, 1)
        _WaterSdfMax ("Distance Range (m)", Float) = 8
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent-10"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #define WATER_MAX_RIPPLES 16
            #define WATER_MAX_BODIES 8

            TEXTURE2D(_WaterSdfTex);
            SAMPLER(sampler_WaterSdfTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor;
                half4 _ShallowColor;
                half4 _InnerColor;
                half4 _ShadowTint;
                half4 _SwellColor;
                half4 _LineColor;
                half4 _ContactColor;
                float4 _WaterSdfRect;
                float _WaterSdfMax;
                float _ShallowRange;
                float _InnerStrength;
                float _ShallowAlpha;
                float _DeepAlpha;
                float _SwellScale;
                float _SwellSpeed;
                float _SwellStrength;
                float _LineOffset;
                float _LineWidth;
                float _Line2Offset;
                float _Line2Strength;
                float _LinePulseSpeed;
                float _LinePulseAmount;
                float _WobbleScale;
                float _WobbleSpeed;
                float _WobbleAmount;
                float _ContactGap;
                float _ContactWidth;
                float _ContactBreathSpeed;
                float _ContactBreathAmount;
                float _HeightMatch;
                float _EntryWidthScale;
                float _EntryErodeStart;
                float _EntryInnerErodeBoost;
                float _ShoreStrokeScale;
                float _ShoreLumpScale;
                float _ShoreStrokeDrift;
                float _ShoreInnerErode;
                float _ShoreOuterErode;
                float _ShoreLumpAmount;
                float _ShoreStrokeSoftness;
                float _CrescentShift;
                float _CrescentBackWidth;
                float _CrescentFrontFade;
                float _FrontArcGap;
                float _FrontArcSpan;
                float _FrontArcStrength;
                float _TrailWidthScale;
                float _TrailFadePower;
                float _TrailStrokeFrequency;
                float _TrailLumpFrequency;
                float _TrailLumpAmount;
                float _TrailErodeStart;
                float _TrailErodeEnd;
                float _TrailStrokeSoftness;
                float _TrailSideErode;
                float _TrailRadiusWobble;
                float _NightDim;
                float _DayLightLevel;
            CBUFFER_END

            float4 _WaterRipples[WATER_MAX_RIPPLES];
            float4 _WaterRippleData[WATER_MAX_RIPPLES];
            float4 _WaterBodies[WATER_MAX_BODIES];
            float4 _WaterBodyMotion[WATER_MAX_BODIES];
            float4 _WaterRippleParams;
            float _WaterTime;

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
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

            // Anti-aliased line of the given width centred where signedDistance is zero.
            float Band(float signedDistance, float width, float aa)
            {
                return 1.0 - smoothstep(width * 0.5, width * 0.5 + aa, abs(signedDistance));
            }

            // A line whose thickness follows the taper, so stroke tips come to a point instead of a square cut.
            float TaperedBand(float signedDistance, float width, float aa, float taper)
            {
                return Band(signedDistance, width * sqrt(taper), aa) * smoothstep(0.0, 0.25, taper);
            }

            // One irregular ring of tapered brush strokes. Noise is sampled on the unit circle (dir), so it has no seam.
            // seed: xy noise offset, z erosion side (0..1 turns), w erosion threshold jitter.
            float BrushRing(float2 offset, float radius, float width, float k, float4 seed, float erodeStart, float erodeEnd, float aa)
            {
                float len = max(length(offset), 0.0001);
                float2 dir = offset / len;
                float strokeNoise = ValueNoise(dir * _TrailStrokeFrequency + seed.xy);
                float lumpNoise = ValueNoise(dir * _TrailLumpFrequency + seed.xy * 1.7 + 11.3);
                float wobbleNoise = ValueNoise(dir * _TrailLumpFrequency * 0.5 + seed.xy * 0.5 + 27.0);
                // Per-patch erosion side: the part of the ring facing a random direction loses coverage first.
                float sideAngle = seed.z * 6.2831853;
                float sideBias = _TrailSideErode * saturate(dot(dir, float2(cos(sideAngle), sin(sideAngle))));
                float strokeField = strokeNoise + (lumpNoise - 0.5) * _TrailLumpAmount - sideBias;
                // Coverage threshold rises with age, so strokes shrink from the tips and thin parts break into specks.
                float erodeLevel = lerp(erodeStart, erodeEnd, k) + (seed.w - 0.5) * 0.1;
                float strokeAmount = saturate((strokeField - erodeLevel) / _TrailStrokeSoftness);
                float ringRadius = radius + (wobbleNoise - 0.5) * _TrailRadiusWobble;
                return TaperedBand(len - ringRadius, width, aa, strokeAmount);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 positionWS = input.positionWS;
                float2 p = positionWS.xz;
                float time = _WaterTime;

                float2 sdfUv = (p - _WaterSdfRect.xy) / _WaterSdfRect.zw;
                float distanceToSolid = SAMPLE_TEXTURE2D(_WaterSdfTex, sampler_WaterSdfTex, sdfUv).r * _WaterSdfMax;

                float wobble = (ValueNoise(p * _WobbleScale + time * _WobbleSpeed) - 0.5) * 2.0 * _WobbleAmount;
                float wobbled = distanceToSolid + wobble;
                float pulse = sin(time * _LinePulseSpeed) * _LinePulseAmount;
                float aa = max(fwidth(wobbled), 0.002);

                float depthFade = smoothstep(0.0, _ShallowRange, distanceToSolid);
                half3 color = lerp(_ShallowColor.rgb, _DeepColor.rgb, depthFade);

                float swell = ValueNoise(p * _SwellScale + float2(time, -time * 0.6) * _SwellSpeed);
                color = lerp(color, _SwellColor.rgb, smoothstep(0.45, 0.8, swell) * _SwellStrength);

                float inside = 1.0 - smoothstep(-aa, aa, wobbled - (_LineOffset + pulse));
                color = lerp(color, _InnerColor.rgb, inside * _InnerStrength);

                // Shore strokes: world-space noise has no angle along a contour, so it decides where each line is present.
                // Drift makes the break pattern crawl slowly; each line has its own seed offset, direction and erode threshold.
                float2 strokeDrift1 = float2(0.8, 0.6) * (time * _ShoreStrokeDrift);
                float2 strokeDrift2 = float2(-0.6, 0.8) * (time * _ShoreStrokeDrift);
                float2 strokeP1 = p * _ShoreStrokeScale + strokeDrift1 + float2(3.7, 8.1);
                float2 strokeP2 = p * _ShoreStrokeScale + strokeDrift2 + float2(41.3, 17.9);
                float2 lumpP1 = p * _ShoreLumpScale + strokeDrift1 * 0.5 + float2(23.1, 5.3);
                float2 lumpP2 = p * _ShoreLumpScale + strokeDrift2 * 0.5 + float2(7.7, 61.9);
                float shoreField1 = ValueNoise(strokeP1) + (ValueNoise(lumpP1) - 0.5) * _ShoreLumpAmount;
                float shoreField2 = ValueNoise(strokeP2) + (ValueNoise(lumpP2) - 0.5) * _ShoreLumpAmount;
                float shoreAmount1 = saturate((shoreField1 - _ShoreInnerErode) / _ShoreStrokeSoftness);
                float shoreAmount2 = saturate((shoreField2 - _ShoreOuterErode) / _ShoreStrokeSoftness);

                float lineMask = TaperedBand(wobbled - (_LineOffset + pulse), _LineWidth, aa, shoreAmount1);
                lineMask = max(lineMask, TaperedBand(wobbled - (_Line2Offset - pulse), _LineWidth * 0.6, aa, shoreAmount2) * _Line2Strength);

                float contactMask = 0.0;

                float entryLife = max(_WaterRippleParams.x, 0.001);
                float trailLife = lerp(entryLife, max(_WaterRippleParams.z, 0.001), step(0.001, _WaterRippleParams.z));
                float rippleWidth = _WaterRippleParams.y;

                for (int r = 0; r < WATER_MAX_RIPPLES; r++)
                {
                    float4 ripple = _WaterRipples[r];
                    float4 data = _WaterRippleData[r];
                    float strength = data.x;

                    // data.w > 0 marks a trail patch: thin ring, broken outer dashes, faint fill, shorter life.
                    float isTrail = step(0.001, data.w);
                    float life = lerp(entryLife, trailLife, isTrail);
                    float age = time - ripple.w;
                    float k = saturate(age / life);
                    float alive = step(0.0, age) * step(age, life) * step(abs(positionWS.y - ripple.y), _HeightMatch) * step(0.001, strength);

                    // Cubic ease-out: the ring pops open, then settles and fades at its small final radius.
                    float ease = 1.0 - (1.0 - k) * (1.0 - k) * (1.0 - k);
                    float radius = lerp(data.y, data.z, ease);
                    float2 rippleOffset = p - ripple.xz;
                    float width = rippleWidth * (1.0 - 0.55 * k);
                    float fade = pow(1.0 - k, lerp(1.0, _TrailFadePower, isTrail)) * alive;

                    // Per-patch random seed: xy noise offset, z erosion side (0..1 turns), w erosion threshold jitter.
                    float4 ringSeed = float4(frac(ripple.w * 1.37 + ripple.x * 3.71) * 37.0, frac(ripple.w * 2.11 + ripple.z * 5.13) * 37.0,
                        frac(ripple.w * 3.37 + ripple.x * 1.91), frac(ripple.w * 5.77 + ripple.z * 2.9));
                    float4 innerSeed = float4(ringSeed.xy * 1.3 + 53.7, frac(ringSeed.z + 0.43), frac(ringSeed.w + 0.29));

                    // Main ring: entry and trail share the brush-stroke ring, only width and erode start differ.
                    float ringWidth = width * lerp(_EntryWidthScale, _TrailWidthScale, isTrail);
                    float ringErodeStart = lerp(_EntryErodeStart, _TrailErodeStart, isTrail);
                    float mainRing = BrushRing(rippleOffset, radius, ringWidth, k, ringSeed, ringErodeStart, _TrailErodeEnd, aa);

                    // Faint inner ring of strong entry splashes: thinner, more eroded, own seed.
                    float innerGate = smoothstep(0.35, 0.7, strength);
                    float innerRadius = lerp(data.y, radius, 0.55);
                    float innerRing = BrushRing(rippleOffset, innerRadius, ringWidth * 0.7, k, innerSeed,
                        _EntryErodeStart + _EntryInnerErodeBoost, _TrailErodeEnd + _EntryInnerErodeBoost, aa) * 0.6 * innerGate * (1.0 - isTrail);

                    float rings = max(mainRing, innerRing);
                    contactMask = max(contactMask, rings * fade);
                }

                for (int b = 0; b < WATER_MAX_BODIES; b++)
                {
                    float4 body = _WaterBodies[b];
                    float active = step(0.001, body.w) * step(abs(positionWS.y - body.y), _HeightMatch);
                    // motion = (direction x, direction z, speed 0..1). Standing still gives a plain ring; walking
                    // slides the ring behind the body and turns it into a crescent: thick behind, fading out in front.
                    float4 motion = _WaterBodyMotion[b];
                    float2 heading = motion.xy;
                    float moving = saturate(motion.z);
                    float2 centre = body.xz - heading * _CrescentShift * moving;
                    float2 bodyOffset = p - centre;
                    float bodyDistance = length(bodyOffset) - body.w;
                    float2 bodyDir = bodyOffset / max(length(bodyOffset), 0.0001);
                    float front = dot(bodyDir, heading);
                    float back = saturate(-front);

                    float breath = sin(time * _ContactBreathSpeed + b * 1.7) * _ContactBreathAmount * (1.0 - moving);
                    // Same lumpy brush thickness as the trail strokes; sampled by direction so it stays put while the body walks.
                    float bodyLump = 1.0 + (ValueNoise(bodyDir * _TrailLumpFrequency + b * 5.31) - 0.5) * 2.0 * _TrailLumpAmount;
                    float crescentWidth = _ContactWidth * lerp(1.0, lerp(0.35, _CrescentBackWidth, back), moving) * bodyLump;
                    float frontKeep = lerp(1.0, 1.0 - _CrescentFrontFade * smoothstep(-0.2, 0.9, front), moving);
                    float ring = Band(bodyDistance - (_ContactGap + breath + crescentWidth * 0.5), crescentWidth, aa) * frontKeep;

                    // One tapered stroke just ahead of the body, pointed at both tips.
                    float arcTaper = saturate((front - _FrontArcSpan) / max(1.0 - _FrontArcSpan, 0.001));
                    float frontArc = TaperedBand(bodyDistance - (_ContactGap + _FrontArcGap), _ContactWidth * bodyLump, aa, arcTaper) * _FrontArcStrength * moving;

                    contactMask = max(contactMask, max(ring, frontArc) * active);
                }

                contactMask = saturate(contactMask);

                Light mainLight = GetMainLight(TransformWorldToShadowCoord(positionWS));
                half shadow = mainLight.shadowAttenuation;
                half lightLevel = saturate(dot(mainLight.color, half3(0.3, 0.59, 0.11)) / _DayLightLevel);
                half brightness = lerp(_NightDim, 1.0, lightLevel);

                color = lerp(color * _ShadowTint.rgb * 2.0, color, shadow) * brightness;
                half3 lineColor = _LineColor.rgb * lerp(0.55, 1.0, shadow) * brightness;
                color = lerp(color, lineColor, lineMask);
                color = lerp(color, _ContactColor.rgb * lerp(0.6, 1.0, shadow) * brightness, contactMask);

                float alpha = lerp(_ShallowAlpha, _DeepAlpha, depthFade);
                alpha = max(alpha, max(lineMask, contactMask));

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
