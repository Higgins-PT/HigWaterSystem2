
#ifndef BASICLIGHTING
#define BASICLIGHTING

void Lambert(float3 lightDir, float3 normal, float3 lightColor,
float3 ambientColor, float3 ambientIntensity, float diffuseIntensity, float shadow, float3 WaterColor, float lambertFactor, inout float4 col)
{
    float diff = max(0, dot(normal, lightDir)) * shadow;
    float fractionalLambert = diff * (1.0 / lambertFactor) + (1 - lambertFactor);
    fractionalLambert = saturate(fractionalLambert);
    float3 diffuse = lightColor.rgb * fractionalLambert * diffuseIntensity;
    float3 ambient = ambientColor.rgb * ambientIntensity;
    col.rgb += (ambient + diffuse) * WaterColor;

}
void HighLight(float3 lightDir, float3 normal, float3 viewDir, float shiniess, float highLightIntensity, float originShadow, float3 _LightColor, inout float4 col)
{
    viewDir = -viewDir;
    float3 halfDir = normalize(lightDir + viewDir);
    float3 reflectDir = normalize(reflect(-lightDir, normal));
    float spec = pow(max(dot(reflectDir, halfDir), 0.0), shiniess);
    col += float4(_LightColor * spec * highLightIntensity * originShadow,0);

}

float ReflectionScale(float3 viewDir, float3 normal,float offest)
{
    float3 firstReflectDir = reflect(viewDir, normal);
    return clamp(firstReflectDir.y - offest, -1, 0) + 1 + offest;

}

float DistributionGGX(float3 N, float3 H, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH * NdotH;

    float num = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = 3.14159 * denom * denom;

    return num / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;

    float num = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return num / denom;
}
float GeometrySmith(float3 N, float3 V, float3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

float FresnelSchlick(float cosTheta, float F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

void CookTorranceSpecular(float3 N, float3 V, float3 L, float F0, float roughness, float originShadow, float3 _LightColor, float highLightIntensity, float highLightHDRMax, inout float4 col)
{
    float3 H = normalize(V + L);
    float F = FresnelSchlick(max(dot(H, V), 0.0), F0);
    float D = DistributionGGX(N, H, roughness);
    float G = GeometrySmith(N, V, L, roughness);
    float3 numerator = D * F * G;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.001;
    float3 specular = numerator / denominator;

    col.xyz += clamp(specular * originShadow * _LightColor * highLightIntensity, float3(0, 0, 0), float3(1, 1, 1) * highLightHDRMax);
}



#endif
