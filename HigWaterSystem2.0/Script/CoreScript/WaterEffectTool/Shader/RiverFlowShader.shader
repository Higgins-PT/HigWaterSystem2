Shader"Unlit/RiverFlowShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MinSpeed("Min Speed", Float) = 1.0
        _MaxSpeed("Max Speed", Float) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
Cull Off

LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

#include "UnityCG.cginc"
float _MinSpeed;
float _MaxSpeed;

struct appdata
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
};

struct v2f
{
    float4 pos : SV_POSITION;
    float3 worldNormal : TEXCOORD0;
};

v2f vert(appdata v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
                // Transform the normal from object space to world space
    o.worldNormal = normalize(mul((float3x3) unity_WorldToObject, v.normal));
    return o;
}

float4 frag(v2f i) : SV_Target
{

    return float4(-normalize(i.worldNormal.xz) * lerp(_MinSpeed, _MaxSpeed, clamp(-i.worldNormal.y, 0, 1)), 0.0, 0.0);
}
            ENDCG
        }
    }
}
