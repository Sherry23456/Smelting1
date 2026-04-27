Shader "Custom/BurningCharcoal"
{
    Properties
    {
        // 基础纹理
        _MainTex("木炭基础纹理", 2D) = "white" {}
    // 燃烧噪点（控制燃烧不规则边缘）
    _BurnNoise("燃烧噪点纹理", 2D) = "white" {}
    // 燃烧控制（0=未燃烧，1=完全烧成灰）
    _BurnAmount("燃烧进度", Range(0, 1)) = 0.3
        // 燃烧边缘宽度
        _BurnEdgeWidth("燃烧边缘宽度", Range(0.01, 0.2)) = 0.1
        // 火焰亮度
        _FireIntensity("火焰发光强度", Range(0, 5)) = 2.0
        // 木炭颜色
        _CharcoalColor("木炭颜色", Color) = (0.1, 0.1, 0.1, 1)
        // 灰烬颜色
        _AshColor("灰烬颜色", Color) = (0.3, 0.3, 0.3, 1)
        // 火焰颜色
        _FlameColor("火焰颜色", Color) = (1, 0.3, 0, 1)
        // 动态流动速度
        _FlowSpeed("燃烧流动速度", Range(0, 2)) = 0.8
    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 100
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
                float2 noiseUV : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _BurnNoise;
            float4 _BurnNoise_ST;
            float _BurnAmount;
            float _BurnEdgeWidth;
            float _FireIntensity;
            fixed4 _CharcoalColor;
            fixed4 _AshColor;
            fixed4 _FlameColor;
            float _FlowSpeed;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // 基础UV
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // 噪点UV + 时间流动（动态燃烧效果）
                o.noiseUV = TRANSFORM_TEX(v.uv, _BurnNoise) + float2(0, _Time.y * _FlowSpeed);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 1. 获取基础木炭纹理
                fixed4 col = tex2D(_MainTex, i.uv) * _CharcoalColor;

            // 2. 获取燃烧噪点（不规则边缘核心）
            float noise = tex2D(_BurnNoise, i.noiseUV).r;

            // 3. 计算燃烧区域
            float burn = smoothstep(_BurnAmount - _BurnEdgeWidth, _BurnAmount + _BurnEdgeWidth, noise);
            float ash = step(_BurnAmount + _BurnEdgeWidth * 2, noise);

            // 4. 混合木炭/灰烬
            fixed3 finalColor = lerp(col.rgb, _AshColor.rgb, ash);

            // 5. 火焰发光效果（燃烧边缘高亮）
            float fire = burn * (1 - ash);
            finalColor = lerp(finalColor, _FlameColor.rgb * _FireIntensity, fire);

            return fixed4(finalColor, 1);
        }
        ENDCG
    }
    }
        FallBack "Unlit/Texture"
}