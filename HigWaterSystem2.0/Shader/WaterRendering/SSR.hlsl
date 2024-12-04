
#ifndef SSR
#define SSR



float4 WorldToClip(float3 worldPos, float4x4 viewMatrix, float4x4 projectionMatrix)
{
    float4 viewPos = mul(viewMatrix, float4(worldPos, 1.0));
    return mul(projectionMatrix, viewPos);
}
float2 ClipToScreen(float4 clipPos, float2 screenSize)
{
    float2 ndcPos = clipPos.xy / clipPos.w;
    float2 screenPos;
    screenPos.x = (ndcPos.x + 1.0) * 0.5 * screenSize.x;
    screenPos.y = (ndcPos.y + 1.0) * 0.5 * screenSize.y;

    return screenPos;
}

float2 WorldToUV(float3 worldPos, float4x4 viewMatrix, float4x4 projectionMatrix)
{
    float4 clipPos = WorldToClip(worldPos, viewMatrix, projectionMatrix);
    float2 ndcPos = clipPos.xy / clipPos.w;
    ndcPos.x = (ndcPos.x + 1.0) * 0.5;
    ndcPos.y = (ndcPos.y + 1.0) * 0.5;
    return ndcPos;

}

float4 ScreenToClip(float2 screenPos, float2 screenSize, float depth)
{
    float2 ndcPos = (screenPos / screenSize) * 2.0 - 1.0;
    return float4(ndcPos, depth, 1.0);
}
float3 ClipToWorld(float4 clipPos, float4x4 inverseView, float4x4 inverseProjection)
{
    float4 viewPos = mul(inverseProjection, clipPos);

    float4 worldPos = mul(inverseView, viewPos);
    return worldPos.xyz;
}
float3 UVToWorld(float2 uv, float depth, float4x4 inverseView, float4x4 Projection)
{
    float2 p11_22 = float2(Projection._11, Projection._22);
    float3 vpos = float3((uv * 2 - 1) / p11_22, -1) * depth;
    float4 wposVP = mul(inverseView, float4(vpos, 1));
    return wposVP.xyz;
}

float2 GetNearestGridIntersection(float2 uv, float2 direction, int n)
{
    float gridSize = 1.0 / (float) n;

    float2 step = sign(direction);

    float2 posInGrid = uv / gridSize;

    float2 nextGridLineDist;
    nextGridLineDist.x = step.x > 0 ? (ceil(posInGrid.x) - posInGrid.x) : (posInGrid.x - floor(posInGrid.x));
    nextGridLineDist.y = step.y > 0 ? (ceil(posInGrid.y) - posInGrid.y) : (posInGrid.y - floor(posInGrid.y));
    
    float2 tMax = abs(nextGridLineDist / direction);

    float deltaScale = min(tMax.x, tMax.y);
    
    return uv + direction * gridSize * deltaScale;
}


inline float GetDepth(float2 uv, int mipLevel, sampler2D hizBuffer)
{
    return tex2Dlod(hizBuffer, float4(uv, 0, mipLevel)).y;

}


struct Ray
{
    float3 origin;
    float3 direction;
};


