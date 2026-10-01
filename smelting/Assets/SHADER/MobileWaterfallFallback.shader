// 移动端水锤瀑布替代 shader：向下流动的条纹 + 顶底渐隐
// 用于出水口落水（竖直 Quad），uv.y 沿高度方向
Shader "Custom/MobileWaterfallFallback"
{
    Properties
    {
        _Color("水色", Color) = (0.45, 0.8, 0.82, 0.55)
        _FoamColor("水花色", Color) = (0.95, 1.0, 1.0, 1.0)
        _ScrollSpeed("下落速度", Float) = 2.0
        _StreakScale("条纹密度", Float) = 14.0
        _StreakSharpness("条纹锐度", Float) = 2.0
        _TopFade("顶部渐隐", Range(0, 0.5)) = 0.12
        _BottomFade("底部渐隐", Range(0, 0.5)) = 0.3
        _MaxAlpha("最大不透明度", Range(0,1)) = 0.75
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        LOD 100

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
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _FoamColor;
            float _ScrollSpeed;
            float _StreakScale;
            float _StreakSharpness;
            float _TopFade;
            float _BottomFade;
            float _MaxAlpha;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 每列相位错开，形成多条不等速的水痕
                float phase = sin(i.uv.x * 18.0) * 1.7 + sin(i.uv.x * 43.0) * 0.6;
                float streak = sin(i.uv.y * _StreakScale + phase + _Time.y * _ScrollSpeed);
                float flow = pow(saturate(streak * 0.5 + 0.55), _StreakSharpness);

                // 宽度方向边缘略透，中间实一些
                float edge = saturate(1.0 - abs(i.uv.x - 0.5) * 2.0);

                // 顶底渐隐（uv.y=1 顶部，0 底部）
                float fadeTop = saturate((1.0 - i.uv.y) / _TopFade);
                float fadeBottom = saturate(i.uv.y / _BottomFade);

                float alpha = _Color.a * (0.5 + flow * 0.5) * edge * fadeTop * fadeBottom;
                fixed3 col = lerp(_Color.rgb, _FoamColor.rgb, flow * 0.7);
                return fixed4(col, min(saturate(alpha), _MaxAlpha));
            }
            ENDCG
        }
    }
    FallBack "Unlit/Transparent"
}
