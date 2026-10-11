Shader "Collector/PlanetGlow"
{
    Properties
    {
        _InnerColor ("Inner Color", Color) = (1, 0.35, 0.65, 1)
        _OuterColor ("Outer Color", Color) = (0.6, 0.1, 0.4, 1)
        _PlanetEdge ("Planet Edge", Range(0, 1)) = 0.6
        _Falloff ("Falloff", Float) = 1.5
        _Bands ("Bands", Float) = 5
        _Pixels ("Pixels", Float) = 64
        _Opacity ("Opacity", Range(0, 1)) = 0.5
        _Intensity ("Intensity", Float) = 1
        _RippleCount ("Ripple Count", Float) = 3
        _RippleSpeed ("Ripple Speed", Float) = 0.3
        _RippleStrength ("Ripple Strength", Range(0, 1)) = 0.35
        _WobbleCount ("Wobble Count", Float) = 5
        _WobbleSpeed ("Wobble Speed", Float) = 0.15
        _WobbleAmount ("Wobble Amount", Range(0, 0.5)) = 0.08
        _RayCount ("Ray Count", Float) = 6
        _RaySpeed ("Ray Speed", Float) = 0.03
        _RayStrength ("Ray Strength", Range(0, 1)) = 0.3
        _Rainbow ("Rainbow", Range(0, 1)) = 0
        _RainbowSpeed ("Rainbow Speed", Float) = 0.05
        _RainbowTwist ("Rainbow Twist", Float) = 0.5
        _RainbowSaturation ("Rainbow Saturation", Range(0, 1)) = 0.8
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _InnerColor;
            float4 _OuterColor;
            float _PlanetEdge;
            float _Falloff;
            float _Bands;
            float _Pixels;
            float _Opacity;
            float _Intensity;
            float _RippleCount;
            float _RippleSpeed;
            float _RippleStrength;
            float _WobbleCount;
            float _WobbleSpeed;
            float _WobbleAmount;
            float _RayCount;
            float _RaySpeed;
            float _RayStrength;
            float _Rainbow;
            float _RainbowSpeed;
            float _RainbowTwist;
            float _RainbowSaturation;

            float3 HueToRgb(float hue, float saturation)
            {
                float3 rgb = saturate(abs(frac(hue + float3(0.0, 2.0 / 3.0, 1.0 / 3.0)) * 6.0 - 3.0) - 1.0);
                return lerp(float3(1.0, 1.0, 1.0), rgb, saturation);
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                const float TAU = 6.2831853;
                float time = _Time.y;

                float2 uv = (floor(i.uv * _Pixels) + 0.5) / _Pixels;
                float2 p = uv * 2.0 - 1.0;
                float angle = atan2(p.y, p.x);

                float wobbleCount = max(1.0, round(_WobbleCount));
                float wobble = sin(angle * wobbleCount + time * _WobbleSpeed * TAU) * 0.6
                             + sin(angle * (wobbleCount * 2.0 + 1.0) - time * _WobbleSpeed * 1.7 * TAU) * 0.4;
                float d = length(p) * (1.0 + _WobbleAmount * (wobble * 0.5 + 0.5));

                float t = saturate((d - _PlanetEdge) / max(1.0 - _PlanetEdge, 0.0001));
                float a = pow(saturate(1.0 - t), _Falloff);

                float ripple = sin((t * _RippleCount - time * _RippleSpeed) * TAU) * 0.5 + 0.5;
                a *= lerp(1.0, ripple, _RippleStrength);

                float rays = sin(angle * round(_RayCount) + time * _RaySpeed * TAU) * 0.5 + 0.5;
                a *= lerp(1.0, rays, _RayStrength);

                a = ceil(a * _Bands) / _Bands;

                float3 color = lerp(_InnerColor.rgb, _OuterColor.rgb, t);
                float hue = angle / TAU + t * _RainbowTwist + time * _RainbowSpeed;
                color = lerp(color, HueToRgb(hue, _RainbowSaturation), _Rainbow) * _Intensity;
                return float4(color, a * _Opacity);
            }
            ENDCG
        }
    }
}
