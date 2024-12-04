#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace HigWaterSystem2
{
    [System.Serializable]
    public class WaterRenderingAttributeConfig : ToolboxBasic
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
        // -------------------------Fog
        public bool _FogEnable = true;
        public float _FogDistance = 10000f;
        public float _FogPow = 1f;
        public Color _FogColor = Color.white;
        // -------------------------UnderWater
        public bool _UnderWaterEnable;

        private Vector2 scrollPosition = Vector2.zero;

        public override void ReadData()
        {
            base.ReadData();

            WaterRenderingAttributeManager manager = WaterRenderingAttributeManager.Instance;

            _RenderNormalDistance = manager._RenderNormalDistance;
            _RenderFoamDistance = manager._RenderFoamDistance;
            _ShadowIntensity = manager._ShadowIntensity;
            _LightColor = manager._LightColor;
            _WaterColor = manager._WaterColor;
            _AmbientColor = manager._AmbientColor;
            _ReflectionColor = manager._ReflectionColor;
            _RefractionColor = manager._RefractionColor;
            _SkyColor = manager._SkyColor;
            _SkyColorHDR = manager._SkyColorHDR;
            _AmbientIntensity = manager._AmbientIntensity;
            _DiffuseIntensity = manager._DiffuseIntensity;
            _LambertFactor = manager._LambertFactor;
            _Roughness = manager._Roughness;
            _HighLightIntensity = manager._HighLightIntensity;
            _HighLightHDRMax = manager._HighLightHDRMax;

            _PointLightRoughness = manager._PointLightRoughness;
            _PointLightIntensity = manager._PointLightIntensity;
            _PointLightHDRMax = manager._PointLightHDRMax;

            _PeakThreshold = manager._PeakThreshold;
            _PeakScale = manager._PeakScale;
            _PeakFoamThreshold = manager._PeakFoamThreshold;
            _PeakFoamIntensity = manager._PeakFoamIntensity;
            _FoamIntensity = manager._FoamIntensity;
            _ReflectionBlendRatio = manager._ReflectionBlendRatio;
            _SSScale = manager._SSScale;
            _SSColour = manager._SSColour;
            _SSSIndensity = manager._SSSIndensity;
            _SSSWaveHeight = manager._SSSWaveHeight;
            _SSSRenderDistance = manager._SSSRenderDistance;
            _SubsurfaceDepthGradient = manager._SubsurfaceDepthGradient;
            _SSDepthColor = manager._SSDepthColor;
            _SSDepthMax = manager._SSDepthMax;

            // Fog
            _FogEnable = manager._FogEnable;
            _FogDistance = manager._FogDistance;
            _FogPow = manager._FogPow;
            _FogColor = manager._FogColor;

            // UnderWater
            _UnderWaterEnable = manager._UnderWaterEnable;
        }


        public override void ApplyData()
        {
            base.ApplyData();

            WaterRenderingAttributeManager manager = WaterRenderingAttributeManager.Instance;

            manager._RenderNormalDistance = _RenderNormalDistance;
            manager._RenderFoamDistance = _RenderFoamDistance;
            manager._ShadowIntensity = _ShadowIntensity;
            manager._LightColor = _LightColor;
            manager._WaterColor = _WaterColor;
            manager._AmbientColor = _AmbientColor;
            manager._ReflectionColor = _ReflectionColor;
            manager._RefractionColor = _RefractionColor;
            manager._SkyColor = _SkyColor;
            manager._SkyColorHDR = _SkyColorHDR;
            manager._AmbientIntensity = _AmbientIntensity;
            manager._DiffuseIntensity = _DiffuseIntensity;
            manager._LambertFactor = _LambertFactor;
            manager._Roughness = _Roughness;
            manager._HighLightIntensity = _HighLightIntensity;
            manager._HighLightHDRMax = _HighLightHDRMax;

            manager._PointLightRoughness = _PointLightRoughness;
            manager._PointLightIntensity = _PointLightIntensity;
            manager._PointLightHDRMax = _PointLightHDRMax;

            manager._PeakThreshold = _PeakThreshold;
            manager._PeakScale = _PeakScale;
            manager._PeakFoamThreshold = _PeakFoamThreshold;
            manager._PeakFoamIntensity = _PeakFoamIntensity;
            manager._FoamIntensity = _FoamIntensity;
            manager._ReflectionBlendRatio = _ReflectionBlendRatio;
            manager._SSScale = _SSScale;
            manager._SSColour = _SSColour;
            manager._SSSIndensity = _SSSIndensity;
            manager._SSSWaveHeight = _SSSWaveHeight;
            manager._SSSRenderDistance = _SSSRenderDistance;
            manager._SubsurfaceDepthGradient = _SubsurfaceDepthGradient;
            manager._SSDepthColor = _SSDepthColor;
            manager._SSDepthMax = _SSDepthMax;

            // Fog
            manager._FogEnable = _FogEnable;
            manager._FogDistance = _FogDistance;
            manager._FogPow = _FogPow;
            manager._FogColor = _FogColor;

            // UnderWater
            manager._UnderWaterEnable = _UnderWaterEnable;
        }

#if UNITY_EDITOR
        public override void RenderGUI(EditorWindow editorWindow)
        {
            base.RenderGUI(editorWindow);

            GUILayout.Label("Water Rendering Attributes", EditorStyles.boldLabel);
            GUILayout.Space(10);


            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(editorWindow.position.width), GUILayout.Height(editorWindow.position.height - 100));

            _RenderNormalDistance = EditorGUILayout.FloatField("Render Normal Distance", _RenderNormalDistance);
            _RenderFoamDistance = EditorGUILayout.FloatField("Render Foam Distance", _RenderFoamDistance);
            _ShadowIntensity = EditorGUILayout.FloatField("Shadow Intensity", _ShadowIntensity);
            _LightColor = EditorGUILayout.ColorField("Light Color", _LightColor);
            _WaterColor = EditorGUILayout.ColorField("Water Color", _WaterColor);
            _AmbientColor = EditorGUILayout.ColorField("Ambient Color", _AmbientColor);
            _ReflectionColor = EditorGUILayout.ColorField("Reflection Color", _ReflectionColor);
            _RefractionColor = EditorGUILayout.ColorField("Refraction Color", _RefractionColor);
            _SkyColor = EditorGUILayout.ColorField("Sky Color", _SkyColor);
            _SkyColorHDR = EditorGUILayout.FloatField("Sky Color HDR", _SkyColorHDR);
            _AmbientIntensity = EditorGUILayout.FloatField("Ambient Intensity", _AmbientIntensity);
            _DiffuseIntensity = EditorGUILayout.FloatField("Diffuse Intensity", _DiffuseIntensity);
            _LambertFactor = EditorGUILayout.FloatField("Lambert Factor", _LambertFactor);
            _ReflectionBlendRatio = EditorGUILayout.FloatField("Reflection Blend Ratio", _ReflectionBlendRatio);
            _Roughness = EditorGUILayout.FloatField("Roughness", _Roughness);
            _HighLightIntensity = EditorGUILayout.FloatField("Highlight Intensity", _HighLightIntensity);
            _HighLightHDRMax = EditorGUILayout.FloatField("Highlight HDR Max", _HighLightHDRMax);

            GUILayout.Space(10);
            GUILayout.Label("Point Light Settings", EditorStyles.boldLabel);
            _PointLightRoughness = EditorGUILayout.FloatField("Point Light Roughness", _PointLightRoughness);
            _PointLightIntensity = EditorGUILayout.FloatField("Point Light Intensity", _PointLightIntensity);
            _PointLightHDRMax = EditorGUILayout.FloatField("Point Light HDR Max", _PointLightHDRMax);

            GUILayout.Space(10);
            GUILayout.Label("Peak and Foam Settings", EditorStyles.boldLabel);
            _PeakThreshold = EditorGUILayout.FloatField("Peak Threshold", _PeakThreshold);
            _PeakScale = EditorGUILayout.FloatField("Peak Scale", _PeakScale);
            _PeakFoamThreshold = EditorGUILayout.FloatField("Peak Foam Threshold", _PeakFoamThreshold);
            _PeakFoamIntensity = EditorGUILayout.FloatField("Peak Foam Intensity", _PeakFoamIntensity);
            _FoamIntensity = EditorGUILayout.FloatField("Foam Intensity", _FoamIntensity);


            GUILayout.Space(10);
            GUILayout.Label("Subsurface Scattering (SSS) Settings", EditorStyles.boldLabel);
            _SSScale = EditorGUILayout.FloatField("SS Scale", _SSScale);
            _SSColour = EditorGUILayout.ColorField("SS Colour", _SSColour);
            _SSSIndensity = EditorGUILayout.FloatField("SSS Intensity", _SSSIndensity);
            _SSSWaveHeight = EditorGUILayout.FloatField("SSS Wave Height", _SSSWaveHeight);
            _SSSRenderDistance = EditorGUILayout.FloatField("SSS Render Distance", _SSSRenderDistance);
            _SubsurfaceDepthGradient = EditorGUILayout.GradientField("Subsurface Depth Gradient", _SubsurfaceDepthGradient);
            _SSDepthMax = EditorGUILayout.FloatField("SS Depth Max", _SSDepthMax);

            GUILayout.Space(10);
            GUILayout.Label("Fog Settings", EditorStyles.boldLabel);
            _FogEnable = EditorGUILayout.Toggle("Fog Enable", _FogEnable);
            _FogDistance = EditorGUILayout.FloatField("Fog Distance", _FogDistance);
            _FogPow = EditorGUILayout.FloatField("Fog Power", _FogPow);
            _FogColor = EditorGUILayout.ColorField("Fog Color", _FogColor);

            GUILayout.Space(10);
            GUILayout.Label("Underwater Settings", EditorStyles.boldLabel);
            _UnderWaterEnable = EditorGUILayout.Toggle("Underwater Enable", _UnderWaterEnable);
            GUILayout.Space(200);
            EditorGUILayout.EndScrollView();
        }
#endif
    }
}
