Shader "Custom/RealTimeFire"
{
    Properties
    {
        _MainColor("内层火焰", Color) = (1,0.3,0,1)
        _SecondaryColor("外层火焰", Color) = (1,0.8,0.2,1)
        _EdgeColor("外焰高光", Color) = (1,1,0.6,1)
        _Speed("燃烧速度", Range(0.1,3)) = 1.2
        _Distort("扭曲扰动", Range(0,0.3)) = 0.15
        _Brightness("整体亮度", Range(0.2,3)) = 1.5
    }

        SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _MainColor;
            fixed4 _SecondaryColor;
            fixed4 _EdgeColor;
            float _Speed;
            float _Distort;
            float _Brightness;

            // 简单噪声
            float Noise(float2 p)
            {
                float2 ip = floor(p);
                float2 u = frac(p);
                u = u * u * (3.0 - 2.0 * u);
                float res = lerp(
                    lerp(dot(ip, float2(127.1,311.7)), dot(ip + float2(1,0), float2(127.1,311.7)), u.x),
                    lerp(dot(ip + float2(0,1), float2(127.1,311.7)), dot(ip + float2(1,1), float2(127.1,311.7)), u.x),
                    u.y);
                return sin(res) * 0.5 + 0.5;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float time = _Time.y * _Speed;
                float2 uv = i.uv;

                // 上下流动 + 扭曲扰动
                uv.y += time * 0.4;
                float n = Noise(uv * 3.0 + time);
                uv.x += (n - 0.5) * _Distort;

                // 火焰形状渐变
                float fireMask = 1 - uv.y;
                fireMask = pow(fireMask, 1.8);
                fireMask *= n;

                // 分层火焰颜色
                float core = saturate(fireMask * 2.2);
                float mid = saturate(fireMask * 1.4 - 0.25);
                float edge = saturate(fireMask - 0.5);

                fixed3 col = _MainColor.rgb;
                col = lerp(col, _SecondaryColor.rgb, mid);
                col = lerp(col, _EdgeColor.rgb, edge);

                // 闪烁亮度
                float flicker = 0.85 + sin(time * 8.0 + n * 6.0) * 0.15;
                col *= fireMask * _Brightness * flicker;

                fixed alpha = saturate(fireMask * 0.95);
                return fixed4(col, alpha);
            }
            ENDCG
        }
    }
        FallBack "Unlit/Transparent"
}