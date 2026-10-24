Shader "LinaStyle/StylizedSoftLit_URP"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color / Tint", Color) = (0.72,0.5,0.9,1)
        [Normal] _BumpMap("Normal Map (Optional)", 2D) = "bump" {}
        _BumpScale("Normal Strength", Range(0,2)) = 1
        _ShadeColor("Shadow Tint", Color) = (0.35,0.32,0.65,1)
        _ShadowStrength("Shadow Color Strength", Range(0,1)) = 0.5
        _ShadowSoftness("Shadow Transition Softness", Range(0.03,1)) = 0.6
        _LightWrap("Soft Light Wrap", Range(0,1)) = 0.45
        _Ambient("Fill Light Strength", Range(0,1.5)) = 0.45
        _SpecColor("Soft Highlight Color", Color) = (1,1,1,1)
        _SpecStrength("Highlight Strength", Range(0,2)) = 0.3
        _SpecPower("Highlight Sharpness", Range(4,128)) = 32
        _RimColor("Edge Light Color", Color) = (0.78,0.66,1,1)
        _RimStrength("Edge Light Strength", Range(0,2)) = 0.35
        _RimPower("Edge Light Tightness", Range(0.5,8)) = 3.2
        [HDR] _EmissionColor("Glow Color (HDR)", Color) = (0,0,0,1)
        _EmissionStrength("Glow Strength", Range(0,8)) = 0
        [HideInInspector] _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
        [HideInInspector] _Cull("Cull", Float) = 2
        [HideInInspector] _Surface("Surface", Float) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 250
        Cull Back
        ZWrite On
        Pass
        {
            Name "StylizedForward"
            Tags { "LightMode"="UniversalForwardOnly" }
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _ShadeColor;
                half4 _SpecColor;
                half4 _RimColor;
                half4 _EmissionColor;
                half _BumpScale;
                half _ShadowStrength;
                half _ShadowSoftness;
                half _LightWrap;
                half _Ambient;
                half _SpecStrength;
                half _SpecPower;
                half _RimStrength;
                half _RimPower;
                half _EmissionStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half3 tangentWS : TEXCOORD2;
                half3 bitangentWS : TEXCOORD3;
                float2 uv : TEXCOORD4;
                half fogFactor : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs nor = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                OUT.positionCS = pos.positionCS;
                OUT.positionWS = pos.positionWS;
                OUT.normalWS = nor.normalWS;
                OUT.tangentWS = nor.tangentWS;
                OUT.bitangentWS = nor.bitangentWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.fogFactor = ComputeFogFactor(pos.positionCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                half4 textureColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half3 albedo = textureColor.rgb * _BaseColor.rgb;
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv), _BumpScale);
                half3 N = normalize(IN.tangentWS * normalTS.x + IN.bitangentWS * normalTS.y + IN.normalWS * normalTS.z);
                half3 V = GetWorldSpaceNormalizeViewDir(IN.positionWS);

                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                    shadowCoord = ComputeScreenPos(IN.positionCS);
                #endif
                Light sun = GetMainLight(shadowCoord);
                half ndl = saturate(dot(N, sun.direction));
                half wrapped = saturate((ndl + _LightWrap) / (1 + _LightWrap));
                half edgeWidth = max(_ShadowSoftness * 0.48h, 0.015h);
                half stylizedLight = smoothstep(0.48h - edgeWidth, 0.48h + edgeWidth, wrapped);
                stylizedLight *= sun.shadowAttenuation;

                half3 shadowTint = lerp(half3(1,1,1), _ShadeColor.rgb, _ShadowStrength);
                half3 shapedLight = lerp(shadowTint * 0.72h, sun.color, stylizedLight);
                half3 ambientLight = max(SampleSH(N), half3(0.05h,0.05h,0.05h));
                half3 result = albedo * (shapedLight * 0.8h + ambientLight * (0.18h + _Ambient * 0.45h));

                #if defined(_ADDITIONAL_LIGHTS)
                    uint lightCount = GetAdditionalLightsCount();
                    for (uint i = 0u; i < lightCount; ++i)
                    {
                        Light extra = GetAdditionalLight(i, IN.positionWS);
                        half lambert = saturate(dot(N, extra.direction));
                        result += albedo * extra.color * lambert * extra.distanceAttenuation * extra.shadowAttenuation * 0.45h;
                    }
                #endif

                half3 H = normalize(V + sun.direction);
                half glossy = pow(saturate(dot(N, H)), _SpecPower) * ndl;
                result += _SpecColor.rgb * glossy * _SpecStrength * sun.shadowAttenuation;

                half rim = pow(1.0h - saturate(dot(N,V)), _RimPower);
                result += _RimColor.rgb * rim * _RimStrength;
                result += _EmissionColor.rgb * _EmissionStrength;
                result = MixFog(result, IN.fogFactor);
                return half4(result, _BaseColor.a * textureColor.a);
            }
            ENDHLSL
        }

        // Native URP passes support shadow casting and depth rendering.
        UsePass "Universal Render Pipeline/Lit/SHADOWCASTER"
        UsePass "Universal Render Pipeline/Lit/DEPTHONLY"
    }
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}
