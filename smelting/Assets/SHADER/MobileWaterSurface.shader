// 移动端水面 shader：滚动法线贴图波纹 + 菲涅尔 + 微光斑
// 用于水锤水槽水面（水平 Quad）
Shader "Custom/MobileWaterSurface"
{
    Properties
    {
        _Color("水色", Color) = (0.12, 0.45, 0.50, 0.60)
        _RimColor("边缘高光色", Color) = (0.9, 1.0, 1.0, 1.0)
        _NormalTex("波纹法线贴图", 2D) = "bump" {}
        _TexScale("法线平铺密度", Float) = 3.0
        _RippleSpeed("波纹速度", Float) = 0.35
        _NormalStrength("波纹强度", Float) = 0.7
        _SparklePower("波光锐度", Float) = 5.0
        _FresnelPower("边缘不透明度", Float) = 2.0
        _MaxAlpha("最大不透明度", Range(0, 1)) = 0.9
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

            sampler2D _NormalTex;
            float4 _NormalTex_ST;
            fixed4 _Color;
            fixed4 _RimColor;
            float _TexScale;
            float _RippleSpeed;
            float _NormalStrength;
            float _SparklePower;
            float _FresnelPower;
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
                // 两层不同方向/速度滚动的法线叠加成水面波纹
                float2 uv1 = i.uv * _TexScale + float2(_Time.y * _RippleSpeed, _Time.y * _RippleSpeed * 0.6);
                float2 uv2 = i.uv * _TexScale * 1.7 - float2(_Time.y * _RippleSpeed * 0.8, _Time.y * _RippleSpeed * 0.45);
                float3 n1 = UnpackNormal(tex2D(_NormalTex, uv1));
                float3 n2 = UnpackNormal(tex2D(_NormalTex, uv2));
                float2 ripple = (n1.xy + n2.xy) * 0.5 * _NormalStrength;

                // 用波纹扰动模拟随机的太阳波光
                float sparkle = pow(saturate(ripple.x * 0.6 + ripple.y * 0.6 + 0.35), _SparklePower);

                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.wpos);
                float fresnel = pow(1.0 - saturate(dot(viewDir, normalize(i.wnormal))), _FresnelPower);
                float alpha = lerp(_Color.a, _MaxAlpha, fresnel);

                fixed3 col = _Color.rgb + _RimColor.rgb * sparkle * 0.55;
                return fixed4(col, alpha);
            }
            ENDCG
        }
    }
    FallBack "Unlit/Transparent"
}
