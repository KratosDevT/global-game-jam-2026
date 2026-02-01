Shader "Custom/URP/RevealMaskQuad_Debug"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tint ("Tint", Color) = (1,1,1,1)
        
        _RevealCenterWS ("Reveal Center (World XY)", Vector) = (0,0,0,0)
        _RevealRadius ("Reveal Radius", Float) = 2
        _EdgeSoftness ("Edge Softness", Float) = 0.25
        _EdgeColor ("Edge Color", Color) = (0,1,1,1)
        _RevealEnabled ("Reveal Enabled", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            // ───── Material properties
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _Tint;
            float  _UVRotation;

            // ───── Material properties (debug)
            float4 _RevealCenterWS;
            float  _RevealRadius;
            float  _EdgeSoftness;
            float4 _EdgeColor;
            
            // ───── Global overrides (runtime)
            float3 _GlobalRevealPos;
            float  _GlobalRevealRadius;
            float  _GlobalEdgeSoftness;
            float4 _GlobalEdgeColor;
            float _GlobalRevealEnabled;
            
            Varyings vert (Attributes v)
            {
                Varyings o;
                float3 world = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(world);
                o.worldPos = world;
                o.uv = v.uv;
                return o;
            }

            float2 RotateUV(float2 uv, float angleDeg)
            {
                float rad = radians(angleDeg);
                float s = sin(rad);
                float c = cos(rad);

                uv -= 0.5;
                uv = float2(
                    uv.x * c - uv.y * s,
                    uv.x * s + uv.y * c
                );
                uv += 0.5;

                return uv;
            }
            
            half4 frag (Varyings i) : SV_Target
            {
                // Rotate the uv
                float2 uv = RotateUV(i.uv, _UVRotation);
                
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _Tint;

                // ───── SCEGLIAMO LA SORGENTE DATI
                float2 center =
                    (_GlobalRevealRadius > 0.0001)
                    ? _GlobalRevealPos.xy
                    : _RevealCenterWS.xy;

                float radius =
                    (_GlobalRevealRadius > 0.0001)
                    ? _GlobalRevealRadius
                    : _RevealRadius;

                float softness =
                    (_GlobalRevealRadius > 0.0001)
                    ? _GlobalEdgeSoftness
                    : _EdgeSoftness;

                float4 edgeColor =
                    (_GlobalRevealRadius > 0.0001)
                    ? _GlobalEdgeColor
                    : _EdgeColor;

                float dist = distance(i.worldPos.xy, center);

                // Alpha: dentro = invisibile, fuori = visibile
                float alpha = smoothstep(
                    radius - softness,
                    radius,
                    dist
                );

                alpha = lerp(1.0, alpha, _GlobalRevealEnabled);
                
                // Bordo
                float edge = 1.0 - alpha;
                col.rgb = lerp(col.rgb, edgeColor.rgb, edge * edgeColor.a);

                col.a *= alpha;
                return col;
            }
            ENDHLSL
        }
    }
}