using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
namespace HigWaterSystem2
{
    public class WaterRenderingAttributeManager : HigSingleInstance<WaterRenderingAttributeManager>
    {
        public float _RenderNormalDistance = 3000f;
        public float _RenderFoamDistance = 1000f;
        public float _ShadowIntensity = 1f;
        public Color _LightColor = Color.white;
        public Color _WaterColor = Color.cyan;
        public Color _AmbientColor = Color.cyan;
        public Color _ReflectionColor = Color.white;
        public Color _RefractionColor = Color.black;
        public Color _SkyColor = Color.cyan;
        public float _SkyColorHDR = 1f;
        public float _AmbientIntensity = 1f;
        public float _DiffuseIntensity = 1f;
        public float _LambertFactor = 1f;
        public float _Roughness = 0.3f;
        public float _HighLightIntensity = 10f;
        public float _HighLightHDRMax = 3f;

        public float _PointLightRoughness = 0.3f;
        public float _PointLightIntensity = 10f;
        public float _PointLightHDRMax = 3f;

        public float _PeakThreshold = 0.001f;
        public float _PeakScale = 1f;
        public float _PeakFoamThreshold = 0.001f;
        public float _PeakFoamIntensity = 1f;
        public float _FoamIntensity = 1f;
        public float _ReflectionBlendRatio = 0.3f;
        public float _SSScale = 1f;
        public Color _SSColour = Color.cyan;
        public float _SSSIndensity = 1f;
        public float _SSSWaveHeight = 5f;
        public float _SSSRenderDistance = 100f;
        public Gradient _SubsurfaceDepthGradient;
        [HideInInspector]
        public Vector4[] _SSDepthColor;
        public float _SSDepthMax = 100f;
        //-------------------------Fog
        public bool _FogEnable = true;
        public float _FogDistance = 10000f;
        public float _FogPow = 1f;
        public Color _FogColor = Color.white;
        //-------------------------UnderWater
        public bool _UnderWaterEnable;
        public Vector4 Col2Vec4(Color color)
        {
            return new Vector4(color.r, color.g, color.b, color.a);
        }
        public void RefreshAttribute()
        {

            _SSDepthColor = new Vector4[100];
            for (int i = 0; i < _SSDepthColor.Length; i++)
            {
                float t = (float)i / (_SSDepthColor.Length - 1);
                _SSDepthColor[i] = Col2Vec4(_SubsurfaceDepthGradient.Evaluate(t));
            }
            if (_FogEnable)
            {
                MatAttribute.SetInt("_FogEnable", 1);
            }
            else
            {
                MatAttribute.SetInt("_FogEnable", 0);
            }
            MatAttribute.SetInt("_CullMode", _UnderWaterEnable ? 0 : 2);
            MatAttribute.SetColor("_FogColor", _FogColor);
            MatAttribute.SetFloat("_FogDistance", _FogDistance);
            MatAttribute.SetFloat("_FogPow", _FogPow);
            MatAttribute.SetFloat("_RenderNormalDistance", _RenderNormalDistance);
            MatAttribute.SetFloat("_RenderFoamDistance", _RenderFoamDistance);
            MatAttribute.SetFloat("_ShadowIntensity", _ShadowIntensity);
            MatAttribute.SetFloat("_AmbientIntensity", _AmbientIntensity);
            MatAttribute.SetFloat("_DiffuseIntensity", _DiffuseIntensity);
            MatAttribute.SetFloat("_LambertFactor", _LambertFactor);
            MatAttribute.SetFloat("_Roughness", _Roughness);
            MatAttribute.SetFloat("_HighLightIntensity", _HighLightIntensity);
            MatAttribute.SetFloat("_HighLightHDRMax", _HighLightHDRMax);

            MatAttribute.SetFloat("_PointLightRoughness", _PointLightRoughness);
            MatAttribute.SetFloat("_PointLightIntensity", _PointLightIntensity);
            MatAttribute.SetFloat("_PointLightHDRMax", _PointLightHDRMax);

            MatAttribute.SetColor("_LightColor", _LightColor);
            MatAttribute.SetColor("_AmbientColor", _AmbientColor);
            MatAttribute.SetColor("_ReflectionColor", _ReflectionColor);
            MatAttribute.SetColor("_RefractionColor", _RefractionColor);
            MatAttribute.SetColor("_SkyColor", _SkyColor);
            MatAttribute.SetFloat("_SkyColorHDR", _SkyColorHDR);

            MatAttribute.SetColor("_WaterColor", _WaterColor);
            MatAttribute.SetFloat("_PeakThreshold", _PeakThreshold);
            MatAttribute.SetFloat("_PeakScale", _PeakScale);
            MatAttribute.SetFloat("_ReflectionBlendRatio", _ReflectionBlendRatio);

            MatAttribute.SetFloat("_PeakFoamThreshold", _PeakFoamThreshold);
            MatAttribute.SetFloat("_PeakFoamIntensity", _PeakFoamIntensity);
            MatAttribute.SetFloat("_FoamIntensity", _FoamIntensity);

            MatAttribute.SetFloat("_SSScale", _SSScale);
            MatAttribute.SetFloat("_SSSWaveHeight", _SSSWaveHeight);
            MatAttribute.SetColor("_SSColour", _SSColour);
            MatAttribute.SetFloat("_SSSIndensity", _SSSIndensity);

            MatAttribute.SetFloat("_SSDepthMax", _SSDepthMax);

            MatAttribute.SetFloat("_SSSRenderDistance", _SSSRenderDistance);
            MatAttribute.SetVectorArray("_SSDepthColor", _SSDepthColor);

;
        }
        public override void ResetSystem()
        {
            RefreshAttribute();
        }
    }
}
