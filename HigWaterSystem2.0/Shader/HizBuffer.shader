Shader"Unlit/HizBuffer"
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
int _Size;
float2 GetDepth(float2 uv, int2 offest)
{
    return tex2D(_LastMipMap, ((float) offest / (float) _Size) + uv);

}
float2 GetLimValue(float2 value_1, float2 value_2, float2 value_3, float2 value_4)
{
    float maxX = max(max(value_1.x, value_2.x), max(value_3.x, value_4.x));
    float minY = min(min(value_1.y, value_2.y), min(value_3.y, value_4.y));

    return float2(maxX, minY);
}

float4 frag(v2f i) : SV_Target
{
    float2 offestUV = i.uv + float2(-1, -1) / (float) _Size / 2;
    float4 col = float4(GetLimValue(GetDepth(offestUV, int2(0, 0)), GetDepth(offestUV, int2(1, 0)), GetDepth(offestUV, int2(0, 1)), GetDepth(offestUV, int2(1, 1))), 0, 0);
    return col;
}
            ENDCG
        }
    }
}
