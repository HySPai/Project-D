Shader "Demo/Interior Void"
{
    // Render the interior glow if it passes the stencil test 

    Properties
    {
        _MainColor("Void Color", Color) = (1,1,1)
        _Intensity("Emission Intensity", float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalRenderPipeline"
            "IgnoreProjector" = "True"
            "Queue" = "Geometry-3"
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
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            float3 _MainColor;
            float _Intensity;

            Varyings LitPassVertex(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;

                return output;
            }

            half4 LitPassFragment(Varyings input) : SV_Target
            {
                return half4(_MainColor, 1.0) * _Intensity;
            }

            ENDHLSL
        }
    }
}