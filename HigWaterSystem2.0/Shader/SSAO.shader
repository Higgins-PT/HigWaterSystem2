Shader"Unlit/SSAO"
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

#include "UnityCG.cginc"
#include "Random.hlsl"
#include "WaterRendering/SSR.hlsl"
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
float _SSAOShadowMin;
sampler2D _MainTex;
float4 _MainTex_ST;
sampler2D _NoiseTex;
sampler2D _NormalDepthTex;
sampler2D _RandomRT;
sampler3D _RandomDirTex;
float2 _NoiseScale;

float4x4 _ViewMatrix;
float4x4 _ProjectionMatrix;
float4x4 _InverViewMatrix;
float4x4 _InverProjectionMatrix;
float _NearPlane;
float _FarPlane;
float _SSAORadiusMin;
float _SSAORadiusMax;
float _SSAODistance;

float _SSAOIntensity;
float _Sigma;
int _SamplePointsCount;
float _SSAOIntensityFactor;

v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    return o;
}
int _Size;
float4 GetNormalDepth(float2 uv)
{
    return tex2D(_NormalDepthTex, uv);

}

float frag(v2f i) : SV_Target
{

    float4 initNormal = GetNormalDepth(i.uv);
    float3 initPos = UVToWorld(i.uv, initNormal.w, _InverViewMatrix, _ProjectionMatrix);
    float shadow = 0;

    float depthFactor = clamp(initNormal.w / _SSAODistance, 0, 1);
    float range = lerp(_SSAORadiusMin, _SSAORadiusMax, depthFactor);
    int samplePoints = _SamplePointsCount;
    float deltaShadow = 1 / (float) samplePoints;
    float3 uv = float3(i.uv / _NoiseScale, 0);
    UNITY_LOOP

    for (int j = 0; j < samplePoints; j++)
    {
        uv.z = ((float) j / ((float) samplePoints - 1));
        float3 randomPos = initPos + GenerateRandomPointInHemisphere(initNormal.xyz, range, _RandomDirTex, uv);
        float4 viewPos = mul(_ViewMatrix, float4(randomPos, 1.0));
        float4 clipPos = mul(_ProjectionMatrix, viewPos);
        float2 ndcPos = clipPos.xy / clipPos.w;
        ndcPos.x = (ndcPos.x + 1.0) * 0.5;
        ndcPos.y = (ndcPos.y + 1.0) * 0.5;

        float4 testNormal = GetNormalDepth(ndcPos);
        
        if (!((testNormal.w > -viewPos.z) && abs(testNormal.w - (-viewPos.z)) < range))
        {
            shadow += deltaShadow;

        }
        

    }
    float factor = _SSAOIntensityFactor;
    shadow = clamp(pow(shadow * factor, _SSAOIntensity) / factor + _SSAOShadowMin, 0, 1);
    return shadow;
}
            ENDCG
        }
    }
}
