Shader"Unlit/FogEffect"
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
float _Width;
float _WidthPow;
float _DepthPow;
float4 _FogColor;
float _MaxFogDepth;
float _MinFogDepth;

sampler2D _CameraDepthTexture;
sampler2D _WaterDepthTexture;
float _FarClipPlane;

float4x4 _InverViewMat;
float4x4 _InverProjMat;
struct Ray
{
    float3 origin;
    float3 direction;
};

float Map(float input, float inputMin, float inputMax, float outputMin, float outputMax)
{

    return (input - inputMin) / (inputMax - inputMin) * (outputMax - outputMin) + outputMin;
}   
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
v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    return o;
}

float4 frag(v2f i) : SV_Target
{
                // sample the texture
    //tex2D(_WaterDepthTexture, i.uv).w
    float waterDepth = tex2D(_WaterDepthTexture, i.uv).w;
    float depth_Cam = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv));
    if (waterDepth == 0)
    {
        waterDepth = _MaxFogDepth;

    }

    float depth = min(waterDepth, depth_Cam);
    float depthFactor = clamp(pow(Map(clamp(depth, _MinFogDepth, _MaxFogDepth), _MinFogDepth, _MaxFogDepth, 0, 1), _DepthPow), 0, 1);
    float3 dir = normalize(ScreenToRay(i.uv, _InverProjMat, _InverViewMat).direction);
    float dirFactor = pow((1 - Map(clamp(abs(dir.y), 0, _Width), 0, _Width, 0, 1)), _WidthPow);
    float4 col = tex2D(_MainTex, i.uv);
    col = lerp(col, _FogColor, min(depthFactor, dirFactor));
    return col;
}
            ENDCG
        }
    }
}
