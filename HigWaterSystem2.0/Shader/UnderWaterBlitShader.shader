Shader "Unlit/UnderWaterBlitShader"
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
sampler2D _BackWaterPlaneTemRT;
sampler2D _UnderWaterMaskTemRT;
sampler2D _DepthMaskTex;
float _OffestClip;
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
    float waterDepth = tex2D(_DepthMaskTex, i.uv);
    float backDepth = tex2D(_BackWaterPlaneTemRT, i.uv);
    float underWaterDepth = tex2D(_UnderWaterMaskTemRT, i.uv);
    float testMask = backDepth + underWaterDepth;
    if (testMask != 0)
    {
        if (backDepth > 0)
        {
            return float4(1, 1, 1, 1);
        }
        else
        {
            if (waterDepth <= 0)
            {
                return float4(1, 1, 1, 1);
            }
            if (underWaterDepth + _OffestClip > waterDepth)
            {
                return float4(0, 0, 0, 0);
            }
            else
            {
                
                return float4(1, 1, 1, 1);
            }

        }

    }

    return float4(0, 0, 0, 0);

}
            ENDCG
        }
    }

}
