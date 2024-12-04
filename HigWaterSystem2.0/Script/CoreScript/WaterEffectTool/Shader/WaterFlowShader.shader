Shader "Unlit/WaterFlowShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
                _GlobalFactor ("_GlobalFactor", Float) = 1
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
float _GlobalFactor;
v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

    return o;
}

float4 frag(v2f i) : SV_Target
{
    float4 col = tex2D(_MainTex, i.uv);
    col.xy = clamp(col.xy * 2 - 1, -1, 1);
    col.zw = float2(0, 1);
    return col * _GlobalFactor;
}
            ENDCG
        }
    }
}
