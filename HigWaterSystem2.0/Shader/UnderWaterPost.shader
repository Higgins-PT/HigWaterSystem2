Shader"Unlit/UnderWaterPost"
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
            #pragma multi_compile_fragment __ INVERT
#include "UnityCG.cginc"

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
struct Ray
{
    float3 origin;
    float3 direction;
};


Ray ScreenToRay(float2 uv, float4x4 inverseProjectionMatrix, float4x4 inverseViewMatrix)
{

    float3 ndc;
    ndc.xy = uv.xy * 2.0 - float2(1, 1);
    ndc.z = 1.0;

    float4 clipSpacePos = float4(ndc.xy, -1.0, 1.0);


    float4 viewSpacePos = mul(inverseProjectionMatrix, clipSpacePos);
    viewSpacePos /= viewSpacePos.w;
    

    float4 worldSpacePos = mul(inverseViewMatrix, viewSpacePos);


    float3 rayOrigin = mul(inverseViewMatrix, float4(0, 0, 0, 1)).xyz;
    float3 rayDirection = normalize(worldSpacePos.xyz - rayOrigin);


    Ray result;
    result.origin = rayOrigin;
    result.direction = rayDirection;
    return result;
}
sampler2D _MainTex;
float4 _MainTex_ST;
sampler2D _WaterMask;
sampler2D _DepthMask;
sampler2D _GrabPass;
float4 _UnderWaterColor;    
float _UnderWaterViewDistance;
float _UnderWaterPow;
float4x4 _InverseProjectionMatrix;
float4x4 _InverseViewMatrix;
float3 _MainLightPosition;
float _DepthColorDistance;
float _DepthColorFactor;

float _UnderWaterLightPow;
float _UnderWaterLightIntensity;
float _UnderWaterLightBasic;
float3 _UnderWaterLightColor;
float _AbsorptivityOfWater;

float _SeabedLightBasic;
float _SeabedLightPow;
float _CameraDepth;
float4 _DepthColor[100];
float3 RefractDepthColor(float depth)
{
    if (depth < 0)
    {
        return float3(1, 1, 1);

    }
    int index = clamp(round(depth / _DepthColorDistance * 100), 0, 99);
    return _DepthColor[index].xyz;

}

v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    return o;
}

float4 frag(v2f i) : SV_Target
{
#ifdef INVERT
    float4 col = tex2D(_GrabPass, float2(i.uv.x, 1 - i.uv.y));
#else
    float4 col = tex2D(_GrabPass, float2(i.uv.x, i.uv.y));
    #endif
    float mask = tex2D(_WaterMask, i.uv).xy;
    float2 depth = tex2D(_DepthMask, i.uv).xy;
    if (mask == 1)
    {
        float3 lightDir = _MainLightPosition.xyz;
        Ray ray = ScreenToRay(i.uv, _InverseProjectionMatrix, _InverseViewMatrix);
        float3 underWaterColor = _UnderWaterColor.xyz;
        float lightFactor = dot(ray.direction, lightDir);
        lightFactor = pow(clamp(lightFactor, 0, 1), _UnderWaterLightPow) * _UnderWaterLightIntensity + _UnderWaterLightBasic;
        float depthWaterColor = pow(1 - clamp(-ray.direction.y, 0, 1), _SeabedLightPow) + _SeabedLightBasic;

        underWaterColor = _UnderWaterLightColor * lightFactor + underWaterColor * depthWaterColor * _DepthColorFactor;
        if (depth.y == 1)
        {
            col = lerp(col, float4(underWaterColor, 0), clamp(pow(depth.x / _UnderWaterViewDistance, _UnderWaterPow), 0, 1));
            col *= 1 - clamp(_CameraDepth * _AbsorptivityOfWater, 0, 1);
            col *= float4(RefractDepthColor(_DepthColorDistance), 0);
        }
        else
        {
            col = lerp(col, float4(underWaterColor, 0), clamp(pow(depth.x / _UnderWaterViewDistance, _UnderWaterPow), 0, 1));
            col *= 1 - clamp(_CameraDepth * _AbsorptivityOfWater, 0, 1);
            col *= float4(RefractDepthColor(depth.x), 0);
        }


    }

    return col;
}
            ENDCG
        }
    }
}
