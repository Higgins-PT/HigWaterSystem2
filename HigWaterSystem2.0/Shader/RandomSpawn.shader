Shader"Unlit/RandomSpawn"
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
#include "Random.hlsl"
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
float _Sigma;
v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

    return o;
}
sampler2D _NoiseTex;
float3 GetNoise(float2 uv)
{
    uv = frac(uv);
    return tex2D(_NoiseTex, uv).xyz + float3(0, uv);

}

float4 frag(v2f i) : SV_Target
{
    float3 noise = GetNoise(i.uv).xyz;
    noise.z = noise.x + noise.y;
    return float4(1,1,1,1);
}
            ENDCG
        }
    }
}
