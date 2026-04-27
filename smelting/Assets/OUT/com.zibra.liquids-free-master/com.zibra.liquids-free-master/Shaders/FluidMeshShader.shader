Shader "ZibraLiquids/FluidMeshShader"
{
    // 新增：完整贴图控制参数（适配容器 + 控制显示）
    Properties
    {
        _LavaTex("岩浆贴图", 2D) = "white" {}
        _LavaTiling("贴图重复次数", Vector) = (1, 1, 0, 0) // X/Y 控制重复，匹配容器比例
        _LavaScale("纹理整体大小", Float) = 1.0 // 1=完整显示，<1=放大，>1=缩小
        _LavaSpeed("流动速度", Vector) = (0.0, 0.15, 0, 0) // 向下流动更自然
        _LavaIntensity("岩浆亮度", Float) = 3.0
        _LavaColor("岩浆色调", Color) = (1.0, 0.22, 0.04, 1.0)
    }

        SubShader
        {
            Pass
            {
                Cull Off
                ZWrite Off
                ZTest Always

                HLSLPROGRAM

                #pragma multi_compile_local __ HDRP
                #pragma multi_compile_local __ CUSTOM_REFLECTION_PROBE
                #pragma multi_compile_local __ VISUALIZE_SDF
                #pragma multi_compile_local __ FLIP_BACKGROUND
                #pragma multi_compile_local __ UNDERWATER_RENDER
                #pragma instancing_options procedural:setup
                #pragma vertex VSMain
                #pragma fragment PSMain
                #pragma target 3.0
                #include "UnityCG.cginc"
                #include "UnityStandardBRDF.cginc"
                #include "UnityImageBasedLighting.cginc"

            // 岩浆参数（新增容器适配参数）
            sampler2D _LavaTex;
            float4 _LavaTex_ST;
            float2 _LavaTiling;
            float _LavaScale;
            float2 _LavaSpeed;
            float _LavaIntensity;
            float4 _LavaColor;

            struct VSIn
            {
                uint vertexID : SV_VertexID;
            };

            struct VSOut
            {
                float4 position : POSITION;
                float3 raydir : TEXCOORD1;
                float2 uv : TEXCOORD0;
            };

            struct PSOut
            {
                float4 color : COLOR;
            };

            float4x4 ProjectionInverse;
            float4x4 ViewProjectionInverse;
            float4x4 EyeRayCameraCoeficients;
            float Roughness;
            float AbsorptionAmount;
            float ScatteringAmount;
            float RefractionDistortion;
            float4 RefractionColor;
            float4 ReflectionColor;
            float4 EmissiveColor;
            float Metalness;
            float FoamIntensity;
            float FoamAmount;
            float3 GridSize;
            float3 ContainerScale; // 流体容器缩放（关键！）
            float3 ContainerPosition; // 流体容器位置
            float LiquidIOR;

#ifdef HDRP
            float3 LightColor;
            float3 LightDirection;
#endif

            UNITY_DECLARE_TEXCUBE(ReflectionProbe);
            float4 ReflectionProbe_BoxMax;
            float4 ReflectionProbe_BoxMin;
            float4 ReflectionProbe_ProbePosition;
            float4 ReflectionProbe_HDR;
            float4 WorldSpaceLightPos;
            float2 TextureScale;
            sampler2D Background;
            float4 Background_TexelSize;
            StructuredBuffer<int> Quads;
            StructuredBuffer<int> VertexIDGrid;
            StructuredBuffer<float4> Vertices;
            sampler2D _CameraDepthTexture;

            float2 GetFlippedUV(float2 uv)
            {
                if (_ProjectionParams.x > 0) return float2(uv.x, 1 - uv.y);
                return uv;
            }

            float2 GetFlippedUVBackground(float2 uv)
            {
                uv = GetFlippedUV(uv);
#ifdef FLIP_BACKGROUND
                uv.y = 1 - uv.y;
#else
                if (Background_TexelSize.y < 0) uv.y = 1 - uv.y;
#endif
                return uv;
            }

            float4 ComputeClipSpacePosition(float2 positionNDC, float deviceDepth)
            {
                float4 positionCS = float4(positionNDC * 2.0 - 1.0, deviceDepth, 1.0);
#if UNITY_UV_STARTS_AT_TOP
                positionCS.y = -positionCS.y;
#endif
                return positionCS;
            }

            float3 ComputeWorldSpacePosition(float2 positionNDC, float deviceDepth, float4x4 invViewProjMatrix)
            {
                float4 positionCS = ComputeClipSpacePosition(positionNDC, deviceDepth);
                float4 hpositionWS = mul(invViewProjMatrix, positionCS);
                return hpositionWS.xyz / hpositionWS.w;
            }

            float3 DepthToWorld(float2 uv, float depth)
            {
                return ComputeWorldSpacePosition(uv, depth, ViewProjectionInverse);
            }

            float4 GetDepthAndPos(float2 uv)
            {
                float depth = tex2D(_CameraDepthTexture, uv).x;
                float3 pos = DepthToWorld(uv, depth);
                return float4(pos, depth);
            }

            float PositionToDepth(float3 pos)
            {
                float4 clipPos = mul(UNITY_MATRIX_VP, float4(pos, 1));
                return (1.0 / clipPos.w - _ZBufferParams.w) / _ZBufferParams.z;
            }

            float3 PositionToScreen(float3 pos)
            {
                float4 clipPos = mul(UNITY_MATRIX_VP, float4(pos, 1));
                clipPos = ComputeScreenPos(clipPos);
                return float3(clipPos.xy / clipPos.w, (1.0 / clipPos.w - _ZBufferParams.w) / _ZBufferParams.z);
            }

            float3 BoxProjection(float3 rayOrigin, float3 rayDir, float3 cubemapPosition, float3 boxMin, float3 boxMax)
            {
                float3 tMin = (boxMin - rayOrigin) / rayDir;
                float3 tMax = (boxMax - rayOrigin) / rayDir;
                float3 t1 = min(tMin, tMax);
                float3 t2 = max(tMin, tMax);
                float tFar = min(min(t2.x, t2.y), t2.z);
                return normalize(rayOrigin + rayDir * tFar - cubemapPosition);
            };

            float3 SampleCubemap(float3 pos, float3 ray, float roughness)
            {
                Unity_GlossyEnvironmentData g; g.roughness = roughness;
#if defined(CUSTOM_REFLECTION_PROBE) || defined(HDRP)
                g.reflUVW = BoxProjection(pos, ray, ReflectionProbe_ProbePosition, ReflectionProbe_BoxMin, ReflectionProbe_BoxMax);
                return Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(ReflectionProbe), ReflectionProbe_HDR, g);
#else
                g.reflUVW = ray; g.roughness = roughness;
                return Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, g);
#endif
            }

            float3 ComputeMaterial(float3 cameraPos, float3 cameraRay, float3 normal, float3 lightDirection, float3 lightColor)
            {
                float3 worldView = -cameraRay; float4 reflColor = ReflectionColor;
                float3 H = normalize(lightDirection + worldView);
                float NH = BlinnTerm(normal, H); float NL = DotClamped(normal, lightDirection);
                float NV = abs(dot(normal, worldView));
                half V = SmithBeckmannVisibilityTerm(NL, NV, Roughness);
                half D = NDFBlinnPhongNormalizedTerm(NH, RoughnessToSpecPower(Roughness));
                return lightColor * max(0, (V * D) * (UNITY_PI / 4) * NL);
            }

            float Average(float3 x) { return (x.x + x.y + x.z) / 3.0; }
            float RefractionMinimumDepth; float RefractionDepthBias;

            float3 RefractSample(float3 pos, float3 ray)
            {
                float3 uvz = PositionToScreen(pos);
                float scene_depth = tex2D(_CameraDepthTexture, uvz.xy).x;
                float3 CubeMapSample = Average(ReflectionColor.xyz) * SampleCubemap(pos, ray, 0.05);
                float3 BackgroundSample = tex2D(Background, GetFlippedUVBackground(uvz.xy)).xyz;
                float Interpolate = smoothstep(0.0, 0.1, min(min(uvz.x, uvz.y), min(1.0 - uvz.x, 1.0 - uvz.y)));
                return lerp(CubeMapSample, BackgroundSample, Interpolate);
            }

            float3 ReflectSample(float3 pos, float3 ray)
            {
                return Average(ReflectionColor.xyz) * SampleCubemap(pos, ray, 0.05);
            }

            float3 ComputeScattering(float depth) { return exp(min(-depth * ScatteringAmount, 0.0)); }
            float3 ComputeAbsorption(float depth) { return exp(min(-(1.0 - RefractionColor.xyz) * depth * AbsorptionAmount, 0.0)); }

            #define SHADING
            #include <RenderingUtils.cginc>

            float3 GetCameraRay(float2 uv)
            {
                float2 c = float2(2.0f * uv.x - 1.0f, -2.0f * uv.y + 1.0f);
                float3 r = EyeRayCameraCoeficients[0].xyz;
                float3 u = EyeRayCameraCoeficients[1].xyz;
                float3 v = EyeRayCameraCoeficients[2].xyz;
                return normalize(c.x * r + c.y * u + v);
            }

            VSOut VSMain(VSIn input)
            {
                VSOut output;
                float2 vertexBuffer[4] = { {0,0},{0,1},{1,0},{1,1} };
                uint indexBuffer[6] = {0,1,2,2,1,3};
                uint indexID = indexBuffer[input.vertexID];
                float2 uv = vertexBuffer[indexID];
                float2 flippedUV = GetFlippedUV(uv);
                output.position = float4(2 * flippedUV.x - 1, 1 - 2 * flippedUV.y, 0.5, 1);
                output.uv = uv;
                output.raydir = GetCameraRay(uv);
                return output;
            }

            Texture2D<float4> MeshRenderData;
            Texture2D<float> MeshDepth;
            float4 MeshRenderData_TexelSize;

            PSOut PSMain(VSOut input)
            {
                PSOut output;
                float3 cameraPos = _WorldSpaceCameraPos;
                float3 cameraRay = normalize(input.raydir);
                int3 pixelCoord = int3(input.position.xy, 0);
                if (_ProjectionParams.x > 0) pixelCoord.y = MeshRenderData_TexelSize.w - pixelCoord.y;

                float4 data = MeshRenderData.Load(pixelCoord);
                uint encodedNormal = asuint(data.w);
                float liquidDepth = MeshDepth.Load(pixelCoord);
                float sceneDepth = tex2D(_CameraDepthTexture, input.uv).x;

                if (!any(data.xyz) && !encodedNormal) discard;
#ifndef UNDERWATER_RENDER
                if (liquidDepth < sceneDepth) discard;
#endif
                float3 normal = DecodeDirection(asuint(encodedNormal));

                // ===================== 核心修复：容器适配 + 完整贴图 UV 计算 =====================
                // 1. 获取流体真实世界坐标
                float3 worldPos = DepthToWorld(input.uv, liquidDepth);
                // 2. 转换到流体容器本地 0-1 UV 空间（关键！匹配容器比例，彻底消除拉伸）
                float3 localPos = (worldPos - (ContainerPosition - ContainerScale * 0.5)) / ContainerScale;
                // 3. 用容器本地 UV + 平铺 + 整体缩放，生成最终采样 UV
                float2 lavaUV = localPos.xz * _LavaTiling * _LavaScale + _Time.y * _LavaSpeed;
                // 4. 采样贴图（保证完整显示，不拉伸）
                float3 lavaTex = tex2D(_LavaTex, lavaUV).rgb;
                float3 finalLava = lavaTex * _LavaColor.rgb * _LavaIntensity;
                // ================================================================================

                output.color = float4(finalLava, 1.0);
                return output;
            }
            ENDHLSL
        }
        }
}