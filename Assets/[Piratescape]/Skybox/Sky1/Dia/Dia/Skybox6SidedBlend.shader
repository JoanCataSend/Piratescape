Shader "Custom/Skybox6SidedBlend"
{
    Properties
    {
        _Blend ("Blend", Range(0, 1)) = 0
        _Exposure ("Exposure", Range(0, 8)) = 1
        _Tint ("Tint", Color) = (1,1,1,1)

        _FrontTexA ("A Front", 2D) = "white" {}
        _BackTexA ("A Back", 2D) = "white" {}
        _LeftTexA ("A Left", 2D) = "white" {}
        _RightTexA ("A Right", 2D) = "white" {}
        _UpTexA ("A Up", 2D) = "white" {}
        _DownTexA ("A Down", 2D) = "white" {}

        _FrontTexB ("B Front", 2D) = "white" {}
        _BackTexB ("B Back", 2D) = "white" {}
        _LeftTexB ("B Left", 2D) = "white" {}
        _RightTexB ("B Right", 2D) = "white" {}
        _UpTexB ("B Up", 2D) = "white" {}
        _DownTexB ("B Down", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Background"
            "RenderType" = "Background"
            "PreviewType" = "Skybox"
        }

        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _FrontTexA;
            sampler2D _BackTexA;
            sampler2D _LeftTexA;
            sampler2D _RightTexA;
            sampler2D _UpTexA;
            sampler2D _DownTexA;

            sampler2D _FrontTexB;
            sampler2D _BackTexB;
            sampler2D _LeftTexB;
            sampler2D _RightTexB;
            sampler2D _UpTexB;
            sampler2D _DownTexB;

            float _Blend;
            float _Exposure;
            float4 _Tint;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = v.vertex.xyz;
                return o;
            }

            float2 CalcularUV(float3 dir, out int face)
            {
                float3 absDir = abs(dir);
                float2 uv;

                if (absDir.z >= absDir.x && absDir.z >= absDir.y)
                {
                    if (dir.z > 0)
                    {
                        face = 0;
                        uv = float2(dir.x, dir.y) / absDir.z;
                    }
                    else
                    {
                        face = 1;
                        uv = float2(-dir.x, dir.y) / absDir.z;
                    }
                }
                else if (absDir.x >= absDir.y)
                {
                    if (dir.x > 0)
                    {
                        face = 2;
                        uv = float2(-dir.z, dir.y) / absDir.x;
                    }
                    else
                    {
                        face = 3;
                        uv = float2(dir.z, dir.y) / absDir.x;
                    }
                }
                else
                {
                    if (dir.y > 0)
                    {
                        face = 4;
                        uv = float2(dir.x, -dir.z) / absDir.y;
                    }
                    else
                    {
                        face = 5;
                        uv = float2(dir.x, dir.z) / absDir.y;
                    }
                }

                uv = uv * 0.5 + 0.5;
                return uv;
            }

            fixed4 LeerSkyboxA(int face, float2 uv)
            {
                if (face == 0) return tex2D(_FrontTexA, uv);
                if (face == 1) return tex2D(_BackTexA, uv);
                if (face == 2) return tex2D(_LeftTexA, uv);
                if (face == 3) return tex2D(_RightTexA, uv);
                if (face == 4) return tex2D(_UpTexA, uv);
                return tex2D(_DownTexA, uv);
            }

            fixed4 LeerSkyboxB(int face, float2 uv)
            {
                if (face == 0) return tex2D(_FrontTexB, uv);
                if (face == 1) return tex2D(_BackTexB, uv);
                if (face == 2) return tex2D(_LeftTexB, uv);
                if (face == 3) return tex2D(_RightTexB, uv);
                if (face == 4) return tex2D(_UpTexB, uv);
                return tex2D(_DownTexB, uv);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);

                int face;
                float2 uv = CalcularUV(dir, face);

                fixed4 colorA = LeerSkyboxA(face, uv);
                fixed4 colorB = LeerSkyboxB(face, uv);

                fixed4 colorFinal = lerp(colorA, colorB, _Blend);

                colorFinal.rgb *= _Tint.rgb;
                colorFinal.rgb *= _Exposure;

                return colorFinal;
            }
            ENDCG
        }
    }

    FallBack Off
}