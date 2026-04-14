Shader "Custom/LeafWind_BuiltIn_Final"
{
    Properties
    {
        _MainTex("Leaf Texture", 2D) = "white" {}
        _WindStrength("Wind Strength", Range(0, 2)) = 0.3
        _WindSpeed("Wind Speed", Range(0, 5)) = 1.5
        _WindFrequency("Wind Frequency", Range(0.1, 10)) = 3.0
        _SwingAmplitude("Swing Amplitude", Range(0, 0.5)) = 0.15

        _Color("Leaf Color", Color) = (1,1,1,1)
        _Brightness("Brightness", Range(0.1, 3)) = 1.0
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "RenderType" = "Transparent"
                "IgnoreProjector" = "True"
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
                    float4 pos : SV_POSITION;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float _WindStrength;
                float _WindSpeed;
                float _WindFrequency;
                float _SwingAmplitude;
                float4 _Color;
                float _Brightness;

                v2f vert(appdata v)
                {
                    v2f o;
                    float3 pos = v.vertex.xyz;

                    float time = _Time.y * _WindSpeed;

                    // 风吹摆动（算法不变）
                    float wind = sin(time * _WindFrequency + pos.x * 5.0) * _WindStrength;
                    float swing = sin(time * 2.0 + pos.y * 8.0) * _SwingAmplitude;

                    pos.x += wind + swing;

                    // Built-in 标准剪贴空间转换
                    o.pos = UnityObjectToClipPos(pos);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.uv);

                // 颜色 + 亮度
                col.rgb *= _Color.rgb * _Brightness;
                col.a *= _Color.a;

                return col;
            }
            ENDCG
        }
        }
            FallBack "Transparent/VertexLit"
}