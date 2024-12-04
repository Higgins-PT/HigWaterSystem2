Shader"HigWater/WaterPlane"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CullMode("Cull Mode", Int) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
LOD 100

        Pass
        {

Cull [_CullMode]

            Name"Rendering Pass"
Tags
{
"LightMode"="UniversalForwardOnly"
}
            HLSLPROGRAM
            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
            // -------------------------------------
			#pragma multi_compile_fragment _ Anti_Aliasing_ON
            #pragma multi_compile_fragment RENDERINGMOD SSRMOD SSAOMOD

            #pragma multi_compile_fragment __ WATERMASKMOD DEPTHMASKMOD
            #pragma multi_compile_instancing
            #pragma multi_compile_fragment __ MASKMOD
            #pragma multi_compile_fragment REFLECTION_OFF REFLECTION_FAKE REFLECTION_SSR
            #pragma multi_compile_fragment REFRACTION_OFF REFRACTION_FAKE REFRACTION_SSR
            #pragma vertex vert
            #pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "LodVert.hlsl"
#include "WaterRendering/BasicLighting.hlsl"
#include "WaterRendering/SSR.hlsl"
#include "WaterRendering/FakeReflection.hlsl"
#include "WaterRendering/WaterRendering.hlsl"
#include "HLSLQuaternion.hlsl"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
    float3 worldPos : TEXCOORD1;
    float3 oriWorldPos : TEXCOORD2;
    float3 wave : TEXCOORD3;
    float2 data : TEXCOORD4;
    float4 waveData : TEXCOORD5;
};


//-----------------------------gridDataInput
CBUFFER_START(GridDataInput)
float depthOffest;
sampler2D _MainTex;
float4 _MainTex_ST;
float3 planePos;
float gridSize;
float gridMaxSize;
float gridMinSize;
float planeSize;
float planeSize_Next;
float cellSize;
float3 neiborPlaneOffestPos;
sampler2D _VertTex;
sampler2D _VertTex_Next;

sampler2D _NormalTex;
sampler2D _NormalTex_Next;
sampler2D _FoamTex;
sampler2D _FoamTex_Next;
sampler2D _DetailTex0;
sampler2D _DetailTex1;
float _DetailTexMapSize0;
float _DetailTexMapSize1;

sampler2D _DetailTex_Vert0;
sampler2D _DetailTex_Vert1;
float _DetailTexMapSize_Vert0;
float _DetailTexMapSize_Vert1;
float _DetailTexRenderDistance0;
float _DetailTexRenderDistance1;
int _MaxLod;
uint detailCount;
uint foamTexSize;

uint vertTexSize;
uint vertTexSize_Next;
uint normalTexSize;
uint normalTexSize_Next;
//----------------------------------Fog
float _FogDistance;
float _FogPow;
float3 _FogColor;
int _FogEnable;

CBUFFER_END
//-----------------------------WaterAttribute
CBUFFER_START(WaterAttribute)

float _SafeCode;
float3 _CamPos;

float _RenderNormalDistance;
float _RenderFoamDistance;

float _ShadowIntensity;
float3 _LightColor;
float3 _ShadowColor;
float3 _AmbientColor;
float3 _WaterColor;
float3 _ReflectionColor;
float3 _RefractionColor;

float _AmbientIntensity;
float _DiffuseIntensity;
float _Roughness;

float _HighLightIntensity;
float _HighLightHDRMax;

float _PointLightRoughness;
float _PointLightIntensity;
float _PointLightHDRMax;

float _LightPropagationRatio;
float _FReflectionInitRatio;
float _PeakThreshold;
float _PeakScale;

float _PeakFoamThreshold;
float _PeakFoamIntensity;
float _FoamIntensity;

float _JacobianMaxValue;
float _LambertFactor;
float _ReflectionBlendRatio;
int _DetailTexMapVertEnable0;
int _DetailTexMapVertEnable1;
sampler2D _FoamDetail;
float2 _FoamDetail_Scale;
float3 _SkyColor;
float _SkyColorHDR;
CBUFFER_END
//-------------SSS
CBUFFER_START(RayTrace)

