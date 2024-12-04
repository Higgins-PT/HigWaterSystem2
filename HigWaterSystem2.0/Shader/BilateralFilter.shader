Shader"Unlit/BilateralFilter"
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
#include "UnityCG.cginc"
#include "Random.hlsl"
#include "WaterRendering/SSR.hlsl"
sampler2D _MainTex;
float2 _TexSize;
sampler2D _NormalDepthTex;
int _HorizontalBlur;
float _BlurWeights[12] =
{
    0.05f, 0.05f, 0.1f, 0.1f,
    0.1f, 0.2f, 0.1f, 0.1f,
    0.1f, 0.05f, 0.05f, 0.0f
};
int _BlurRadius = 5;
float _BlurNormalThreshold;
float _BlurDepthThreshold;
float _SSAORadiusMin;
float _SSAORadiusMax;

float _SSAODistance;

float _BlurRangeFactor;
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
float4 GetNormalDepth(float2 uv)
{
    return tex2D(_NormalDepthTex, uv);

}
v2f vert(appdata v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    return o;
}


float4 frag(v2f i) : SV_Target
{
    float2 texOffset;
    if (_HorizontalBlur == 1)
    {
        texOffset = float2(1.0f / _TexSize.x, 0.0f);
    }
    else
    {
        texOffset = float2(0.0f, 1.0f / _TexSize.y);
    }

    float4 color = _BlurWeights[_BlurRadius] * tex2D(_MainTex, i.uv);
    float totalWeight = _BlurWeights[_BlurRadius];
    
    float4 centerNormalDepth = GetNormalDepth(i.uv);

    float3 centerNormal = centerNormalDepth.xyz;
    float centerDepth = centerNormalDepth.w;
    float range = lerp(_SSAORadiusMin, _SSAORadiusMax, clamp(centerDepth / _SSAODistance, 0, 1)) * _BlurRangeFactor;
    for (float j = -_BlurRadius; j <= _BlurRadius; ++j)
    {
        if (j == 0)
            continue;
        
        float2 tex = i.uv + j * texOffset;
        
        float4 neighborNormalDepth = GetNormalDepth(tex);
        float3 neighborNormal = neighborNormalDepth.xyz;
        float neighborDepth = neighborNormalDepth.w;

        
        if (dot(neighborNormal, centerNormal) >= _BlurNormalThreshold && abs(neighborDepth - centerDepth) <= range)
        {
            float weight = _BlurWeights[j + _BlurRadius];
            
            color += weight * tex2D(_MainTex, tex);
            totalWeight += weight;
        }
        
    }

    return color / totalWeight;
}
            ENDCG
        }
    }
}
