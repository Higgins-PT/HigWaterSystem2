using HigWaterSystem2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HigWaterSystem2
{
#if UNITY_EDITOR
    public class HigWaterSystemToolbox : EditorWindow
    {
        private int selectedTabIndex = 0;
        private int selectedTabIndexLast = 0;
        private string[] toolbarTabs = { "Water Rendering", "Configuration System" };

        // Sub-tabs for Water Simulation
        private int selectedWaterSimSubTabIndex = 0;
        private int selectedWaterSimSubTabIndexLast = 0;
        private string[] waterSimSubTabs = { "Basic Ocean", "Basic Renderer", "Interaction", "Projection", "SSR", "UnderWater", "SSAO", "Texture Manager" ,"Other Setting","Water Plane Setting"};
        private string newConfigName { get { return ConfigManager.Instance.configName; } set { ConfigManager.Instance.configName = value; } }
        private int selectedConfigIndex;
        private float lastDeleteClickTime;
        private float lastSaveClickTime;
        private const float doubleClickThreshold = 0.5f;

        [MenuItem("Tools/HigWaterSystem/Toolbox")]

        public static void ShowWindow()
        {
            // Open the toolbox window
            GetWindow<HigWaterSystemToolbox>("HigWaterSystem Toolbox");
        }

        private void Awake()
        {
        }
        private void OnGUI()
        {

            // Toolbar for selecting different main tabs
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, toolbarTabs);
            if (selectedTabIndex != selectedTabIndexLast)
            {
                selectedTabIndexLast = selectedTabIndex;
                GUI.FocusControl(null);
            }
            // Draw content based on the selected main tab
            switch (selectedTabIndex)
            {
                case 0:
                    DrawWaterSimulationTab();
                    break;

                case 1:
                    DrawConfigSystemTab();
                    break;

            }
        }
        private void DrawWaterSimulationTab()
        {
            GUILayout.Label("Water Simulation Controls", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Draw a single-line toolbar with adjusted width to fit all the options
            int numButtonsPerRow = 5;
            selectedWaterSimSubTabIndex = GUILayout.SelectionGrid(
                selectedWaterSimSubTabIndex,
                waterSimSubTabs,
                numButtonsPerRow,
                GUILayout.Height(25 * Mathf.CeilToInt((float)waterSimSubTabs.Length / numButtonsPerRow)),
                GUILayout.Width(EditorGUIUtility.currentViewWidth)
            );
            if (selectedWaterSimSubTabIndex != selectedWaterSimSubTabIndexLast)
            {
                selectedWaterSimSubTabIndexLast = selectedWaterSimSubTabIndex;
                GUI.FocusControl(null);
            }
;
            // Draw content based on the selected sub-tab
            switch (selectedWaterSimSubTabIndex)
            {
                case 0:
                    DrawBasicOceanSurface();
                    break;
                case 1:
                    DrawWaterRenderer();
                    break;
                case 2:
                    DrawOceanInteraction();
                    break;
                case 3:
                    DrawProjectionSystem();
                    break;
                case 4:
                    DrawSSRSystem();
                    break;
                case 5:
                    DrawUnderWaterSystem();
                    break;
                case 6:
                    DrawSSAOManager();
                    break;
                case 7:
                    DrawTextureManager();
                    break;
                case 8:
                    DrawOtherSetting();
                    break;
                case 9:
                    DrawWaterPlaneSetting();
                    break;

            }


        }
        private void DrawWaterPlaneSetting()
        {
            ConfigManager.Instance.config.waterPlaneControlConfig.RenderGUI(this);
        }
        private void DrawOtherSetting()
        {
            ConfigManager.Instance.config.otherSettingConfig.RenderGUI(this);
            
        }
        private void DrawSSAOManager()
        {
            ConfigManager.Instance.config.sSAOConfig.RenderGUI(this);
        }
        private void DrawUnderWaterSystem()
        {
            ConfigManager.Instance.config.underWaterConfig.RenderGUI(this);
        }
        private void DrawWaterRenderer()
        {
            ConfigManager.Instance.config.waterRenderingAttributeConfig.RenderGUI(this);
        }
        // Draw the Configuration System tab
        private void DrawConfigSystemTab()
        {
            GUILayout.Label("Configuration System", EditorStyles.boldLabel);
            GUILayout.Space(10);
            newConfigName = EditorGUILayout.TextField("Config Name", newConfigName);
            if (GUILayout.Button("Save Config"))
            {
                if (!string.IsNullOrEmpty(newConfigName))
                {
                    bool configExists = false;

                    // Check if the configuration name already exists
                    foreach (string configFile in ConfigManager.Instance.configFiles)
                    {
                        if (configFile.Equals(newConfigName, System.StringComparison.OrdinalIgnoreCase))
                        {
                            configExists = true;
                            break;
                        }
                    }

                    float currentTime = Time.realtimeSinceStartup;

                    if (configExists)
                    {
                        if (currentTime - lastSaveClickTime < doubleClickThreshold)
                        {
                            ConfigManager.Instance.SaveConfig(newConfigName);

                            ConfigManager.Instance.configFiles = ConfigManager.Instance.GetConfigFiles();
                            Debug.Log("Config saved: " + newConfigName);
                        }
                        else
                        {
                            Debug.Log("Config name already exists. Double click to confirm save.");
                        }
                    }
                    else
                    {
                        ConfigManager.Instance.SaveConfig(newConfigName);

                        ConfigManager.Instance.configFiles = ConfigManager.Instance.GetConfigFiles();
                        Debug.Log("Config saved: " + newConfigName);
                    }

                    lastSaveClickTime = currentTime;
                }
                else
                {
                    Debug.LogWarning("Config name cannot be empty.");
                }
            }

            if (ConfigManager.Instance.configFiles.Count > 0)
            {
                selectedConfigIndex = EditorGUILayout.Popup("Select Config to Load", selectedConfigIndex, ConfigManager.Instance.configFiles.ToArray());
                foreach (var config in ConfigManager.Instance.toolBoxReadFilter.ToList())
                {
                    ConfigManager.Instance.toolBoxReadFilter[config.Key] = GUILayout.Toggle(config.Value, config.Key.Name);
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Select All"))
                {
                    var keys = ConfigManager.Instance.toolBoxReadFilter.Keys.ToList();
                    foreach (var key in keys)
                    {
                        ConfigManager.Instance.toolBoxReadFilter[key] = true;
                    }
                }
                if (GUILayout.Button("Deselect All"))
                {
                    var keys = ConfigManager.Instance.toolBoxReadFilter.Keys.ToList();
                    foreach (var key in keys)
                    {
                        ConfigManager.Instance.toolBoxReadFilter[key] = false;
                    }
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Load Config"))
                {
                    string selectedConfigName = ConfigManager.Instance.configFiles[selectedConfigIndex];
                    newConfigName = selectedConfigName;
                    ConfigManager.Instance.LoadConfig(selectedConfigName);
                }

                if (GUILayout.Button("Delete Config"))
                {
                    float currentTime = Time.realtimeSinceStartup;
                    if (currentTime - lastDeleteClickTime < doubleClickThreshold)
                    {
                        string selectedConfigName = ConfigManager.Instance.configFiles[selectedConfigIndex];
                        ConfigManager.Instance.DeleteConfig(selectedConfigName);
                        // Refresh the list of configuration files
                        ConfigManager.Instance.configFiles = ConfigManager.Instance.GetConfigFiles();
                        selectedConfigIndex = 0; // Reset the selected index
                        Debug.Log("Config deleted: " + selectedConfigName);
                    }
                    else
                    {
                        Debug.Log("Double click to confirm delete.");
                    }
                    lastDeleteClickTime = currentTime;
                }

            }
            else
            {
                EditorGUILayout.LabelField("No configuration files found.");
            }



            if (GUILayout.Button("Read Config"))
            {
                ReadConfiguration();
            }
            if (GUILayout.Button("Apply Config"))
            {
                ApplyConfiguration();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Reset System"))
            {
                HigSingleInstance<MonoBehaviour>.ResetAllSystem(); 
            }
            URPRendererData();
        }
        public void URPRendererData()
        {
            string rendererDataName = "SimplePipeline_Renderer";
            UniversalRenderPipelineAsset urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            if (urpAsset == null)
            {
                EditorGUILayout.HelpBox("The current render pipeline is not URP.", MessageType.Error);
                return;
            }

            // Use reflection to access the private field m_RendererDataList
            FieldInfo fieldInfo = typeof(UniversalRenderPipelineAsset).GetField(
                "m_RendererDataList",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            if (fieldInfo == null)
            {
                EditorGUILayout.HelpBox("Unable to access the renderer data list.", MessageType.Error);
                return;
            }

            // Get the renderer data list
            ScriptableRendererData[] rendererDataList = fieldInfo.GetValue(urpAsset) as ScriptableRendererData[];

            if (rendererDataList == null)
            {
                EditorGUILayout.HelpBox("Renderer data list is null.", MessageType.Error);
                return;
            }

            // Find the ScriptableRendererData asset by name
            ScriptableRendererData targetRendererData = FindRendererDataByName(rendererDataName);

            if (targetRendererData == null)
            {
                EditorGUILayout.HelpBox($"ScriptableRendererData named '{rendererDataName}' not found in the project.", MessageType.Error);
                return;
            }

            // Check if targetRendererData exists in the rendererDataList
            bool exists = false;
            foreach (var data in rendererDataList)
            {
                if (data == targetRendererData)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                EditorGUILayout.HelpBox($"RendererData '{rendererDataName}' is not in the renderer data list.", MessageType.Warning);

                if (GUILayout.Button($"Add '{rendererDataName}' to Renderer Data List at Index 1"))
                {
                    AddRendererDataAtIndex(urpAsset, rendererDataList, targetRendererData, 1);
                }
            }
            else
            {
                EditorGUILayout.HelpBox($"RendererData '{rendererDataName}' is already in the renderer data list.", MessageType.Info);
            }
        }
        private ScriptableRendererData FindRendererDataByName(string name)
        {
            string[] guids = AssetDatabase.FindAssets($"{name} t:ScriptableRendererData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableRendererData rendererData = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(path);
                if (rendererData != null && rendererData.name == name)
                {
                    return rendererData;
                }
            }
            return null;
        }
        private void AddRendererDataAtIndex(
        UniversalRenderPipelineAsset urpAsset,
        ScriptableRendererData[] rendererDataList,
        ScriptableRendererData newData,
        int index
    )
        {
            // Create a new list with the new renderer data inserted at the specified index
            var newRendererDataList = new System.Collections.Generic.List<ScriptableRendererData>(rendererDataList);
            newRendererDataList.Insert(index, newData);

            // Set the updated list back to the URP asset using reflection
            FieldInfo fieldInfo = typeof(UniversalRenderPipelineAsset).GetField(
                "m_RendererDataList",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            fieldInfo.SetValue(urpAsset, newRendererDataList.ToArray());

            // Mark the URP asset as dirty and save
            EditorUtility.SetDirty(urpAsset);
            AssetDatabase.SaveAssets();

            Debug.Log($"Added {newData.name} to renderer data list at index {index}.");
        }
        private void DrawBasicOceanSurface()
        {
            ConfigManager.Instance.config.simulationConfig.RenderGUI(this);

        }


        // Sub-toolbox for Ocean Interaction
        private void DrawOceanInteraction()
        {
            ConfigManager.Instance.config.interactiveWaterConfig.RenderGUI(this);
        }

        // Sub-toolbox for River System
        private void DrawProjectionSystem()
        {
            ConfigManager.Instance.config.cameraMapConfig.RenderGUI(this);
        }
        private void DrawSSRSystem()
        {
            ConfigManager.Instance.config.reflectionConfig.RenderGUI(this);
        }

        public void DrawTextureManager()
        {
            ConfigManager.Instance.config.textureManagerConfig.RenderGUI(this);
            
        }
        // Configuration System functionality
        private void ReadConfiguration()
        {
            ConfigManager.Instance.ReadConfiguration();
        }
        private void ApplyConfiguration()
        {
            ConfigManager.Instance.ApplyConfiguration();
        }


    }
#endif
    [System.Serializable]
    public class Config
    {
        public WaterSimulationConfig simulationConfig = new WaterSimulationConfig();
        public WaterRenderingAttributeConfig waterRenderingAttributeConfig = new WaterRenderingAttributeConfig();
        public CameraMapConfig cameraMapConfig = new CameraMapConfig();
        public InteractiveWaterConfig interactiveWaterConfig = new InteractiveWaterConfig();
        public ReflectionConfig reflectionConfig = new ReflectionConfig();
        public UnderWaterConfig underWaterConfig = new UnderWaterConfig();
        public TextureManagerConfig textureManagerConfig = new TextureManagerConfig();
        public SSAOConfig sSAOConfig = new SSAOConfig();
        public OtherSettingConfig otherSettingConfig = new OtherSettingConfig();
        public WaterPlaneControlConfig waterPlaneControlConfig = new WaterPlaneControlConfig();
        public List<ToolboxBasic> toolboxBasics = new List<ToolboxBasic>();
        public void CopyConfig(Config config)
        {
            bool enable = false;

            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var fieldsC = config.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (typeof(ToolboxBasic).IsAssignableFrom(field.FieldType))
                {
                    Type type = field.FieldType;


                    var fieldValue = field.GetValue(this);

                    if (fieldValue != null)
                    {

                        var toolboxBasicInstance = fieldValue as ToolboxBasic;

                        if ((toolboxBasicInstance != null && !toolboxBasicInstance.noNewToolBox) || (ConfigManager.Instance.toolBoxReadFilter.TryGetValue(type, out enable) && !enable))
                        {
                            var correspondingField = fieldsC.FirstOrDefault(f => f.Name == field.Name && f.FieldType == field.FieldType);

                            if (correspondingField != null)
                            {
                                var value = correspondingField.GetValue(config);
                                field.SetValue(this, value);
                            }
                        }

                    }
                }
            }

        }
        public void InitToolBoxBasics()
        {
            toolboxBasics.Clear();
            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (typeof(ToolboxBasic).IsAssignableFrom(field.FieldType))
                {

                    var value = field.GetValue(this);
                    if (value != null)
                    {
                        var method = field.FieldType.GetMethod("AddBasic");
                        method.Invoke(value, new object[] { toolboxBasics });
                    }
                }
            }

        }
        public void ApplyData(Dictionary<Type, bool> toolBoxReadFilter)
        {

            foreach (ToolboxBasic toolboxBasic in toolboxBasics)
            {
                toolboxBasic.noNewToolBox = true;
                if (toolBoxReadFilter.TryGetValue(toolboxBasic.GetType(), out bool enable))
                {
                    if (enable)
                    {
                        toolboxBasic.ApplyData();
                    }
                }
            }
        }
        public void ReadData()
        {
            foreach (ToolboxBasic toolboxBasic in toolboxBasics)
            {
                toolboxBasic.ReadData();
            }
        }
    }
}
