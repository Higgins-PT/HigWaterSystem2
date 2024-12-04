Shader"Unlit/CausticEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            //#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
            // -------------------------------------
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "HLSLQuaternion.hlsl"

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
float _CausticDistance;
float _CausticPow;
float _CausticIntensity;
float4 _CausticColor;
float4x4 _ProjMat;
float4x4 _InverViewMat;
sampler2D _MainTex;
float4 _MainTex_ST;
sampler2D _WaterDepthTexture;
sampler2D _CameraDepth;
sampler2D _RefractionTex;
uint detailCount;
sampler2D _DetailTex0;
sampler2D _DetailTex1;
float _DetailTexMapSize0;
float _DetailTexMapSize1;

float _DetailTexRenderDistance0;
float _DetailTexRenderDistance1;
float2 GetMapUV(float2 worldPos, float mapSize)
{
    return ((worldPos % mapSize + mapSize) % mapSize) / mapSize;

}
float4 GetDetailValue(float2 worldPos, float range)
{
    if (detailCount == 0)
    {
        return float4(0, 1, 0, 0);

    }
    else if (detailCount == 1)
    {
        return tex2D(_DetailTex0, GetMapUV(worldPos, _DetailTexMapSize0)).xyzw;
    }
    else if (detailCount == 2)
    {
        float4 nor_1 = float4(0, 0, 0, 0);
        nor_1 = tex2D(_DetailTex0, GetMapUV(worldPos, _DetailTexMapSize0)).xyzw;
        nor_1.xyz = lerp(nor_1.xyz, float3(0, 1, 0), clamp(range / _DetailTexRenderDistance0, 0, 1));
        float4 nor_2 = float4(0, 0, 0, 0);
        nor_2 = tex2D(_DetailTex1, GetMapUV(worldPos, _DetailTexMapSize1)).xyzw;
        nor_2.xyz = lerp(nor_2.xyz, float3(0, 1, 0), clamp(range / _DetailTexRenderDistance1, 0, 1));
        return float4(AddNormals(nor_1.xyz, nor_2.xyz), nor_1.w + nor_2.w);
    }
    else
    {
        return float4(0, 1, 0, 0);
    }

    return float4(0, 1, 0, 0);
}
float3 UVToWorld(float2 uv, float depth)
{
    float2 p11_22 = float2(_ProjMat._11, _ProjMat._22);
    float3 vpos = float3((uv * 2 - 1) / p11_22, -1) * depth;
    float4 wposVP = mul(_InverViewMat, float4(vpos, 1));
    return wposVP.xyz;
}
v2f vert(appdata v)
{
    v2f o;
    o.vertex = TransformWorldToHClip(TransformObjectToWorld(v.vertex.xyz));
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    return o;
}

float4 frag(v2f i) : SV_Target
{

    float waterDepth = tex2D(_WaterDepthTexture, i.uv).w;
    float4 cameraDepthTex = tex2D(_CameraDepth, i.uv);
    float depth = cameraDepthTex.x;
    float4 color = tex2D(_RefractionTex, i.uv);
    float3 worldPos = UVToWorld(i.uv, depth);
    float3 lightDir = _MainLightPosition.xyz;
    float4 SHADOW_COORDS = TransformWorldToShadowCoord(worldPos);
    Light light = GetMainLight(SHADOW_COORDS);
    half originShadow = light.shadowAttenuation;
    float enterDepth = abs(cameraDepthTex.x - waterDepth);


    float2 mapPos = sqrt(enterDepth * enterDepth - worldPos.y * worldPos.y) * lightDir.xz + worldPos.xz;
    float3 normal = GetDetailValue(mapPos, depth).xyz;
    float4 caustic = _CausticColor * pow(clamp(dot(lightDir, normal), 0, 1), _CausticPow) * _CausticIntensity * originShadow * clamp(1 - enterDepth / _CausticDistance, 0, 1);
    return color + caustic;
}
            ENDHLSL
        }
    }
}
