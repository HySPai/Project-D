Shader "Demo/Lit Stencil Reference"
{
    Properties
    {
        _Albedo("Albedo Color", Color) = (1.0, 1.0, 1.0)
        _AlbedoMap("Albedo Map", 2D) = "white" {}

        [Space(30)]_Metallic("Metallic", Range(0 , 1)) = 0
        _Smoothness("Smoothness", Range(0 , 1)) = 0.3
        _Specular("Specular Color", Color) = (1.0, 1.0, 1.0)
        _Occlusion("Occlusion", float) = 1.0
        _Emission("Emission Color", Color) = (0.0, 0.0, 0.0)
        _Alpha("Alpha", float) = 1.0

        [Space(30)]_RimPower("Rim Power", float) = 4.0
        _RimColor("Rim Color", Color) = (1.0, 1.0, 1.0)

        [Space(30)][Toggle(USE_NORMALMAP)] _UseNormalMap("Use Normal Map?", Int) = 0
        _NormalMap("Normal Map", 2D) = "white" {}
        _NormalMapIntensity("Normal Map Intensity", float) = 1.0
    }

        SubShader
        {
            Tags
            {
                "RenderType" = "Opaque"
                "RenderPipeline" = "UniversalRenderPipeline"
                "IgnoreProjector" = "True"
                "Queue" = "Geometry-2"
            }
            LOD 100

            Pass
            {
            Name "Forward Lighting"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Stencil
            {
                Ref 2
                Comp Equal
            }

            Blend One Zero
            ZWrite On
            Cull Back

            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICSPECGLOSSMAP
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _OCCLUSIONMAP
            #pragma shader_feature _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature _SPECULAR_SETUP
            #pragma shader_feature _RECEIVE_SHADOWS_OFF
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            #pragma multi_compile __ USE_NORMALMAP

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TECOORD3;
                float2 uv0 : TEXCOORD4;
                float4 lightmapUV : TEXCOORD5;
                float4 fog : TEXCOORD6;
            };


            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                /* Vertex positions in different coordinate spaces */
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;

                /* Normal space vectors */
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;

                /* UVs */
                output.uv0 = input.uv0;

                /* Lightmap / Spherical Harmonics */
                OUTPUT_LIGHTMAP_UV(input.uv1, unity_LightmapST, output.lightmapUV);
                OUTPUT_SH(normalInput.normalWS, output.lightmapUV);

                /* Fog */
                output.fog = ComputeFogFactor(vertexInput.positionCS.z);

                return output;
            }

            float3 _Albedo;
            float _Metallic;
            float3 _Specular;
            float _Smoothness;
            float _Occlusion;
            float3 _Emission;
            float _Alpha;
            float _RimPower;
            float3 _RimColor;

            sampler2D _AlbedoMap;
            float4 _AlbedoMap_ST; // Scale and tiling

            sampler2D _NormalMap;
            float4 _NormalMap_ST;

            float _NormalMapIntensity;

            float4 frag(Varyings input) : SV_Target
            {
                /* Texture Samples */
                float4 albedoSample = tex2D(_AlbedoMap, input.uv0 * _AlbedoMap_ST.xy + _AlbedoMap_ST.zw);
                float4 normalSample = tex2D(_NormalMap, input.uv0 * _NormalMap_ST.xy + _NormalMap_ST.zw);

                /* Normal Mapping */
                #ifdef USE_NORMALMAP
                    float3 normal = float3(0.0, 0.0, 0.0);
                    normal.xy = normalSample.wy * 2.0 - 1.0;
                    normal.z = sqrt(1.0 - dot(normal, normal));
                    normal = normalize(normal);

                    float3x3 NormalToWorldTranspose = float3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                    normal = normalize(mul(normal, NormalToWorldTranspose));
                    normal = lerp(input.normalWS, normal, _NormalMapIntensity);
                #else
                    float3 normal = input.normalWS;
                #endif

                    /* Build input data for unity pbr lighting */
                    InputData inputData;
                    inputData.positionWS = input.positionWS;
                    inputData.positionCS = input.positionCS;
                    inputData.normalWS = normalize(normal);
                    inputData.viewDirectionWS = normalize(_WorldSpaceCameraPos.xyz - input.positionWS);
                    inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                    inputData.fogCoord = input.fog.x;

                    /* Sample GI */
                    float3 SH = SampleSH(inputData.normalWS);
                    float3 bakedGI = SAMPLE_GI(input.lightmapUV, SH, inputData.normalWS);
                    inputData.bakedGI = bakedGI;

                    /* Default unity pbr lighting */
                    float4 unityColor = UniversalFragmentPBR(inputData, _Albedo * albedoSample, _Metallic, _Specular, _Smoothness, _Occlusion, _Emission, _Alpha);

                    /* Add fog */
                    unityColor.rgb = MixFog(unityColor.rgb, input.fog.x);

                    /* Rim lighting */
                    float rim = 1.0 - max(dot(inputData.viewDirectionWS, inputData.normalWS), 0.0);
                    unityColor.rgb += pow(rim, _RimPower) * _RimColor;

                    return unityColor;
                }
                ENDHLSL
            }

            /* Shadow rendering pass */
            UsePass "Universal Render Pipeline/Lit/ShadowCaster"

            /* Depth prepass */
            UsePass "Universal Render Pipeline/Lit/DepthOnly"

            /* Meta pass for global illumination */
            UsePass "Universal Render Pipeline/Lit/Meta"
        }
}
