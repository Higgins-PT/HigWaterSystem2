Shader"Unlit/NearshoreWave"
{    Properties
    {
        _WaveAmplitude("Wave Amplitude", Float) = 1.0
        _WaveFrequency("Wave Frequency", Float) = 1.0
        _WaveSpeed("Wave Speed", Float) = 1.0
        _WaveGamma("Wave Gamma", Float) = 1.0
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

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
    float4 worldPos : TEXCOORD1;
    float3 localPos : TEXCOORD2;
};

sampler2D _MainTex;
float4 _MainTex_ST;
float _WaveAmplitude;
float _WaveFrequency;
float _WaveSpeed;
float _WaveGamma;
v2f vert(appdata v)
{
    v2f o;
    float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
    o.localPos = mul(unity_WorldToObject, worldPos).xyz;

    o.worldPos = worldPos;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    return o;
}


float4 frag(v2f i) : SV_Target
{
    float x = i.localPos.x + 1;
    float t = _Time.y * _WaveSpeed;
    float gamma = pow(2.0 * UNITY_PI * _WaveFrequency * x, _WaveGamma);
    float y = cos(gamma - t);

    float alpha = saturate(y) * _WaveAmplitude;

    return fixed4(0, 0.5, 1, alpha);
}
            ENDCG
        }
    }
}