float3 _SSColour;
float _SSSIndensity;
float _SSScale;
float _SSSWaveHeight;
float _SSSRenderDistance;

//------------------------------SSR
float _Size;
int _TexSize;
int _MipCount;
float3 _SSRReflection_Color;
float3 _SSRRefraction_Color;

float _DistortionIntensity_Re;
float _DistortionIntensity_Refr;
float _SSRStep_Refl;
float _SSRThickness_Refl;
float _SSRStep_Refr;
float _SSRThickness_Refr;

float _DepthMax;
float _SSRIntensity_Reflection;
float _SSRIntensity_Refraction;
float _SSDepthMax;
float4 _SSDepthColor[100];
float _ReflectClipOffest;
float4x4 _ViewMatrixRe;
float4x4 _ProjectionMatrixRe;
float4x4 _InverViewMatrixRe;
float4x4 _InverProjectionMatrixRe;

float4x4 _ViewMatrixRefra;
float4x4 _ProjectionMatrixRefra;
float4x4 _InverViewMatrixRefra;
float4x4 _InverProjectionMatrixRefra;

sampler2D _ReflectionHizBuffer;
sampler2D _ReflectionScreenTex;
sampler2D _RefractionHizBuffer;
sampler2D _RefractionScreenTex;
sampler2D _SSRTex;
float3 _SSRReflectionColor;
float3 _SSRRefractionColor;

float3 _CamPos_Refl;
float3 _CamPos_Refr;
float _SSRReflectRoughness;
float _SSRRefractRoughness;
float _SkyReflectRoughness;
float _ReflectionIntensity;
float _RefractionIntensity;
float _UnderWaterSSRIntensity_Refraction;

float _UnderWaterRefractRoughness;
float _UnderWaterRefractIntensity;
float _ExtendRefractionRange;

int _SSR_ON;
//-------------------------------SSAO
float _SSAONormalDisturbanceIntensity;
int _SSAOEnable;
sampler2D _SSAOTex;

samplerCUBE _SkyBox;
CBUFFER_END
//-------------------------------HeightRT
CBUFFER_START(CamMap)

sampler2D _HeightTex;
sampler2D _HeightTex_Next;
sampler2D _AldobeTex;
sampler2D _WaveDataTex;
sampler2D _FlowMapTex;
float _HeightTexSize;
float _WaterTime;
float _FlowTimeSpeed;
float _FlowMaxDistance;
int _Flow_On;
int _Height_On;
int _WaveData_On;
int _Aldobe_On;
CBUFFER_END
//---------------------------------InteractiveWater

CBUFFER_START(InteractiveWater)

sampler2D _InteractiveWaterHeightTex;
sampler2D _InteractiveWaterNormalTex;
CBUFFER_END

float2 GetMapUV(float2 worldPos, float mapSize)
{
    return ((worldPos % mapSize + mapSize) % mapSize) / mapSize;

}
float4 GetFlowData(float2 worldPos, float time, float timeSpeed, inout float lerp, float flowMaxDistance)
{
    float2 index = (worldPos + float2(planeSize / 2, planeSize / 2)) / (planeSize);
    float2 flowDir = tex2Dlod(_FlowMapTex, float4(index, 0, 0)).xy;
    float phase0 = frac(time * timeSpeed);
    float phase1 = frac(time * timeSpeed + 0.5);
    float4 result = float4(flowDir * phase0, flowDir * phase1);
    lerp = abs((0.5 - phase0) / 0.5);
    result = clamp(result * flowMaxDistance, -flowMaxDistance, flowMaxDistance);
    return result;

}
float4 GetWaveData(float2 worldPos)
{
    float2 index = (worldPos + float2(planeSize / 2, planeSize / 2)) / (planeSize);
    return tex2Dlod(_WaveDataTex, float4(index, 0, 0));

}
float4 GetAldobeMap(float2 worldPos)
{
    float2 index = (worldPos + float2(planeSize / 2, planeSize / 2)) / (planeSize);
    return tex2Dlod(_AldobeTex, float4(index, 0, 0));

}
float4 GetHeightMap(float2 worldPos)
{
    float length = max(abs(worldPos.x), abs(worldPos.y));
    if (length >= gridSize * 0.9)
    {
        float2 index = (worldPos + float2(planeSize_Next / 2, planeSize_Next / 2) - neiborPlaneOffestPos.xz) / (planeSize_Next);
        return tex2Dlod(_HeightTex_Next, float4(index, 0, 0));
    }
    else
    {
        float2 index = (worldPos + float2(planeSize / 2, planeSize / 2)) / (planeSize);
        return tex2Dlod(_HeightTex, float4(index, 0, 0));

    }


}

