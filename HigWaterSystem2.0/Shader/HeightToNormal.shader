Shader"Unlit/HeightToNormal"
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
sampler2D _LastMipMap;

v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    return o;
}
float texSize;
float cellSize;

float4 frag(v2f i) : SV_Target
{
    float height_c = tex2D(_MainTex, i.uv).w;
    float height_d = tex2D(_MainTex, i.uv + float2(0, cellSize)).w;
    float height_r = tex2D(_MainTex, i.uv + float2(cellSize, 0)).w;
    float deltaSize = cellSize * texSize;
    float3 normal = cross(float3(deltaSize, height_r - height_c, 0), float3(0, height_d - height_c, deltaSize));
    normal.y = -normal.y;
    normal = normalize(normal);
    return float4(normal, height_c);
}
            ENDCG
        }
    }
}
