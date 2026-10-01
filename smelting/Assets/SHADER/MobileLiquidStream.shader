// 移动端流带 shader：模拟 Zibra 水流/铁水流的半透明流动条带
// uv.y 沿流动方向（弧形水带/明渠水带/铁水带共用，材质参数区分）
Shader "Custom/MobileLiquidStream"
{
    Properties
    {
        _Color("水体颜色", Color) = (0.30, 0.80, 0.75, 0.50)
        _RimColor("边缘高光色", Color) = (0.95, 1.0, 1.0, 1.0)
        _RimPower("边缘锐度", Float) = 2.5
        _FlowSpeed("流速", Float) = 1.5
        _FlowScale("纹理密度", Float) = 9.0
        _Brightness("亮度", Float) = 1.0
        _MaxAlpha("最大不透明度", Range(0, 1)) = 0.8
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

            fixed4 _Color;
            fixed4 _RimColor;
            float _RimPower;
            float _FlowSpeed;
            float _FlowScale;
            float _Brightness;
            float _MaxAlpha;

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
                // 两层错相流动波纹，制造液体内部流动感
                float t = _Time.y * _FlowSpeed;
                float w1 = sin(i.uv.y * _FlowScale - t * 2.0 + sin(i.uv.x * _FlowScale * 0.6 + t * 0.7) * 1.3);
                float w2 = sin(i.uv.y * _FlowScale * 2.3 + t * 3.1 + i.uv.x * _FlowScale * 1.7);
                float w3 = sin(i.uv.y * _FlowScale * 4.7 - t * 4.3 + i.uv.x * _FlowScale * 0.9);
                float flow = w1 * 0.5 + w2 * 0.3 + w3 * 0.2; // -1..1

                float body = 0.78 + flow * 0.22;

                // 菲涅尔：侧视时边缘发亮，模拟玻璃质感水流
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.wpos);
                float fres = pow(1.0 - saturate(dot(viewDir, normalize(i.wnormal))), _RimPower);

                fixed3 col = _Color.rgb * body + _RimColor.rgb * (fres * 0.85 + saturate(flow) * 0.12);
                col *= _Brightness;

                float alpha = saturate(_Color.a * (0.72 + flow * 0.28) + fres * 0.35);
                return fixed4(col, min(alpha, _MaxAlpha));
            }
            ENDCG
        }
    }
    FallBack "Unlit/Transparent"
}
