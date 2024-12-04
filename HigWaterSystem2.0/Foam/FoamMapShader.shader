Shader"Unlit/FoamMapShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_Color("Color",Color)=(1,1,1,1) 
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
LOD 100

        Pass
        {
Blend SrcAlpha
OneMinusSrcAlpha
ZWrite
Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float4 color : COLOR;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
    float4 color : COLOR;
};

sampler2D _MainTex;
float4 _MainTex_ST;
float4 _Color;
v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.color = v.color;
    return o;
}

float4 frag(v2f i) : SV_Target
{
                // sample the texture
    float4 col = tex2D(_MainTex, i.uv);
    col.w = (col.r + col.g + col.b) / 3;
    col *= _Color * i.color;

    return col;
}
            ENDCG
        }
    }
}
