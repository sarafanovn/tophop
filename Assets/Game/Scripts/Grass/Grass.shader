Shader "Custom/ToonGrass"
{
    Properties
    {
        _GrassColor      ("Grass Color",       Color)       = (0.15, 0.65, 0.15, 1)
        _DirtColor       ("Dirt Color",        Color)       = (0.45, 0.25, 0.10, 1)
        _GrassBlendStart ("Grass Blend Start", Range(-1,1)) = 0.3
        _GrassBlendEnd   ("Grass Blend End",   Range(-1,1)) = 0.7
        _ToonSteps       ("Toon Steps",        Float)       = 3.0
        _AmbientMin      ("Ambient Min",       Range(0,1))  = 0.25
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
            "RenderPipeline"="UniversalPipeline"
        }
        Cull Back

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _GrassColor;
                float4 _DirtColor;
                float  _GrassBlendStart;
                float  _GrassBlendEnd;
                float  _ToonSteps;
                float  _AmbientMin;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS   = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 normal = normalize(IN.normalWS);

                // grass on top, dirt on bottom — blend by how much face looks upward
                float  upness     = dot(normal, float3(0, 1, 0));
                float  grassBlend = smoothstep(_GrassBlendStart, _GrassBlendEnd, upness);
                float4 color      = lerp(_DirtColor, _GrassColor, grassBlend);

                // toon lighting (half-lambert → quantized steps)
                Light  light = GetMainLight();
                float  NdotL = dot(normal, light.direction) * 0.5 + 0.5;
                float  toon  = floor(NdotL * _ToonSteps) / _ToonSteps;
                toon         = max(toon, _AmbientMin);

                color.rgb *= toon * light.color;

                return color;
            }
            ENDHLSL
        }
    }
}