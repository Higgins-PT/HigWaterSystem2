
#ifndef WATERRENDERING
#define WATERRENDERING
float3 GetGradient(float3 colors[100], float t)
{
    return colors[(uint) round(clamp(t * 100, 0, 99))];

}
void DepthSSS(inout float3 col, float3 _SSDepthColor[100], float depth, float _DepthMax)
{
    col.xyz *= GetGradient(_SSDepthColor, depth / _DepthMax);
}
/*
void SSS(inout float4 colour, float3 vDir, float3 lightDir, float SSSunFallOff, float SSbase, float SSSun, float3 SSColour, float3 lightColour, float shadow, float SSSIndensity, float peak, float fresnel)
{
    float v = abs(vDir.y);
    float towardsSun = pow(max(0, dot(lightDir, -vDir)), SSSunFallOff);
    float3 subSurface = (SSbase + SSSun * towardsSun) * SSColour * lightColour * shadow;
    subSurface *= (1.0 - v * v) * SSSIndensity;
    //colour.xyz += subSurface ;
    colour.xyz += subSurface * SSSIndensity + SSColour * SSSIndensity * pow(peak, 2) * (1 - fresnel);
}*/
void SSS(inout float4 colour, float3 normal, float3 vDir, float3 lightDir, float3 SSColour, float shadow, float SSSIndensity, float peak, float distance, float renderRange)
{
    float3 H = normalize(-normal + lightDir);
    float vH = pow(saturate(dot(vDir, -H)), 2)  * SSSIndensity;
    
    float3 SSSColour = saturate(SSColour.rgb * vH * peak * shadow);
    colour.xyz += SSSColour * clamp((1 - distance / renderRange), 0, 1);
}
float LinearEyeDepth(float depth, float zNear, float zFar)
{
    float zLinear = zNear * zFar / (zFar + depth * (zNear - zFar));
    return zLinear;
}
#endif
