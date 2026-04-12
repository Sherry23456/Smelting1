Shader "Custom/HighlightOutline"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}           // 原物体纹理（可选）
        _OutlineColor("Outline Color", Color) = (0, 1, 1, 1)   // 轮廓颜色（支持呼吸调色）
        _OutlineWidth("Outline Width", Range(0.0, 0.1)) = 0.02 // 厚度
        _OutlineIntensity("Outline Intensity", Range(0, 5)) = 2.0 // 亮度强度

        // 呼吸效果参数（高自由度）
        _BreathSpeed("Breath Speed", Range(0.1, 10)) = 2.0     // 呼吸速度
        _BreathAmplitude("Breath Amplitude", Range(0, 1)) = 0.3 // 呼吸幅度（0=无呼吸）
        _BreathMinScale("Breath Min Scale", Range(0.5, 1)) = 0.8 // 最小缩放比例
        _BreathColorShift("Breath Color Shift", Color) = (0.2, 0.2, 0.2, 0) // 呼吸时颜色偏移

        _ZTest("ZTest", Int) = 0   // 深度测试（通常用 Always 让轮廓显示在最前面）
    }

        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }

            // ==================== 第一 Pass：渲染正常物体 ====================
            Pass
            {
                Name "BASE"
                ZWrite On
                ZTest[_ZTest]
                Cull Back

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

                sampler2D _MainTex;
                float4 _MainTex_ST;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    return tex2D(_MainTex, i.uv);
                }
                ENDCG
            }

            // ==================== 第二 Pass：渲染轮廓（Inverted Hull 方法） ====================
            Pass
            {
                Name "OUTLINE"
                Tags { "Queue" = "Transparent+1" }
                Cull Front
                ZWrite Off
                ZTest Always
                Blend SrcAlpha OneMinusSrcAlpha   // 支持透明呼吸

                CGPROGRAM
                #pragma vertex vertOutline
                #pragma fragment fragOutline
                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float4 color : COLOR;
                };

                fixed4 _OutlineColor;
                float _OutlineWidth;
                float _OutlineIntensity;

                // 呼吸参数
                float _BreathSpeed;
                float _BreathAmplitude;
                float _BreathMinScale;
                fixed4 _BreathColorShift;

                v2f vertOutline(appdata v)
                {
                    v2f o;

                    // 顶点法线外扩（Inverted Hull）
                    float3 norm = normalize(v.normal);
                    float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
                    float3 offset = norm * _OutlineWidth * 10.0;   // 厚度调节系数

                    // 呼吸效果：缩放 + 轻微扰动
                    float breath = sin(_Time.y * _BreathSpeed) * 0.5 + 0.5; // 0~1 呼吸波
                    float scale = lerp(_BreathMinScale, 1.0, breath * _BreathAmplitude);
                    offset *= scale;

                    v.vertex.xyz += offset;

                    o.vertex = UnityObjectToClipPos(v.vertex);

                    // 呼吸颜色与强度
                    float intensity = _OutlineIntensity * (1.0 + breath * _BreathAmplitude * 0.8);
                    fixed4 breathColor = _OutlineColor + _BreathColorShift * breath;

                    o.color = breathColor * intensity;
                    o.color.a = _OutlineColor.a * (0.7 + breath * 0.3); // 轻微透明呼吸

                    return o;
                }

                fixed4 fragOutline(v2f i) : SV_Target
                {
                    return i.color;
                }
                ENDCG
            }
        }

            FallBack "Diffuse"
}