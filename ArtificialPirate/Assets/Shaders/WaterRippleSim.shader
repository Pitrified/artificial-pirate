// One step of the water's ripple height field, blitted from the previous state into the next
// by Game.Gameplay.Water.WaterRipples (two render textures, ping-pong). R holds the height now,
// G the height one step ago; each step is the 2D wave equation, damped, plus this step's impulses.
// B holds foam (a wake): faded and spread a little each step, plus this step's foam stamps.
// When the square has moved, _Shift (uv) reads the previous state where the same world point was.
// See docs/systems/water.md.
Shader "artificial-pirate/WaterRippleSim"
{
    Properties
    {
        _MainTex ("Previous state", 2D) = "black" {}
        _Damping ("Damping per step", Float) = 0.97
        _FoamFade ("Foam kept per step", Float) = 0.98
        _FoamSpread ("Foam spread to neighbours per step", Float) = 0.15
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "Step"

            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert_img
            #pragma fragment Frag
            #pragma target 3.0

            #define MAX_IMPULSES 16
            #define MAX_FOAM 16

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Damping;
            float _FoamFade;
            float _FoamSpread;
            float2 _Shift;
            float4 _Impulses[MAX_IMPULSES]; // uv x, uv y, radius in uv, strength
            float _ImpulseCount;
            float4 _FoamStamps[MAX_FOAM];   // uv x, uv y, radius in uv, amount
            float _FoamCount;

            // The previous state at a uv; nothing from outside the square.
            float3 Previous(float2 uv)
            {
                float2 inside = step(0.0, uv) * step(uv, 1.0);
                return tex2D(_MainTex, uv).rgb * (inside.x * inside.y);
            }

            // A soft round stamp: 1 at the centre, 0 at the radius.
            float Stamp(float2 uv, float4 stamp)
            {
                float d = length(uv - stamp.xy) / max(stamp.z, 1e-4);
                return stamp.w * saturate(1.0 - d * d);
            }

            float4 Frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                float2 from = uv + _Shift;
                float2 texel = _MainTex_TexelSize.xy;
                float3 here = Previous(from);
                float3 neighbours = Previous(from + float2(texel.x, 0)) + Previous(from - float2(texel.x, 0))
                                  + Previous(from + float2(0, texel.y)) + Previous(from - float2(0, texel.y));
                float next = (neighbours.r * 0.5 - here.g) * _Damping;
                float foam = lerp(here.b, neighbours.b * 0.25, _FoamSpread) * _FoamFade;

                for (int k = 0; k < MAX_IMPULSES; k++)
                {
                    if (k >= (int)_ImpulseCount)
                    {
                        break;
                    }
                    next += Stamp(uv, _Impulses[k]);
                }
                for (int f = 0; f < MAX_FOAM; f++)
                {
                    if (f >= (int)_FoamCount)
                    {
                        break;
                    }
                    foam += Stamp(uv, _FoamStamps[f]);
                }

                // Absorb at the border, so ripples leave the area instead of bouncing back.
                float edge = min(min(uv.x, uv.y), min(1.0 - uv.x, 1.0 - uv.y));
                float absorb = saturate(edge * 25.0);
                return float4(next * absorb, here.r, saturate(foam), 0);
            }
            ENDCG
        }
    }
}
