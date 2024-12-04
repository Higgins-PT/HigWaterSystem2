Shader"Unlit/Albedo"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Height ("Height",  Range(-2, 2)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }



        Pass
        {
Blend One One
ZWrite
Off
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
float _Height;


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
    float4 col = tex2D(_MainTex, i.uv);
    col.w *= _Height;
    return col;
}
            ENDCG
        }
    }
}
