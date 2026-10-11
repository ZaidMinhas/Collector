
Shader "UI/CircleTransition"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,1)
        _Radius ("Radius", Float) = 1
        _Center ("Center", Vector) = (0.5,0.5,0,0)
        _Aspect ("Aspect Ratio", Float) = 1.7778
        _Feather ("Edge Softness", Float) = 0.01
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;
            float _Radius;
            float4 _Center;
            float _Aspect;
            float _Feather;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 delta = i.uv - _Center.xy;
                delta.x *= _Aspect;

                float distanceFromCenter = length(delta);

                float alpha = smoothstep(
                    _Radius,
                    _Radius + max(_Feather, 0.0001),
                    distanceFromCenter
                );

                return fixed4(_Color.rgb, _Color.a * alpha);
            }
            ENDHLSL
        }
    }
}