float3 GetDetailValue_Vert_TestEnable(float2 worldPos)
{
    if (detailCount == 0)
    {
        return float3(0, 0, 0);

    }
    else if (detailCount == 1)
    {
        if (_DetailTexMapVertEnable0)
        {
            return tex2Dlod(_DetailTex_Vert0, float4(GetMapUV(worldPos, _DetailTexMapSize_Vert0), 0, 0)).xyz;
        }
        else
        {
        
            return float3(0, 0, 0);
        }
    }
    else if (detailCount == 2)
    {
        float3 vert = float3(0, 0, 0);
        if (_DetailTexMapVertEnable0)
        {
            vert += tex2Dlod(_DetailTex_Vert0, float4(GetMapUV(worldPos, _DetailTexMapSize_Vert0), 0, 0)).xyz;

        }
        if (_DetailTexMapVertEnable1)
        {
            vert += tex2Dlod(_DetailTex_Vert1, float4(GetMapUV(worldPos, _DetailTexMapSize_Vert1), 0, 0)).xyz;

        }
        return vert;

    }
    else
    {
        return float3(0, 0, 0);
    }


}
float4 GetVertValue(sampler2D vertTex, float2 worldPos, float2 detailPos, float _PlaneSize)
{
    float2 index = (worldPos + float2(_PlaneSize / 2, _PlaneSize / 2)) / (_PlaneSize);
    float3 detailWave = GetDetailValue_Vert_TestEnable(detailPos);
    
    return float4(tex2Dlod(vertTex, float4(index, 0, 0)).xyz, 0) + float4(detailWave, detailWave.y);

}
float4 GetWave(float2 localPos, float2 worldPos, float gridSize, float _PlaneSize, float _PlaneSizeNext, float2 flowMap)
{
    localPos += flowMap;
    if (max(abs(localPos.x), abs(localPos.y)) >= gridSize)
    {
        return GetVertValue(_VertTex_Next, localPos - neiborPlaneOffestPos.xz, worldPos, _PlaneSizeNext);

    }
    else
    {
        return GetVertValue(_VertTex, localPos, worldPos, _PlaneSize);
    }
}

float4 GetNormalValue(float2 worldPos, sampler2D normalTex, float planeSize)
{
    float2 index = (worldPos + float2(planeSize / 2, planeSize / 2)) / (planeSize);
    float4 normal = tex2Dlod(normalTex, float4(index, 0, 0));
    return normal;

}
float GetFoamValue(float2 worldPos, sampler2D foamTex, float planeSize)
{
    float2 index = (worldPos + float2(planeSize / 2, planeSize / 2)) / (planeSize);
    float foam = tex2Dlod(foamTex, float4(index, 0, 0)).x;
    return foam;

}


