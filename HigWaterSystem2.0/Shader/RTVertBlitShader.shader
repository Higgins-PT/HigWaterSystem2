Shader"Unlit/RTBlitVertShader"
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
            #pragma multi_compile __ RenderScaleEnable
#include "UnityCG.cginc"
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
sampler2D _MainTex;
float4 _MainTex_ST;

sampler2D Texture_1;
sampler2D Texture_2;
uint FirstSize;
uint SecSize;
float FirstWorldSize;
float SecWorldSize;
float2 WorldPosOffest;
float _AddScale;
float _RenderDistance;
float _CamHeight;
v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    return o;
}

float4 frag(v2f i) : SV_Target
{
    float2 index = i.uv;
    float4 color = tex2D(Texture_2, index);
    uint2 halfSize_1 = FirstSize / 2;
    uint2 halfSize_2 = SecSize / 2;
    float2 worldPos = SecWorldSize * (float2) ((float2) index - float2(0.5, 0.5));
    float scale = 1 - clamp(length(float3(worldPos, _CamHeight)) / _RenderDistance, 0, 1);
    worldPos += WorldPosOffest;
    float2 mapIndex = ((worldPos / FirstWorldSize + float2(0.5, 0.5)) % float2(1, 1) + float2(1, 1)) % float2(1, 1);
    float4 vert = tex2D(Texture_1, mapIndex);
#if RenderScaleEnable
    return color + vert * _AddScale * scale;
#else
    return color + vert * _AddScale;
#endif
}
            ENDCG
        }
    }
}
