Shader"Custom/GaussianBlur"
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

sampler2D _MainTex;
float _BlurStrength;
float4 _MainTex_TexelSize;
float _GaussianKernel[9];
struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
};

v2f vert(appdata v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    return o;
}


float4 frag(v2f i) : SV_Target
{
    float2 texelSize = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);
    float2 texelOffset = _BlurStrength * _MainTex_TexelSize.xy;
    float4 color = tex2D(_MainTex, i.uv) * 0.2270270270;
    int index = 0;
    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float2 offset = float2(x, y) * texelSize;
            color += tex2D(_MainTex, i.uv + offset) * _GaussianKernel[index++];
        }
    }

    return color;
}
            ENDCG
        }
    }
}