float3 GetDetailValue_Vert(float2 worldPos)
{
    if (detailCount == 0)
    {
        return float3(0, 0, 0);

    }
    else if (detailCount == 1)
    {
        if (!_DetailTexMapVertEnable0)
        {
            return tex2D(_DetailTex_Vert0, GetMapUV(worldPos, _DetailTexMapSize_Vert0)).xyz;
        }
        else
        {
            return float3(0, 0, 0);
        }
        return float3(0, 0, 0);
    }
    else if (detailCount == 2)
    {
        float3 vert = float3(0, 0, 0);
        bool hasData = false;
        if (!_DetailTexMapVertEnable0)
        {
            vert += tex2D(_DetailTex_Vert0, GetMapUV(worldPos, _DetailTexMapSize_Vert0)).xyz;
            hasData = true;
        }
        if (!_DetailTexMapVertEnable1)
        {
            vert += tex2D(_DetailTex_Vert1, GetMapUV(worldPos, _DetailTexMapSize_Vert1)).xyz;
            hasData = true;
        }
        return hasData ? vert : float3(0, 0, 0);

    }
    else
    {
        return float3(0, 0, 0);
    }
    return float3(0, 0, 0);

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
        float4 nor_1 = tex2D(_DetailTex0, GetMapUV(worldPos, _DetailTexMapSize0)).xyzw;
        nor_1.xyz = lerp(nor_1.xyz, float3(0, 1, 0), clamp(range / _DetailTexRenderDistance0, 0, 1));
        float4 nor_2 = tex2D(_DetailTex1, GetMapUV(worldPos, _DetailTexMapSize1)).xyzw;
        nor_2.xyz = lerp(nor_2.xyz, float3(0, 1, 0), clamp(range / _DetailTexRenderDistance1, 0, 1));
        return float4(AddNormals(nor_1.xyz, nor_2.xyz), nor_1.w + nor_2.w);
    }
    else
    {
        return float4(0, 1, 0, 0);
    }
    return float4(0, 1, 0, 0);

}

float GetFoam(float2 originWorldPos, float2 planePos)
{
    float2 localPos = originWorldPos - planePos;
    float scale = max(abs(localPos.x), abs(localPos.y)) - (planeSize / 2.3);
    if (scale > 0)
    {
        return lerp(GetFoamValue(originWorldPos - planePos, _FoamTex, planeSize), GetFoamValue(originWorldPos - planePos, _FoamTex_Next, planeSize_Next), 0.5);
    }
    else
    {
        float foam = GetFoamValue(originWorldPos - planePos, _FoamTex, planeSize);
        return foam;
    }

}
float3 GetNormal(float2 originWorldPos, float2 planePos, float range)
{
    float2 localPos = originWorldPos - planePos;
    if (max(abs(localPos.x), abs(localPos.y)) > gridMaxSize)
    {
        return float3(0, 0, 0);

    }
    
    float3 normal = lerp(GetNormalValue(localPos, _NormalTex, planeSize).xyz, GetNormalValue(localPos - neiborPlaneOffestPos.xz, _NormalTex_Next, planeSize_Next).xyz, pow(GetSmoothT(localPos, gridSize, gridMinSize), 1));
    normal = AddNormals(normal, GetDetailValue(originWorldPos, range).xyz);
    return lerp(normal, float3(0, 1, 0), clamp(range / _RenderNormalDistance, 0, 0.9));
}
float GetPeak(float2 originWorldPos, float2 planePos)
{
    float2 localPos = originWorldPos - planePos;
    if (max(abs(localPos.x), abs(localPos.y)) > gridMaxSize)
    {
        return 0;
    }
    float peak = GetNormalValue(originWorldPos - planePos, _NormalTex, planeSize).w;
    
    return peak + GetDetailValue(originWorldPos, 0).w;
}
float GetPeakFoamValue(float2 originWorldPos, float2 planePos)
{
    return 1 - clamp((GetPeak(originWorldPos, planePos) - _JacobianMaxValue - _PeakFoamThreshold) * _PeakFoamIntensity, 0, 1);
}
float GetPeakValue(float2 originWorldPos, float2 planePos)
{
    return 1 - clamp((GetPeak(originWorldPos, planePos) - _JacobianMaxValue - _PeakThreshold) * _PeakScale, 0, 1);
}
float4 RefractDepthColor(float depth)
{
    if (depth < 0)
    {
        return float4(1, 1, 1, 1);

    }
    int index = clamp(floor(depth / _SSDepthMax * 100), 0, 99);
     
    return lerp(_SSDepthColor[index], _SSDepthColor[clamp(index + 1, 0, 99)], frac(depth / _SSDepthMax * 100));

}

