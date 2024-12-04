Shader"Unlit/RiverHeightShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
Cull Off
Lod 100
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
    float3 normal : NORMAL;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
    float4 worldPos : TEXCOORD1;
};

sampler2D _MainTex;
float4 _MainTex_ST;
float _Weight;
float _OceanBasicHeight;
v2f vert(appdata v)
{
    v2f o;
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    return o;
}

float4 frag(v2f i) : SV_Target
{
    float4 col = float4(0, 0, 0, 0);
    
    col.a = i.worldPos.y - _OceanBasicHeight;
    return col;
}
            ENDCG
        }
    }
}
