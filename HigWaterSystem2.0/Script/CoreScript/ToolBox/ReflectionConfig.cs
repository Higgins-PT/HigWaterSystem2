using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using LightType = HigWaterSystem2.ReflectionManager.LightType;
namespace HigWaterSystem2
{
    [System.Serializable]
    public class ReflectionConfig : ToolboxBasic
    {


        public  LightType reflectionType = LightType.SSR;
        public LightType refractionType = LightType.fake;

        public Color SSRReflection_Color = Color.white;
        public Color SSRRefraction_Color = Color.white;
        public float SSRIntensity_Reflection = 2f;
        public float SSRIntensity_Refraction = 2f;
        public float distortionIntensity_Reflection = 2f;
        public float distortionIntensity_Refraction = 4f;
        public float refractionIntensity = 1f;
        public float reflectionIntensity = 1f;

        [Range(0.05f, 1)]
        public float SSRReflectRoughness = 0.35f;
        [Range(0.05f, 1)]
        public float SSRRefractRoughness = 0.05f;
        [Range(0f, 1)]
        public float SkyReflectRoughness = 1f;

        public float reflectClipOffest = 0f;
        public float SSRStep_Reflection = 150f;
        public float SSRStep_Refraction = 150f;
        public float SSRThickness_Reflection = 5f;
        public float SSRThickness_Refraction = 5f;

        [Range(0.05f, 1)]
        public float UnderWaterRefractRoughness = 1f;
        public float UnderWaterRefractIntensity = 1f;
        public float UnderWaterSSRIntensity_Refraction = 1f;
        public int maxMipCount = 4;

        public float maxSSRLenght = 1000f;

        public int texSize = 256;
        public float fReflectionInitRatio = 0f;
        public bool enableBlur;
        public float blurStrength = 1f;

        private Vector2 scrollPosition = Vector2.zero;


        public override void ReadData()
        {
            base.ReadData();

            ReflectionManager manager = ReflectionManager.Instance;

            reflectionType = manager.reflectionType;
            refractionType = manager.refractionType;

            SSRReflection_Color = manager.SSRReflection_Color;
            SSRRefraction_Color = manager.SSRRefraction_Color;
            SSRIntensity_Reflection = manager.SSRIntensity_Reflection;
            SSRIntensity_Refraction = manager.SSRIntensity_Refraction;
            distortionIntensity_Reflection = manager.distortionIntensity_Reflection;
            distortionIntensity_Refraction = manager.distortionIntensity_Refraction;
            refractionIntensity = manager.refractionIntensity;
            reflectionIntensity = manager.reflectionIntensity;
            SSRReflectRoughness = manager.SSRReflectRoughness;
            SSRRefractRoughness = manager.SSRRefractRoughness;
            SkyReflectRoughness = manager.SkyReflectRoughness;
            reflectClipOffest = manager.reflectClipOffest;
            SSRStep_Reflection = manager.SSRStep_Reflection;
            SSRStep_Refraction = manager.SSRStep_Refraction;
            SSRThickness_Reflection = manager.SSRThickness_Reflection;
            SSRThickness_Refraction = manager.SSRThickness_Refraction;
            UnderWaterRefractRoughness = manager.UnderWaterRefractRoughness;
            UnderWaterRefractIntensity = manager.UnderWaterRefractIntensity;
            UnderWaterSSRIntensity_Refraction = manager.UnderWaterSSRIntensity_Refraction;
            maxMipCount = manager.maxMipCount;
            maxSSRLenght = manager.maxSSRLenght;
            texSize = manager.texSize;
            fReflectionInitRatio = manager.fReflectionInitRatio;
            enableBlur = manager.enableBlur;
            blurStrength = manager.blurStrength;
        }