float2 clipToScreen(float4 clipPos)
{
    float3 ndcPos = clipPos.xyz / clipPos.w;
    float2 screenPos;
    screenPos.x = (ndcPos.x + 1.0) * 0.5;
    screenPos.y = (1.0 - ndcPos.y) * 0.5; 
    return screenPos;
}
float3 GetNormalWithFlow(float3 oriWorldPos, float2 data, float4 wave, float flowLerp, float4 flowMap)
{
    if (_Flow_On == 1)
    {
        if (all(flowMap))
        {
            float3 normal_0 = lerp(float3(0, 1, 0), GetNormal(oriWorldPos.xz + flowMap.xy, planePos.xz, data.y), clamp(wave.x, 0, 1));
            float3 normal_1 = lerp(float3(0, 1, 0), GetNormal(oriWorldPos.xz + flowMap.zw, planePos.xz, data.y), clamp(wave.x, 0, 1));
            return lerp(normal_0, normal_1, flowLerp);
        }
        else
        {
            return lerp(float3(0, 1, 0), GetNormal(oriWorldPos.xz, planePos.xz, data.y), clamp(wave.x, 0, 1));
        }
    }
    else
    {

        return lerp(float3(0, 1, 0), GetNormal(oriWorldPos.xz, planePos.xz, data.y), clamp(wave.x, 0, 1));
    }
}

float3 GetAddNormal(float3 oriWorldPos, float2 data, float4 wave, float flowLerp, float4 flowMap)
{

    return AddNormals(GetNormalWithFlow(oriWorldPos, data, wave, flowLerp, flowMap), lerp(float3(0, 1, 0), GetHeightMap(oriWorldPos.xz - planePos.xz).xyz, clamp(wave.y, 0, 1)));
}

v2f vert(appdata v)
{
    UNITY_SETUP_INSTANCE_ID(v);
    v2f o;
        
    
    
    float3 worldPos = TransformObjectToWorld(v.vertex.xyz);
    SnapToGrid(worldPos, cellSize / 2);
    LodSmooth(worldPos, planePos, gridSize, gridMinSize, cellSize);
    o.oriWorldPos = worldPos;
    float lerpScale = 0;
    float4 waveData = float4(1, 1, 1, 1);
    if (_WaveData_On == 1)
    {
    
        waveData = GetWaveData(worldPos.xz - planePos.xz);
    }
    else
    {
    
        waveData = float4(1, 1, 1, 1);
    }
    float4 wave = float4(1, 1, 1, 1);
    if (_Flow_On == 1)
    {
    float4 flowMap = GetFlowData(worldPos.xz - planePos.xz, _WaterTime, _FlowTimeSpeed, lerpScale, _FlowMaxDistance);
    wave = lerp(GetWave(worldPos.xz - planePos.xz, worldPos.xz, planeSize / 2, planeSize, planeSize_Next, flowMap.xy).xyzw, GetWave(worldPos.xz - planePos.xz, worldPos.xz, planeSize / 2, planeSize, planeSize_Next, flowMap.zw).xyzw, lerpScale);
    }
    else
    {
        wave = GetWave(worldPos.xz - planePos.xz, worldPos.xz, planeSize / 2, planeSize, planeSize_Next, float2(0, 0)).xyzw;
    }

    if (_Height_On == 1)
    {
    o.wave = (wave.xyz * waveData.x) + (float3(0, GetHeightMap(worldPos.xz - planePos.xz).a, 0) * waveData.y);
    }
    else
    {
    o.wave = (wave.xyz * waveData.x);
    }
    worldPos += o.wave;
    o.vertex = TransformWorldToHClip(worldPos);
    o.worldPos = worldPos;
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.data = float2(max(wave.w - _SSSWaveHeight, 0) / _SSScale, length(worldPos - _CamPos));
    o.waveData = waveData;
    return o;
}
#ifdef MASKMOD

