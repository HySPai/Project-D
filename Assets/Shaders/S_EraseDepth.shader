Shader "Demo/Erase Depth"
{
    // Erase any of the depth values in the current depth buffer

    Properties
    {
        _DebugColor("Debug Color", Color) = (1,0,0)
        _Alpha("Debug Alpha", float) = 0.0
    }

        SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalRenderPipeline"
            "IgnoreProjector" = "True"
            "Queue" = "Geometry-12"
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
                Ref 255
                Comp NotEqual
            }

            Blend Zero One
            ZTest Always
            ZWrite On
            Cull Back

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

            float3 _DebugColor;
            float _Alpha;

            Varyings LitPassVertex(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;

                return output;
            }

            half4 LitPassFragment(Varyings input, out float outDepth : SV_Depth) : SV_Target
            {
                outDepth = 0.0f;
                return half4(_DebugColor, _Alpha);
            }

            ENDHLSL
        }
    }
}