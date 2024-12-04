using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace HigWaterSystem2
{
    public class UnderWaterManager : HigSingleInstance<UnderWaterManager>
    {
        public enum RenderType
        {
            RenderPipelineManager,
            RenderFeature
        }
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

        [HideInInspector]
        public Vector4[] depthColor;

        public Material postMat;
        public Material depthShader;
        public Material grabPass;
        public Material blitMat;
        public void Post(ScriptableRenderContext context, Camera cam)
        {
            if (cam.pixelWidth <= 0 || cam.pixelHeight <= 0)
            {
                return;
            }
            if (!cam || !enabled) return;
            UnderWaterRenderer underWaterRenderer = cam.GetComponent<UnderWaterRenderer>();
            if (underWaterRenderer == null)
            {
                return;
            }
            else
            {
                if (underWaterRenderer.enabled == false)
                {
                    return;
                }
            }
            underWaterRenderer.PostUnderWaterCam(context, cam, null);
        }
        public void RenderMask(ScriptableRenderContext context, Camera cam)
        {
            if (cam.pixelWidth <= 0 || cam.pixelHeight <= 0)
            {
                return;
            }
            if (!cam || !enabled) return;
            UnderWaterRenderer underWaterRenderer = cam.GetComponent<UnderWaterRenderer>();
            if (underWaterRenderer == null)
            {
                return;
            }
            else
            {
                if (underWaterRenderer.enabled == false)
                {
                    return;
                }
            }
            underWaterRenderer.RenderWaterMask(context, cam);

        }
        public Vector4 Col2Vec4(Color color)
        {
            return new Vector4(color.r, color.g, color.b, 1);
        }
        public override void ResetSystem()
        {
            depthColor = new Vector4[100];
            for (int i = 0; i < depthColor.Length; i++)
            {
                float t = (float)i / (depthColor.Length - 1);
                depthColor[i] = Col2Vec4(depthColorGradient.Evaluate(t));
            }
            if (renderType == RenderType.RenderPipelineManager)
            {
                postMat.EnableKeyword("INVERT");
            }
            else
            {
                postMat.DisableKeyword("INVERT");
            }
            foreach (var renderer in FindObjectsOfType<UnderWaterRenderer>())
            {

                renderer.ResetSystem();
            }
            try
            {
                RenderPipelineManager.beginCameraRendering -= RenderMask;
                RenderPipelineManager.endCameraRendering -= Post;

            }
            catch
            {

            }
            try
            {
                RenderPipelineManager.beginCameraRendering += RenderMask;
                if (renderType == RenderType.RenderPipelineManager)
                {
                    RenderPipelineManager.endCameraRendering += Post;
                }

            }
            catch
            {

            }


        }

        public void OnDisable()
        {
            try
            {
                RenderPipelineManager.beginCameraRendering -= RenderMask;
                RenderPipelineManager.endCameraRendering -= Post;
            }
            catch
            {

            }
 
        }
        public override void DestroySystem()
        {
            try
            {
                RenderPipelineManager.beginCameraRendering -= RenderMask;
                RenderPipelineManager.endCameraRendering -= Post;
            }
            catch
            {

            }

        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
#if UNITY_EDITOR
        private void AddUnderWaterFeature(ScriptableRendererData rendererData)
        {

            UnderWaterFeature newFeature = ScriptableObject.CreateInstance<UnderWaterFeature>();
            newFeature.name = "UnderWaterFeature";

            rendererData.rendererFeatures.Add(newFeature);

            newFeature.SetActive(true);

            EditorUtility.SetDirty(rendererData);
            AssetDatabase.SaveAssets();
            AssetDatabase.AddObjectToAsset(newFeature, rendererData);

            EditorUtility.SetDirty(newFeature);
            EditorUtility.SetDirty(rendererData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("UnderWaterFeature has been added to the renderer.");
        }
        public void FixGUI()
        {
            UniversalRenderPipelineAsset urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

            if (urpAsset == null)
            {
                EditorGUILayout.HelpBox("The current render pipeline is not URP.", MessageType.Error);
                return;
            }

            FieldInfo fieldInfo = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo == null)
            {
                EditorGUILayout.HelpBox("Unable to access the renderer data list.", MessageType.Error);
                return;
            }


            ScriptableRendererData[] rendererDataList = fieldInfo.GetValue(urpAsset) as ScriptableRendererData[];

            if (rendererDataList == null || rendererDataList.Length == 0)
            {
                EditorGUILayout.HelpBox("The renderer data list is empty.", MessageType.Error);
                return;
            }

            int rendererIndex = 0;
            if (rendererIndex >= rendererDataList.Length)
            {
                EditorGUILayout.HelpBox($"Renderer index out of range. Maximum index is {rendererDataList.Length - 1}.", MessageType.Error);
                return;
            }

            ScriptableRendererData rendererData = rendererDataList[rendererIndex];

            if (rendererData == null)
            {
                EditorGUILayout.HelpBox("Unable to get the renderer data.", MessageType.Error);
                return;
            }

            bool featureExists = false;
            foreach (var feature in rendererData.rendererFeatures)
            {
                if (feature != null && feature is UnderWaterFeature)
                {
                    featureExists = true;
                    break;
                }
            }

            if (!featureExists)
            {
                EditorGUILayout.HelpBox("UnderWaterFeature is not added to the renderer, which may cause underwater effects to not display correctly.", MessageType.Warning);

                if (GUILayout.Button("Fix"))
                {
                    AddUnderWaterFeature(rendererData);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("UnderWaterFeature is correctly added to the renderer.", MessageType.Info);
            }
        }
#endif
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(UnderWaterManager))]
    public class UnderWaterManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {

            DrawDefaultInspector();
            UnderWaterManager manager = (UnderWaterManager)target;
            manager.FixGUI();
            // Get the current render pipeline asset

        }


    }
#endif
}