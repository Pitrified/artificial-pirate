// A flag that waves: waves run from the hoist (uv.x 0, held still) to the fly, growing along it,
// and the fly droops a little. The vertices move along the flag's normal in the vertex stage, in
// every pass, so its shadow waves with it. Two-sided, lit by the main light with its shadows.
// The mesh comes from Tools/asset_builders/builders/ship/props.py.
Shader "Game/Flag"
{
    Properties
    {
        _BaseMap ("Flag", 2D) = "white" {}
        _BaseColor ("Tint", Color) = (1, 1, 1, 1)
        _WaveAmplitude ("Wave amplitude at the fly (m)", Float) = 0.35
        _WaveCount ("Waves along the flag", Float) = 1.6
        _WaveSpeed ("Waves a second", Float) = 1.4
        _Droop ("Droop at the fly (m)", Float) = 0.25
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }
        Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            float _WaveAmplitude;
            float _WaveCount;
            float _WaveSpeed;
            float _Droop;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS   : NORMAL;
            float2 uv         : TEXCOORD0;
        };

        // The wave's phase at this point, and the point moved by it.
        float3 Wave(float3 positionOS, float3 normalOS, float2 uv, out float phase)
        {
            phase = TWO_PI * (uv.x * _WaveCount - _Time.y * _WaveSpeed);
            float along = uv.x;
            float3 offset = normalOS * (sin(phase) * _WaveAmplitude * along);
            offset.y -= _Droop * along * along;
            return positionOS + offset;
        }
        ENDHLSL

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float2 uv         : TEXCOORD2;
                float  ripple     : TEXCOORD3;
                float  fogFactor  : TEXCOORD4;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float phase;
                float3 positionOS = Wave(input.positionOS.xyz, input.normalOS, input.uv, phase);
                output.positionWS = TransformObjectToWorld(positionOS);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                // The slope of the wave darkens the cloth turned away: a cheap fold shading.
                output.ripple = cos(phase) * saturate(input.uv.x * 2.0);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input, bool frontFace : SV_IsFrontFace) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                float3 normal = normalize(frontFace ? input.normalWS : -input.normalWS);
                Light sun = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half diffuse = saturate(dot(normal, sun.direction)) * sun.shadowAttenuation;
                half3 ambient = SampleSH(normal);
                half folds = 0.85 + 0.15 * input.ripple;
                half3 colour = albedo.rgb * (ambient + sun.color * diffuse) * folds;
                return half4(MixFog(colour, input.fogFactor), 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            float4 Vert(Attributes input) : SV_POSITION
            {
                float phase;
                float3 positionWS = TransformObjectToWorld(Wave(input.positionOS.xyz, input.normalOS, input.uv, phase));
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
            #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #endif
                return positionCS;
            }

            half4 Frag() : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            float4 Vert(Attributes input) : SV_POSITION
            {
                float phase;
                return TransformObjectToHClip(Wave(input.positionOS.xyz, input.normalOS, input.uv, phase));
            }

            half4 Frag() : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