        public override void ApplyData()
        {
            base.ApplyData();

            ReflectionManager manager = ReflectionManager.Instance;

            manager.reflectionType = reflectionType;
            manager.refractionType = refractionType;

            manager.SSRReflection_Color = SSRReflection_Color;
            manager.SSRRefraction_Color = SSRRefraction_Color;
            manager.SSRIntensity_Reflection = SSRIntensity_Reflection;
            manager.SSRIntensity_Refraction = SSRIntensity_Refraction;
            manager.distortionIntensity_Reflection = distortionIntensity_Reflection;
            manager.distortionIntensity_Refraction = distortionIntensity_Refraction;
            manager.refractionIntensity = refractionIntensity;
            manager.reflectionIntensity = reflectionIntensity;
            manager.SSRReflectRoughness = SSRReflectRoughness;
            manager.SSRRefractRoughness = SSRRefractRoughness;
            manager.SkyReflectRoughness = SkyReflectRoughness;
            manager.reflectClipOffest = reflectClipOffest;
            manager.SSRStep_Reflection = SSRStep_Reflection;
            manager.SSRStep_Refraction = SSRStep_Refraction;
            manager.SSRThickness_Reflection = SSRThickness_Reflection;
            manager.SSRThickness_Refraction = SSRThickness_Refraction;
            manager.UnderWaterRefractRoughness = UnderWaterRefractRoughness;
            manager.UnderWaterRefractIntensity = UnderWaterRefractIntensity;
            manager.UnderWaterSSRIntensity_Refraction = UnderWaterSSRIntensity_Refraction;
            manager.maxMipCount = maxMipCount;
            manager.maxSSRLenght = maxSSRLenght;

            manager.texSize = texSize;
            manager.fReflectionInitRatio = fReflectionInitRatio;
            manager.enableBlur = enableBlur;
            manager.blurStrength = blurStrength;
        }

#if UNITY_EDITOR
        public override void RenderGUI(EditorWindow editorWindow)
        {
            base.RenderGUI(editorWindow);

            GUILayout.Label("Reflection Settings", EditorStyles.boldLabel);
            GUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(editorWindow.position.width), GUILayout.Height(editorWindow.position.height - 100));

            reflectionType = (LightType)EditorGUILayout.EnumPopup("Reflection Type", reflectionType);
            refractionType = (LightType)EditorGUILayout.EnumPopup("Refraction Type", refractionType);

            GUILayout.Space(10);
            GUILayout.Label("SSR Settings", EditorStyles.boldLabel);
            SSRReflection_Color = EditorGUILayout.ColorField("SSR Reflection Color", SSRReflection_Color);
            SSRRefraction_Color = EditorGUILayout.ColorField("SSR Refraction Color", SSRRefraction_Color);
            SSRIntensity_Reflection = EditorGUILayout.FloatField("SSR Intensity Reflection", SSRIntensity_Reflection);
            SSRIntensity_Refraction = EditorGUILayout.FloatField("SSR Intensity Refraction", SSRIntensity_Refraction);
            distortionIntensity_Reflection = EditorGUILayout.FloatField("Distortion Intensity Reflection", distortionIntensity_Reflection);
            distortionIntensity_Refraction = EditorGUILayout.FloatField("Distortion Intensity Refraction", distortionIntensity_Refraction);
            refractionIntensity = EditorGUILayout.FloatField("Refraction Intensity", refractionIntensity);
            reflectionIntensity = EditorGUILayout.FloatField("Reflection Intensity", reflectionIntensity);
            SSRReflectRoughness = EditorGUILayout.Slider("SSR Reflect Roughness", SSRReflectRoughness, 0.05f, 1f);
            SSRRefractRoughness = EditorGUILayout.Slider("SSR Refract Roughness", SSRRefractRoughness, 0.05f, 1f);
            SkyReflectRoughness = EditorGUILayout.Slider("Sky Reflect Roughness", SkyReflectRoughness, 0f, 1f);
            reflectClipOffest = EditorGUILayout.FloatField("Reflect Clip Offset", reflectClipOffest);
            SSRStep_Reflection = EditorGUILayout.FloatField("SSR Step Reflection", SSRStep_Reflection);
            SSRStep_Refraction = EditorGUILayout.FloatField("SSR Step Refraction", SSRStep_Refraction);
            SSRThickness_Reflection = EditorGUILayout.FloatField("SSR Thickness Reflection", SSRThickness_Reflection);
            SSRThickness_Refraction = EditorGUILayout.FloatField("SSR Thickness Refraction", SSRThickness_Refraction);
            UnderWaterRefractRoughness = EditorGUILayout.Slider("Underwater Refract Roughness", UnderWaterRefractRoughness, 0.05f, 1f);
            UnderWaterRefractIntensity = EditorGUILayout.FloatField("Underwater Refract Intensity", UnderWaterRefractIntensity);
            UnderWaterSSRIntensity_Refraction = EditorGUILayout.FloatField("Underwater SSR Intensity Refraction", UnderWaterSSRIntensity_Refraction);
            maxMipCount = EditorGUILayout.IntField("Max Mip Count", maxMipCount);
            maxSSRLenght = EditorGUILayout.FloatField("Max SSR Length", maxSSRLenght);

            GUILayout.Space(10);
            texSize = EditorGUILayout.IntField("Texture Size", texSize);
            fReflectionInitRatio = EditorGUILayout.FloatField("Reflection Init Ratio", fReflectionInitRatio);
            enableBlur = EditorGUILayout.Toggle("Enable Blur", enableBlur);
            blurStrength = EditorGUILayout.FloatField("Blur Strength", blurStrength);

            GUILayout.Space(200);
            EditorGUILayout.EndScrollView();
        }
#endif


    }
}
