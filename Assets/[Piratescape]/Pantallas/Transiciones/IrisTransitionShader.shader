Shader "UI/IrisTransition"
{
    Properties
    {
        _Color ("Color", Color) = (0, 0, 0, 1)
        _Radius ("Radius", Range(-0.5, 2)) = 1.5
        _Softness ("Softness", Range(0.001, 0.5)) = 0.08
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        _Aspect ("Aspect", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Radius;
            float _Softness;
            float4 _Center;
            float _Aspect;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 center = _Center.xy;

                float2 correctedUV = uv - center;
                correctedUV.x *= _Aspect;

                float dist = length(correctedUV);

                float alpha = smoothstep(_Radius, _Radius + _Softness, dist);

                fixed4 col = _Color * i.color;
                col.a *= alpha;

                return col;
            }
            ENDCG
        }
    }
}