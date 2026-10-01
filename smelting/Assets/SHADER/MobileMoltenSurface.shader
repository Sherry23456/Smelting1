// 移动端熔液表面 shader：双层流动岩浆/铁水纹理 + 强自发光（配合 Bloom 出辉光）
// 用于高炉熔池、模具铁水表面
Shader "Custom/MobileMoltenSurface"
{
    Properties
    {
        _MainTex("熔液贴图", 2D) = "white" {}
        _Color("熔液色调", Color) = (1.0, 0.55, 0.18, 1.0)
        _HotColor("高温核心色", Color) = (1.0, 0.95, 0.55, 1.0)
        _TexScale("贴图平铺", Float) = 1.0
        _Flow1("流动方向 1", Vector) = (0.08, 0.05, 0, 0)
        _Flow2("流动方向 2", Vector) = (-0.05, 0.09, 0, 0)
        _EmissionPower("发光强度", Float) = 2.6
        _HDRBoost("HDR 增强", Float) = 1.6
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
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 wpos : TEXCOORD1;
                float3 wnormal : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _HotColor;
            float _TexScale;
            float4 _Flow1;
            float4 _Flow2;
            float _EmissionPower;
            float _HDRBoost;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.wnormal = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 双层不同速度的流动纹理叠加
                float2 uv1 = i.uv * _TexScale + _Time.y * _Flow1.xy;
                float2 uv2 = i.uv * _TexScale * 1.9 + _Time.y * _Flow2.xy;
                fixed3 a = tex2D(_MainTex, uv1).rgb;
                fixed3 b = tex2D(_MainTex, uv2).rgb;
                fixed3 flow = (a + b) * 0.5;

                // 正对观察（俯视）时更热更亮，侧视稍暗，形成“中心炽热”的熔池感
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.wpos);
                float facing = saturate(dot(viewDir, normalize(i.wnormal)));
                fixed3 col = lerp(flow, flow * _HotColor.rgb * 1.4, facing * 0.45);
                col *= _Color.rgb * _EmissionPower;

                // HDR 输出（相机开 HDR 时可触发 Bloom 辉光）
                return fixed4(col * _HDRBoost, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Unlit/Texture"
}
