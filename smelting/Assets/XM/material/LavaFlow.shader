Shader "Custom/LavaFlow"
{
    Properties
    {
        // 主材质贴图（你的岩浆纹理）
        _MainTex("岩浆基础贴图", 2D) = "white" {}
    // 岩浆主颜色
    _Color("岩浆主色", Color) = (1,0.3,0,1)
        // 自发光强度（让岩浆发亮）
        _GlowIntensity("自发光强度", Range(0,5)) = 2.0

        // 第一层流动速度（X横向 / Y纵向）
        _FlowSpeed1("流动速度 1", Vector) = (0.5, 0.1, 0, 0)
        // 第二层流动速度（双层流动更自然）
        _FlowSpeed2("流动速度 2", Vector) = (0.2, 0.3, 0, 0)
    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // 变量声明（和Properties对应）
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _GlowIntensity;
            float2 _FlowSpeed1;
            float2 _FlowSpeed2;

            // 顶点着色器输入结构体
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // 片元着色器输入结构体
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // 顶点着色器：处理坐标和UV
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // UV平铺+偏移（支持Unity材质面板的Tiling/Offset）
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // 片元着色器：核心渲染逻辑
            fixed4 frag(v2f i) : SV_Target
            {
                // ===================== 核心：双层UV流动动画 =====================
                // 时间驱动UV偏移，实现流动效果
                float2 uv1 = i.uv + _Time.x * _FlowSpeed1;
                float2 uv2 = i.uv + _Time.x * _FlowSpeed2;

                // 采样两层纹理并混合，让流动效果更细腻
                fixed4 tex1 = tex2D(_MainTex, uv1);
                fixed4 tex2 = tex2D(_MainTex, uv2);
                fixed4 finalTex = (tex1 + tex2) * 0.5;

                // ===================== 颜色与自发光 =====================
                // 贴图 * 主颜色 * 自发光强度
                fixed4 col = finalTex * _Color * _GlowIntensity;

                return col;
            }
            ENDCG
        }
    }
        FallBack "Diffuse"
}