using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UIElements;
using static HigWaterSystem2.UnderWaterManager;

namespace HigWaterSystem2
{
    [System.Serializable]
    public class UnderWaterConfig : ToolboxBasic
    {
        public RenderType renderType = RenderType.RenderPipelineManager;
        public float underWaterViewDistance = 100f;
        public Color underWaterColor = Color.cyan;
        public float underWaterPow = 1f;
        public Gradient depthColorGradient;
        public float depthColorDistance = 100f;
        public float depthColorFactor = 1f;
        public Color underWaterLightColor = Color.yellow;
        public float underWaterLightPow = 8f;
        public float underWaterLightIntensity = 40f;
        public float underWaterLightBasic = 1f;
        public float seabedLightBasic = 0.2f;
        public float seabedLightPow = 0.8f;
        public float absorptivityOfWater = 1f;


        private Vector2 scrollPosition = Vector2.zero;


        public override void ReadData()
        {
            base.ReadData();

            UnderWaterManager manager = UnderWaterManager.Instance;
            if (manager == null)
            {
                Debug.LogError("UnderWaterManager instance is null.");
                return;
            }
            renderType = manager.renderType;
            underWaterViewDistance = manager.underWaterViewDistance;
            underWaterColor = manager.underWaterColor;
            underWaterPow = manager.underWaterPow;
            depthColorGradient = manager.depthColorGradient;
            depthColorDistance = manager.depthColorDistance;
            depthColorFactor = manager.depthColorFactor;
            underWaterLightColor = manager.underWaterLightColor;
            underWaterLightPow = manager.underWaterLightPow;
            underWaterLightIntensity = manager.underWaterLightIntensity;
            underWaterLightBasic = manager.underWaterLightBasic;
            seabedLightBasic = manager.seabedLightBasic;
            seabedLightPow = manager.seabedLightPow;
            absorptivityOfWater = manager.absorptivityOfWater;
        }


        public override void ApplyData()
        {
            base.ApplyData();

            UnderWaterManager manager = UnderWaterManager.Instance;
            if (manager == null)
            {
                Debug.LogError("UnderWaterManager instance is null.");
                return;
            }
            manager.renderType = renderType;
            manager.underWaterViewDistance = underWaterViewDistance;
            manager.underWaterColor = underWaterColor;
            manager.underWaterPow = underWaterPow;
            manager.depthColorGradient = depthColorGradient;
            manager.depthColorDistance = depthColorDistance;
            manager.depthColorFactor = depthColorFactor;
            manager.underWaterLightColor = underWaterLightColor;
            manager.underWaterLightPow = underWaterLightPow;
            manager.underWaterLightIntensity = underWaterLightIntensity;
            manager.underWaterLightBasic = underWaterLightBasic;
            manager.seabedLightBasic = seabedLightBasic;
            manager.seabedLightPow = seabedLightPow;
            manager.absorptivityOfWater = absorptivityOfWater;
        }
#if UNITY_EDITOR
        public override void RenderGUI(EditorWindow editorWindow)
        {
            base.RenderGUI(editorWindow);

            GUILayout.Label("Underwater Settings", EditorStyles.boldLabel);
            GUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(editorWindow.position.width), GUILayout.Height(editorWindow.position.height - 100));
            renderType = (RenderType)EditorGUILayout.EnumPopup("Render Type", renderType);
            underWaterViewDistance = EditorGUILayout.FloatField("Underwater View Distance", underWaterViewDistance);
            underWaterColor = EditorGUILayout.ColorField("Underwater Color", underWaterColor);
            underWaterPow = EditorGUILayout.FloatField("Underwater Power", underWaterPow);
            depthColorGradient = EditorGUILayout.GradientField("Depth Color Gradient", depthColorGradient);
            depthColorDistance = EditorGUILayout.FloatField("Depth Color Distance", depthColorDistance);
            depthColorFactor = EditorGUILayout.FloatField("Depth Color Factor", depthColorFactor);
            underWaterLightColor = EditorGUILayout.ColorField("Underwater Light Color", underWaterLightColor);
            underWaterLightPow = EditorGUILayout.FloatField("Underwater Light Power", underWaterLightPow);
            underWaterLightIntensity = EditorGUILayout.FloatField("Underwater Light Intensity", underWaterLightIntensity);
            underWaterLightBasic = EditorGUILayout.FloatField("Underwater Light Basic", underWaterLightBasic);
            seabedLightBasic = EditorGUILayout.FloatField("Seabed Light Basic", seabedLightBasic);
            seabedLightPow = EditorGUILayout.FloatField("Seabed Light Power", seabedLightPow);
            absorptivityOfWater = EditorGUILayout.FloatField("Absorbtivity Of Water", absorptivityOfWater);
            UnderWaterManager.Instance.FixGUI();
            GUILayout.Space(200);
            EditorGUILayout.EndScrollView();
        }


#endif
    }
}
