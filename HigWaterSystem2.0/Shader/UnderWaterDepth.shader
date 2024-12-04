Shader "Unlit/UnderWaterDepth"
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
sampler2D _CameraDepthTexture;
float4 _Zbuffer;
v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    return o;
}

float4 frag(v2f i) : SV_Target
{
    
    float depthValue = tex2D(_CameraDepthTexture, i.uv);
    float color = tex2D(_MainTex, i.uv);
    if (color == 0)
    {
        color = 1000000;

    }
    float4 col = float4(0, 0, 0, 0);
    if (LinearEyeDepth(depthValue) > color)
    {
        col = float4(color, 1, 0, 0);

    }
    else
    {
        col = float4(LinearEyeDepth(depthValue), 0, 0, 0);
    }
        

    return col;
}
            ENDCG
        }
    }
}
