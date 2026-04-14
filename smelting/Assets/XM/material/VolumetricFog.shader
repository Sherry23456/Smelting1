Shader "Hidden/VolumetricFogSimple"
{
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float4 _MainTex_TexelSize;

            float4 _FogColor;
            float _Density;
            float _FogStart;
            float _FogEnd;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float linearDepth = Linear01Depth(depth) * _ProjectionParams.z;

                float fogFactor = smoothstep(_FogStart, _FogEnd, linearDepth);
                fogFactor *= _Density;

                col.rgb = lerp(col.rgb, _FogColor.rgb, fogFactor);
                return col;
            }
            ENDCG
        }
    }
}