float4 frag(v2f i, bool isFront : SV_IsFrontFace) : SV_Target
{
#ifdef DEPTHMASKMOD
    float detailVert = GetDetailValue_Vert(i.oriWorldPos.xz).y;
    float depth = -TransformWorldToView(i.worldPos + detailVert).z;
    return float4(depth, 1, 1, 1);
#endif
#ifdef WATERMASKMOD
    float detailVert = GetDetailValue_Vert(i.oriWorldPos.xz).y;
    float depth = -TransformWorldToView(i.worldPos + detailVert).z;
if(_MaxLod == 1){
return float4(0, 0, 0, 0);
}
    return isFront ? float4(0, 0, 0, 0) : float4(depth, 1, 1, 1);
#endif
    return float4(0,0,0,0);
}

#else
float4 frag(v2f i, bool isFront : SV_IsFrontFace) : SV_Target
{
    float lerpScale = 0;
    float4 flowMap = float4(0, 0, 0, 0);
    float3 normal = float3(0, 0, 0);
    float2 flowPos0 = float2(0, 0);
    float2 flowPos1 = float2(0, 0);
    float2 flowPos2 = float2(0, 0);
    float2 flowPos3 = float2(0, 0);
    if (_Flow_On == 1)
    { // normal FlowMap influence
        lerpScale = 0;
        flowMap = GetFlowData(i.oriWorldPos.xz - planePos.xz, _WaterTime, _FlowTimeSpeed, lerpScale, _FlowMaxDistance);
        normal = GetAddNormal(i.oriWorldPos, i.data, i.waveData, lerpScale, flowMap);
        flowPos0 = i.oriWorldPos.xz + flowMap.xy;
        flowPos1 = i.oriWorldPos.xz + flowMap.zw;
        flowPos2 = i.worldPos.xz + flowMap.xy;
        flowPos3 = i.worldPos.xz + flowMap.zw;
    }
    else
    {
   
        normal = GetAddNormal(i.oriWorldPos, i.data, i.waveData, 0, float4(0, 0, 0, 0));
    }
    
#if SSRMOD
    float3 viewDir = normalize(i.worldPos - _WorldSpaceCameraPos);  

    float3 result = float3(0, 0, 0);
    float3 reflection = float3(0, 0, 0);
    float3 refration = float3(0, 0, 0);
    float2 uv = clipToScreen(TransformWorldToHClip(i.worldPos)).xy;

#ifdef REFLECTION_FAKE
    reflection = FakeRayTrace(_ReflectionScreenTex, i.worldPos, reflect(viewDir, normal), _CamPos_Refl, float2(_Size, _Size), _ViewMatrixRe, _ProjectionMatrixRe, _DistortionIntensity_Re);
#endif
#ifdef REFLECTION_SSR
    float3 oriDir = reflect(viewDir,normalize(float3(0, 1, 0.01)));
    float3 realDir = reflect(viewDir, normal);
    float3 resultDir = lerp(oriDir, realDir, _SSRReflectRoughness);
    reflection = RayTrace_Refl(i.worldPos, resultDir, _CamPos_Refl, float2(_Size, _Size), _TexSize, _MipCount, _SSRStep_Refl, _ViewMatrixRe, _ProjectionMatrixRe, _InverViewMatrixRe, _InverProjectionMatrixRe, _ReflectionHizBuffer, _ReflectionScreenTex, _SkyBox, _SSRThickness_Refl, _DepthMax, _SSRIntensity_Reflection, i.worldPos.y + _ReflectClipOffest, _SSRReflection_Color);
#endif
    float alpha = 1;
    float refrDepth = -1;
#ifdef REFRACTION_FAKE
    refration = FakeRayTrace(_RefractionScreenTex, i.worldPos, refract(viewDir, normal, 0.751879), _CamPos_Refr, float2(_Size, _Size), _ViewMatrixRefra, _ProjectionMatrixRefra, _DistortionIntensity_Refr);
    float4 nowClipPos = WorldToClip(i.worldPos, _ViewMatrixRefra, _ProjectionMatrixRefra);
    float2 nowScenePos = ClipToScreen(nowClipPos, float2(_Size, _Size));
    float depthTex = GetDepth(nowScenePos / float2(_Size, _Size), 0, _RefractionHizBuffer);
    float startDepth = GetEyeDepthFromWorldPosition(i.worldPos, _ViewMatrixRefra);
    float intensity = 1;
    if(depthTex <= _DepthMax){
    refrDepth = depthTex - startDepth;
    intensity = _SSRIntensity_Refraction;
    }else{
    refrDepth = -1;
    }
    float4 depthColor = RefractDepthColor(refrDepth);
    refration *= depthColor.xyz * intensity;
    alpha = clamp(depthColor.a, 0, 1);
#endif

#ifdef REFRACTION_SSR
    
    float3 oriDirR;
    float3 realDirR;
    float3 resultDirR;
    float intensity;
    if(isFront == false){
    oriDirR = refract(viewDir, -normalize(float3(0, 1, 0.01)), 0.751879);
    realDirR = refract(viewDir, -normal, 0.751879);
    resultDirR = lerp(oriDirR, realDirR, _UnderWaterRefractRoughness);
    intensity = _UnderWaterSSRIntensity_Refraction;
   // resultDirR = viewDir;
    }else{
    oriDirR = refract(viewDir, normalize(float3(0, 1, 0.01)), 0.751879);
    realDirR = refract(viewDir, normal, 0.751879);
    resultDirR = lerp(oriDirR, realDirR, _SSRRefractRoughness);
    intensity = _SSRIntensity_Refraction;
    }

    refration = RayTrace_Refr(i.worldPos, resultDirR, _CamPos_Refr, float2(_Size, _Size), _TexSize, _MipCount, _SSRStep_Refr, _ViewMatrixRefra, _ProjectionMatrixRefra, _InverViewMatrixRefra, _InverProjectionMatrixRefra, _RefractionHizBuffer, _RefractionScreenTex, _SkyBox, _SSRThickness_Refr, _DepthMax, intensity, refrDepth, _SSRRefraction_Color);
    if(isFront == true){
    float4 depthColor = RefractDepthColor(refrDepth);
    refration *= depthColor.xyz;
    alpha = clamp(depthColor.a, 0, 1);
    }
#endif
    float fresnel = Fresnel(-viewDir, normal, _FReflectionInitRatio);
    refration *= lerp(float3(1, 1, 1), _RefractionColor, alpha);

    if(isFront == false){
        return float4(refration * _UnderWaterRefractIntensity,  -1);
    }
    if (!all(reflection))
    {   
        result = refration;
        return float4(result,  1);
    }
    else
    {   
        result = lerp(refration * _RefractionIntensity, reflection * _ReflectionIntensity, clamp(fresnel, 0, 1));
        return float4(result, -1);
    }

#endif 
#if SSAOMOD
    float detailVert = GetDetailValue_Vert(i.oriWorldPos.xz).y;
    float depth = -TransformWorldToView(i.worldPos + detailVert * _SSAONormalDisturbanceIntensity).z;
    return float4(normal, depth);
#endif
#if RENDERINGMOD

    //return float4(normal , 1);
    float4 SHADOW_COORDS = TransformWorldToShadowCoord(i.worldPos.xyz);
    Light light = GetMainLight(SHADOW_COORDS);
    float3 viewDir = normalize(i.worldPos - _WorldSpaceCameraPos);
    float depth = length(i.worldPos - _WorldSpaceCameraPos);
    float4 resultCol = float4(0, 0, 0, 1);
    float2 uv = clipToScreen(TransformWorldToHClip(i.worldPos)).xy;
    float3 lightDir = _MainLightPosition.xyz;
    half originShadow = light.shadowAttenuation;
    half shadow = (1 - (1 - originShadow) * _ShadowIntensity);
     float oceanFoam;
     float waveFoam;
     float foam;
if(_Flow_On == 1){
     oceanFoam = lerp(max(GetPeakFoamValue(flowPos0, planePos.xz), 0), max(GetPeakFoamValue(flowPos1, planePos.xz), 0), lerpScale);
     waveFoam = lerp(GetFoam(flowPos2, planePos.xz), GetFoam(flowPos3, planePos.xz), lerpScale);
     foam = oceanFoam + waveFoam;
}else{
     oceanFoam = max(GetPeakFoamValue(i.oriWorldPos.xz, planePos.xz), 0);
     waveFoam = GetFoam(i.worldPos.xz, planePos.xz);
     foam = oceanFoam + waveFoam;
}

    
    oceanFoam = lerp(oceanFoam, 0, clamp(length(i.oriWorldPos.xyz - _WorldSpaceCameraPos.xyz) / _RenderFoamDistance, 0, 1));
    waveFoam = lerp(waveFoam, 0, clamp(length(i.oriWorldPos.xyz - _WorldSpaceCameraPos.xyz) / _RenderFoamDistance, 0, 1));
    oceanFoam *= i.waveData.x;
    float noAOShadow = shadow;

if(_SSAOEnable == 1){
    shadow *= tex2D(_SSAOTex, uv).x;
}
    float fresnel = Fresnel(-viewDir, normal, _FReflectionInitRatio);
    Lambert(lightDir, normal, _LightColor, _AmbientColor, _AmbientIntensity, _DiffuseIntensity, shadow, _WaterColor, _LambertFactor, resultCol);
if(_SSR_ON == 1){
    float4 SSRValue = tex2D(_SSRTex, uv);
    if(SSRValue.w > 0){
       float3 oriDir = reflect(viewDir, float3(0, 1, 0));
       float3 realDir = reflect(viewDir, normal);
       float3 reflCol = float3(0, 0, 0);
       reflCol = GetCubeRefl(lerp(oriDir, realDir, _SkyReflectRoughness), _SkyBox, _SkyColor, _SkyColorHDR, _ReflectionColor);
       SSRValue = float4(lerp(SSRValue.xyz * _RefractionIntensity, reflCol * _ReflectionIntensity * _ReflectionColor, clamp(fresnel, 0, 1)),0);
    }
    
    resultCol = lerp(resultCol, SSRValue * shadow, _ReflectionBlendRatio);
}
    CookTorranceSpecular(normal, -viewDir, lightDir, _DistortionIntensity_Refr, _Roughness, originShadow, _LightColor, _HighLightIntensity,_HighLightHDRMax, resultCol);


    
    //float peakMask = max(GetPeakValue(i.oriWorldPos.xz, planePos.xz), 0);
    SSS(resultCol, normal, -viewDir, lightDir, _SSColour, noAOShadow, _SSSIndensity, i.data.x, i.data.y, _SSSRenderDistance);
    float foamDetail = clamp(1 - tex2D(_FoamDetail, i.worldPos.xz * _FoamDetail_Scale).x, 0, 1);
    waveFoam = clamp(waveFoam * _FoamIntensity - foamDetail * 1.5, 0, 1);
    foam = oceanFoam + waveFoam;
    resultCol += foam * shadow;
if(_Aldobe_On == 1){
    float4 aldobe = GetAldobeMap(i.oriWorldPos.xz - planePos.xz);
    resultCol = lerp(resultCol, aldobe, aldobe.w);
}
if(_FogEnable==1){
    float fogFactor = pow(clamp(depth / _FogDistance, 0, 1), _FogPow);
    float3 _SkyColor = texCUBE(_SkyBox, viewDir).xyz;
    resultCol = lerp(resultCol, float4(_FogColor, 0), fogFactor);
        int addLightCount = GetAdditionalLightsCount();
    float3 viewAbs = viewDir;
    viewAbs.y = -abs(viewAbs.y);
    for (int lightIndex = 0; lightIndex < addLightCount; ++lightIndex)
    {
     Light addLight = GetAdditionalLight(lightIndex, i.worldPos);
     half3 addLightColAtten = addLight.color * addLight.distanceAttenuation;
     CookTorranceSpecular(normal, -viewAbs, addLight.direction, _DistortionIntensity_Refr, _PointLightRoughness, 1, addLightColAtten, _PointLightIntensity,_PointLightHDRMax, resultCol);
    }
}
    return resultCol;
#endif
}
#endif
            ENDHLSL
        }

    }
}
