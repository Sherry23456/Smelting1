// 移动端水锤水体替代 shader：顶点涟漪 + 波光 + 菲涅尔边缘不透明
// 用于水锤水池水面（水平 Quad）
Shader "Custom/MobileWaterFallback"
{
    Properties
    {
        _BaseColor("水色", Color) = (0.08, 0.4, 0.45, 0.5)
        _GlintColor("波光色", Color) = (0.85, 1.0, 1.0, 1.0)
        _WaveSpeed("波纹速度", Float) = 0.8
        _WaveScale("波纹密度", Float) = 4.0
        _WaveStrength("波纹起伏", Float) = 0.02
        _GlintPower("波光锐度", Float) = 6.0
        _FresnelPower("边缘不透明度", Float) = 2.0
        _MaxAlpha("最大不透明度", Range(0,1)) = 0.85
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
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 wpos : TEXCOORD0;
                float3 wnormal : TEXCOORD1;
            };

            fixed4 _BaseColor;
            fixed4 _GlintColor;
            float _WaveSpeed;
            float _WaveScale;
            float _WaveStrength;
            float _GlintPower;
            float _FresnelPower;
            float _MaxAlpha;

            v2f vert(appdata v)
            {
                v2f o;
                float3 wp = mul(unity_ObjectToWorld, v.vertex).xyz;
                // 两组正弦叠加的顶点涟漪
                float w = sin(wp.x * _WaveScale + _Time.y * _WaveSpeed)
                        * cos(wp.z * _WaveScale * 0.8 + _Time.y * _WaveSpeed * 1.3);
                wp.y += w * _WaveStrength;
                o.pos = mul(UNITY_MATRIX_VP, float4(wp, 1.0));
                o.wpos = wp;
                o.wnormal = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.wpos);
                float fresnel = pow(1.0 - saturate(dot(viewDir, normalize(i.wnormal))), _FresnelPower);
                float alpha = lerp(_BaseColor.a, _MaxAlpha, fresnel);

                // 两组各向异性的高光带，拉长成条纹状波光
                float t = _Time.y * _WaveSpeed;
                float gx = sin(i.wpos.x * _WaveScale * 3.1 + i.wpos.z * _WaveScale * 0.7 + t * 2.2);
                float gz = sin(i.wpos.x * _WaveScale * 1.3 - i.wpos.z * _WaveScale * 2.3 - t * 1.7);
                float glint = pow(saturate(gx * 0.5 + gz * 0.5 + 0.35), _GlintPower);

                fixed3 col = _BaseColor.rgb + _GlintColor.rgb * glint * 0.6;
                return fixed4(col, alpha);
            }
            ENDCG
        }
    }
    FallBack "Unlit/Transparent"
}
