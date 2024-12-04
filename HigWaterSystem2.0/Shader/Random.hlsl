// MyShaderFunctions.cginc
#ifndef RANDOM
#define RANDOM

inline float Random(float3 seed)
{
    return frac(sin(dot(seed, float3(12.9898, 78.233, 45.164))) * 43758.5453);
}
float3 GenerateNormalDistribution(float3 seed, float sigma)
{
    float u1 = max(Random(seed), 1e-4); 
    float u2 = saturate(Random(seed + 1.0));
    float u3 = saturate(Random(seed + 2.0)); 


    float r = sqrt(-2.0 * log(u1)) * sigma;
    float theta = 2.0 * 3.14159265359 * u2;
    float phi = acos(2.0 * u3 - 1.0);

    float3 pointE;
    pointE.x = r * sin(phi) * cos(theta);
    
    pointE.y = r * sin(phi) * sin(theta);
    
    pointE.z = r * cos(phi);
    

    return pointE;
}

inline float3 GenerateRandomPointInSphere(float radius, sampler3D randomTex, float3 uv)
{
    float3 pointE = tex3Dlod(randomTex, float4(uv, 0)).xyz * radius;
    float distance = length(pointE);

    float clampedDistance = clamp(distance, 0.0, radius);
    pointE *= clampedDistance / distance;

    return pointE;
}

float3 GenerateRandomPointInHemisphere(float3 normal, float radius, sampler3D randomTex, float3 uv)
{

    float3 pointE = GenerateRandomPointInSphere(radius, randomTex, uv);

    pointE = sign(dot(pointE, normal)) * pointE;
    return pointE;
}
/*
float3 GenerateRandomPointInHemisphere(float3 normal, float2 uv, sampler2D randomTex)
{

    float3 pointE = tex2D(randomTex, uv);

    pointE = sign(dot(pointE, normal)) * pointE;
    return pointE;
}*/
#endif