inline Ray ScreenToRay(float2 screenPos, float2 screneSize, float4x4 inverseProjectionMatrix, float4x4 inverseViewMatrix)
{

    float3 ndc;
    ndc.x = (screenPos.x / screneSize.x) * 2.0 - 1.0;
    ndc.y = (screenPos.y / screneSize.y) * 2.0 - 1.0; 
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

inline float3 FindRayIntersection(float3 P1, float3 D1, float3 P2, float3 D2)
{
    float3 crossD1D2 = cross(D1, D2);
    float3 P2P1 = P2 - P1;
    float3 crossP2P1D2 = cross(P2P1, D2);
    float t = dot(crossP2P1D2, crossD1D2) / dot(crossD1D2, crossD1D2);
    float3 intersection = P1 + t * D1;
    
    return intersection;
}

inline bool CaculateNextStep(inout float3 nowWorldPos, inout float2 nowScenePos, float3 nowWorldDir,
float2 nowSceneDir, int mip0Size, int size, float3 startPos, float3 camPos, float4x4 inverseProjectionMatrix, float4x4 inverseViewMatrix)
{

    float2 nextScenePos = GetNearestGridIntersection(nowScenePos / (float) mip0Size, normalize(nowSceneDir), size) * (float) mip0Size;

    Ray ray = ScreenToRay(nextScenePos, mip0Size, inverseProjectionMatrix, inverseViewMatrix);
    float3 hitPos = FindRayIntersection(camPos, ray.direction, startPos, nowWorldDir);
    nowScenePos = nextScenePos + normalize(nowSceneDir) / 100;
    float3 lastPos = nowWorldPos;
    nowWorldPos = hitPos;
    return dot(hitPos - lastPos, nowWorldDir) > 0;

}
float GetEyeDepthFromWorldPosition(float3 worldPos, float4x4 viewMatrix)
{
    float4 viewPos = mul(viewMatrix, float4(worldPos, 1.0));
    float eyeDepth = -viewPos.z; 

    return eyeDepth;
}
inline bool IsScenePosInScreen(float2 scenePos, float2 screenSize)
{
    bool isInside = (scenePos.x >= 0.0 && scenePos.x <= screenSize.x) &&
                    (scenePos.y >= 0.0 && scenePos.y <= screenSize.y);
    return isInside;
}
float3 GetCubeRefl(float3 startDir, samplerCUBE skyBox, float3 waterColor, float skyColorHDR, float3 reflectionColor)
{
    if (startDir.y < 0)
    {
        startDir = normalize(startDir);
        float factor = sqrt(abs(startDir.y));
        return lerp(texCUBE(skyBox, startDir).xyz, waterColor / reflectionColor, factor) * skyColorHDR;

    }
    else
    {
        startDir = normalize(startDir);
        return texCUBE(skyBox, startDir).xyz;
    }

}
inline bool RayOutOfRange(float2 scenePos, float2 screneSize, float depth, float depthMax)
{
    return !IsScenePosInScreen(scenePos, screneSize) || depth > depthMax;

}
/*
#define PROCESS_NEXT_STEP(nowWorldPos, nowScenePos, startDir, nowSceneDir, size, mipNowSize, startPos, camPos, inverProjectionMatrix, inverViewMatrix, mipLevel, lastScenePos, lastWorldPos, depth, lastDepth, screenSize, depthMax) \
    if (!CaculateNextStep(nowWorldPos, nowScenePos, startDir, nowSceneDir, size, mipNowSize, startPos, camPos, inverProjectionMatrix, inverViewMatrix)) { \
        if (mipLevel > 1) { \
            mipLevel -= 2; \
            nowScenePos = lastScenePos; \
            nowWorldPos = lastWorldPos; \
            continue; \
        } else { \
            break; \
        } \
    } else { \
        lastDepth = depth; \
        if (RayOutOfRange(nowScenePos, screenSize, depth, depthMax)) { \
            if (mipLevel > 1) { \
                mipLevel -= 2; \
                nowScenePos = lastScenePos; \
                nowWorldPos = lastWorldPos; \
                continue; \
            } else { \
                break; \
            } \
        } \
    }

*/
float3 RayTrace_Refl(float3 startPos, float3 startDir, float3 camPos, float2 screneSize, int size, int mipMapCount,
int stepCount, float4x4 viewMatrix, float4x4 projectionMatrix, float4x4 inverViewMatrix, float4x4 inverProjectionMatrix,
sampler2D hizBuffer, sampler2D screenTex, samplerCUBE skyBox, float thickness, float depthMax, float SSRIntensity, float minHeight, float3 SSRColor)
{
    float4 nowClipPos = WorldToClip(startPos, viewMatrix, projectionMatrix);
    float2 nowScenePos = ClipToScreen(nowClipPos, screneSize);
    float2 nowSceneDir = ClipToScreen(WorldToClip((startDir * 100) + startPos, viewMatrix, projectionMatrix), screneSize) - ClipToScreen(WorldToClip(startPos, viewMatrix, projectionMatrix), screneSize);
    float3 nowWorldPos = startPos;
    if (abs(nowSceneDir.x) < 0.01 && abs(nowSceneDir.y) < 0.01)
    {
        return tex2D(screenTex, nowScenePos / screneSize).xyz * SSRIntensity;
    }
    else
    {
        nowSceneDir = normalize(nowSceneDir);
        int mipLevel = 0;
        float lastDepth = GetEyeDepthFromWorldPosition(nowWorldPos, viewMatrix);
        float2 lastScenePos = nowScenePos;
        float3 lastWorldPos = nowWorldPos;
        UNITY_LOOP

        for (int i = 0; i < stepCount; i++)
        {
            int mipNowSize = size >> mipLevel;

            float depthTex = GetDepth(nowScenePos / screneSize, mipLevel, hizBuffer);
            float depth = GetEyeDepthFromWorldPosition(nowWorldPos, viewMatrix);
            float depthDiff = depthTex - depth;
            float lastDepthDiff = depthTex - lastDepth;
            if (depthDiff < 0)
            {
                if (mipLevel > 0)// if clip depth smaller then hiz pos ----- back mip level
                {
                    mipLevel -= 1;
                    nowScenePos = lastScenePos;
                    nowWorldPos = lastWorldPos;
                    lastDepth = depth;

                }
                else
                {
                    lastScenePos = nowScenePos;
                    lastWorldPos = nowWorldPos;
                    if (abs(depthDiff) < thickness || lastDepthDiff > 0)
                    {


                        return tex2Dlod(screenTex, float4(nowScenePos / screneSize, 0, 0)).xyz * SSRIntensity * SSRColor;
                    }
                    else
                    {

                    }
                }

            }
            else
            {
                lastScenePos = nowScenePos;
                lastWorldPos = nowWorldPos;
                if (mipLevel < mipMapCount - 1)
                {
                    mipLevel += 1;
                }
            }
            if (!CaculateNextStep(nowWorldPos, nowScenePos, startDir, nowSceneDir, size, mipNowSize, startPos, camPos, inverProjectionMatrix, inverViewMatrix))
            {

                if (mipLevel > 1)
                {
                    mipLevel -= 2;
                    nowScenePos = lastScenePos;
                    nowWorldPos = lastWorldPos;
                    continue;
                }
                else
                {
                    break;
                }
            }
            else
            {
                lastDepth = depth;
                if (RayOutOfRange(nowScenePos, screneSize, depth, depthMax))
                {

                    if (mipLevel > 1)
                    {
                        mipLevel -= 2;
                        nowScenePos = lastScenePos;
                        nowWorldPos = lastWorldPos;
                        continue;
                    }
                    else
                    {
                        break;
                    }

                }
            }

        }

    
        return float3(0, 0, 0);
    }



}

float3 RayTrace_Refr(float3 startPos, float3 startDir, float3 camPos, float2 screneSize, int size, int mipMapCount,
int stepCount, float4x4 viewMatrix, float4x4 projectionMatrix, float4x4 inverViewMatrix, float4x4 inverProjectionMatrix,
sampler2D hizBuffer, sampler2D screenTex, samplerCUBE skyBox, float thickness, float depthMax, float SSRIntensity, inout float refractDepth, float3 SSRColor)
{

    float4 nowClipPos = WorldToClip(startPos, viewMatrix, projectionMatrix);
    float2 nowScenePos = ClipToScreen(nowClipPos, screneSize);
    float2 nowSceneDir = ClipToScreen(WorldToClip((startDir * 100) + startPos, viewMatrix, projectionMatrix), screneSize) - ClipToScreen(WorldToClip(startPos, viewMatrix, projectionMatrix), screneSize);
    float3 nowWorldPos = startPos;
    float startDepth = GetEyeDepthFromWorldPosition(nowWorldPos, viewMatrix);
    if (abs(nowSceneDir.x) < 0.01 && abs(nowSceneDir.y) < 0.01)
    {
        float depthTex = GetDepth(nowScenePos / screneSize, 0, hizBuffer);
        refractDepth = depthTex - startDepth;
        return tex2D(screenTex, nowScenePos / screneSize).xyz * SSRIntensity;
    }
    else
    {
        float2 startScenePos = nowScenePos;
        nowSceneDir = normalize(nowSceneDir);
        int mipLevel = 0;
        float lastDepth = GetEyeDepthFromWorldPosition(nowWorldPos, viewMatrix);

        float2 lastScenePos = nowScenePos;
        float3 lastWorldPos = nowWorldPos;
        
        UNITY_LOOP

        for (int i = 0; i < stepCount; i++)
        {
            int mipNowSize = size >> mipLevel;

            float depthTex = GetDepth(nowScenePos / screneSize, mipLevel, hizBuffer);
            float depth = GetEyeDepthFromWorldPosition(nowWorldPos, viewMatrix);
            float depthDiff = depthTex - depth;
            float lastDepthDiff = depthTex - lastDepth;
            if (depthDiff < 0)
            {
                if (mipLevel > 0)// if clip depth smaller then hiz pos ----- back mip level
                {
                    mipLevel -= 1;
                    nowScenePos = lastScenePos;
                    nowWorldPos = lastWorldPos;
                    lastDepth = depth;

                }
                else
                {
                    lastScenePos = nowScenePos;
                    lastWorldPos = nowWorldPos;
                    if (abs(depthDiff) < thickness || lastDepthDiff > 0)
                    {

                        refractDepth = depthTex - startDepth;

                        return tex2Dlod(screenTex, float4(nowScenePos / screneSize, 0, 0)).xyz * SSRIntensity * SSRColor;
                    }
                    else
                    {

                    }
                }

            }
            else
            {
                lastScenePos = nowScenePos;
                lastWorldPos = nowWorldPos;
                if (mipLevel < mipMapCount - 1)
                {
                    mipLevel += 1;
                }
            }
            if (!CaculateNextStep(nowWorldPos, nowScenePos, startDir, nowSceneDir, size, mipNowSize, startPos, camPos, inverProjectionMatrix, inverViewMatrix))
            {

                if (mipLevel > 1)
                {
                    mipLevel -= 2;
                    nowScenePos = lastScenePos;
                    nowWorldPos = lastWorldPos;
                    continue;
                }
                else
                {
                    break;
                }
            }
            else
            {
                lastDepth = depth;
                if (RayOutOfRange(nowScenePos, screneSize, depth, depthMax))
                {

                    if (mipLevel > 1)
                    {
                        mipLevel -= 2;
                        nowScenePos = lastScenePos;
                        nowWorldPos = lastWorldPos;
                        continue;
                    }
                    else
                    {
                        break;
                    }

                }
            }


        }

        refractDepth = -1;
        return texCUBE(skyBox, startDir).xyz;
    }


}
float3 CalculateMicrofacetReflection(float oriDir, float realDir, float roughness)
{
    float dotProduct = dot(oriDir, realDir);
    float angle = acos(dotProduct);
    float angle01 = clamp(angle / 3.1415, 0, 1);
    return lerp(oriDir, realDir, pow(angle01, roughness));

}
float Random(float seed)
{
    return frac(sin(seed) * 43758.5453);
}


float3 GenerateRandomDirectionInCone(float3 coneCenter, float maxAngleRad, float seed)
{

    float randomAngle = acos(lerp(cos(maxAngleRad), 1.0, Random(seed)));
    float randomRotation = Random(seed + 1.0) * 2.0 * 3.14159265359;


    float sinAngle = sin(randomAngle);
    float3 randomDir = float3(
        cos(randomRotation) * sinAngle,
        sin(randomRotation) * sinAngle,
        cos(randomAngle)
    );
    

    float3 tangent = normalize(coneCenter.yzx - dot(coneCenter, float3(0, 0, 1)) * float3(0, 0, 1)); 
    float3 bitangent = cross(coneCenter, tangent); 

    float3 rotatedDir = randomDir.x * tangent + randomDir.y * bitangent + randomDir.z * coneCenter;

    return normalize(rotatedDir);
}
#endif
