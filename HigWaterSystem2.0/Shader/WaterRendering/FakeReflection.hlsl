
#ifndef FAKEREFLECTION
#define FAKEREFLECTION


float RresnelSchlick(float cosTheta, float F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

float Fresnel(float3 viewDir, float3 normal, float fReflectionInitRatio)
{
    float cosTheta = saturate(dot(normal, viewDir)); 
    return RresnelSchlick(cosTheta, fReflectionInitRatio);
}
float4 WorldToClipFa(float3 worldPos, float4x4 viewMatrix, float4x4 projectionMatrix)
{
    float4 viewPos = mul(viewMatrix, float4(worldPos, 1.0));
    return mul(projectionMatrix, viewPos);
}
float2 ClipToScreenFa(float4 clipPos, float2 screenSize)
{
    float2 ndcPos = clipPos.xy / clipPos.w;
    float2 screenPos;
    screenPos.x = (ndcPos.x + 1.0) * 0.5 * screenSize.x;
    screenPos.y = (ndcPos.y + 1.0) * 0.5 * screenSize.y;

    return screenPos;
}

float3 FakeRayTrace(sampler2D screenTex, float3 startPos, float3 startDir, float3 camPos, float2 screneSize, float4x4 viewMatrix, float4x4 projectionMatrix, float distortionIntensity)
{
    float4 nowClipPos = WorldToClipFa(startPos, viewMatrix, projectionMatrix);
    float2 nowScenePos = ClipToScreenFa(nowClipPos, screneSize);
    float2 nowSceneDir = ClipToScreenFa(WorldToClipFa((startDir * 1) + startPos, viewMatrix, projectionMatrix), screneSize) - ClipToScreenFa(WorldToClipFa(startPos, viewMatrix, projectionMatrix), screneSize);
    nowScenePos += nowSceneDir * distortionIntensity;
    return tex2D(screenTex, nowScenePos / screneSize).xyz;

}

#endif
