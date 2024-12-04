using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace HigWaterSystem2
{
    [System.Serializable]
    public class SSAOConfig : ToolboxBasic
    {
        public bool enable = true;
        public float SSAOShadowMin = 0.2f;
        public float SSAORadiusMin = 0.2f;
        public float SSAORadiusMax = 1f;
        public float SSAODistance = 100f;
        public float SSAOIntensity = 1f;
        public float SSAOIntensityFactor = 1f;
        public float sigma = 2f;
        public float normalDisturbanceIntensity = 0f;
        [Range(2, 64)]
        public int samplePointsCount = 16;
        public bool enableBlur = false;
        public float blurNormalThreshold = 0.8f;
        public float blurRangeFactor = 3f;
        [Range(0, 5)]
        public int resolutionScaleFactor = 0;


        private Vector2 scrollPosition = Vector2.zero;


        public override void ReadData()
        {
            base.ReadData();

            SSAOManager manager = SSAOManager.Instance;
            if (manager == null)
            {
                Debug.LogError("SSAOManager instance is null.");
                return;
            }

            enable = manager.enable;
            SSAOShadowMin = manager.SSAOShadowMin;
            SSAORadiusMin = manager.SSAORadiusMin;
            SSAORadiusMax = manager.SSAORadiusMax;
            SSAODistance = manager.SSAODistance;
            SSAOIntensity = manager.SSAOIntensity;
            SSAOIntensityFactor = manager.SSAOIntensityFactor;
            sigma = manager.sigma;
            normalDisturbanceIntensity = manager.normalDisturbanceIntensity;
            samplePointsCount = manager.samplePointsCount;
            enableBlur = manager.enableBlur;
            blurNormalThreshold = manager.blurNormalThreshold;
            blurRangeFactor = manager.blurRangeFactor;
            resolutionScaleFactor = manager.resolutionScaleFactor;
        }

        public override void ApplyData()
        {
            base.ApplyData();

            SSAOManager manager = SSAOManager.Instance;
            if (manager == null)
            {
                Debug.LogError("SSAOManager instance is null.");
                return;
            }

            manager.enable = enable;
            manager.SSAOShadowMin = SSAOShadowMin;
            manager.SSAORadiusMin = SSAORadiusMin;
            manager.SSAORadiusMax = SSAORadiusMax;
            manager.SSAODistance = SSAODistance;
            manager.SSAOIntensity = SSAOIntensity;
            manager.SSAOIntensityFactor = SSAOIntensityFactor;
            manager.sigma = sigma;
            manager.normalDisturbanceIntensity = normalDisturbanceIntensity;
            manager.samplePointsCount = samplePointsCount;
            manager.enableBlur = enableBlur;
            manager.blurNormalThreshold = blurNormalThreshold;
            manager.blurRangeFactor = blurRangeFactor;
            manager.resolutionScaleFactor = resolutionScaleFactor;
        }
#if UNITY_EDITOR

        public override void RenderGUI(EditorWindow editorWindow)
        {
            base.RenderGUI(editorWindow);

            GUILayout.Label("SSAO Settings", EditorStyles.boldLabel);
            GUILayout.Space(10);


            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(editorWindow.position.width), GUILayout.Height(editorWindow.position.height - 100));

            enable = EditorGUILayout.Toggle("Enable", enable);
            SSAOShadowMin = EditorGUILayout.FloatField("SSAO Shadow Min", SSAOShadowMin);
            SSAORadiusMin = EditorGUILayout.FloatField("SSAO Radius Min", SSAORadiusMin);
            SSAORadiusMax = EditorGUILayout.FloatField("SSAO Radius Max", SSAORadiusMax);
            SSAODistance = EditorGUILayout.FloatField("SSAO Distance", SSAODistance);
            SSAOIntensity = EditorGUILayout.FloatField("SSAO Intensity", SSAOIntensity);
            SSAOIntensityFactor = EditorGUILayout.FloatField("SSAO Intensity Factor", SSAOIntensityFactor);
            sigma = EditorGUILayout.FloatField("Sigma", sigma);
            normalDisturbanceIntensity = EditorGUILayout.FloatField("Normal Disturbance Intensity", normalDisturbanceIntensity);
            samplePointsCount = EditorGUILayout.IntSlider("Sample Points Count", samplePointsCount, 2, 64);
            enableBlur = EditorGUILayout.Toggle("Enable Blur", enableBlur);
            blurNormalThreshold = EditorGUILayout.FloatField("Blur Normal Threshold", blurNormalThreshold);
            blurRangeFactor = EditorGUILayout.FloatField("Blur Range Factor", blurRangeFactor);
            resolutionScaleFactor = EditorGUILayout.IntSlider("Resolution Scale Factor", resolutionScaleFactor, 0, 5);
            GUILayout.Space(200);
            EditorGUILayout.EndScrollView();
        }



#endif
    }
}
