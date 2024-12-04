using HigWaterSystem2;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UIElements;
namespace HigWaterSystem2
{
    [System.Serializable]
    public class WaterSimulationConfig : ToolboxBasic
    {
        private Vector2 basicOceanSurface_ScrollPosition = Vector2.zero;
        public List<SimTexBase> simTexBases = new List<SimTexBase>();
        public override void ReadData()
        {

            base.ReadData();
            simTexBases.Clear();
            foreach (var simTexBase in OceanWaveformBase.Instance.simTexBases)
            {
                SimTexBase newSimTex = new SimTexBase();
                newSimTex.DeepCopy(simTexBase);
                simTexBases.Add(newSimTex);
            }
        }
        public override void ApplyData()
        {
            base.ApplyData();

            OceanWaveformBase.Instance.simTexBases.Clear();

            foreach (var simTexBase in simTexBases)
            {
                SimTexBase newSimTex = new SimTexBase();
                newSimTex.DeepCopy(simTexBase);
                OceanWaveformBase.Instance.simTexBases.Add(newSimTex);
            }
            IFFTManager.Instance.ResetT0Tex();

        }
#if UNITY_EDITOR
        public override void RenderGUI(EditorWindow editorWindow)
        {
            base.RenderGUI(editorWindow);
            DrawBasicOceanSurface(editorWindow);

        }
#endif
#if UNITY_EDITOR
        private void DrawBasicOceanSurface(EditorWindow editorWindow)
        {
            GUILayout.Label("Basic Ocean Surface", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Ensure OceanWaveformBase.Instance is not null
            if (OceanWaveformBase.Instance == null)
            {
                GUILayout.Label("No OceanWaveformBase instance found.");
                return;
            }

            // Get the list from OceanWaveformBase.Instance
            List<SimTexBase> simTexBases = this.simTexBases;

            // Track the index of the item to be removed
            int indexToRemove = -1;

            // Display the list content
            basicOceanSurface_ScrollPosition = EditorGUILayout.BeginScrollView(basicOceanSurface_ScrollPosition, GUILayout.Width(editorWindow.position.width), GUILayout.Height(editorWindow.position.height));

            if (simTexBases != null && simTexBases.Count > 0)
            {
                for (int i = 0; i < simTexBases.Count; i++)
                {
                    GUILayout.BeginVertical("box");
                    GUILayout.Label("Tex Type: " + simTexBases[i].texType.ToString());
                    simTexBases[i].waterSimEnable = GUILayout.Toggle(simTexBases[i].waterSimEnable, "SimEnable");
                    // Display the settings based on the TexType
                    switch (simTexBases[i].texType)
                    {
                        case SimTexBase.TexType.IFFT:
                            DrawIFFTSettings(simTexBases[i].iFFTSettings);
                            break;
                        case SimTexBase.TexType.Noise:
                            DrawNoiseSettings(simTexBases[i].noiseSettings);
                            break;
                    }

                    if (GUILayout.Button("Remove"))
                    {
                        indexToRemove = i; // Mark the index for removal
                    }

                    GUILayout.EndVertical();
                    GUILayout.Space(10);
                }
            }
            else
            {
                GUILayout.Label("No data available in simTexBases.");
            }

            // Perform the removal after the loop to avoid layout issues
            if (indexToRemove >= 0)
            {
                simTexBases.RemoveAt(indexToRemove);
            }

            // Add button
            if (GUILayout.Button("Add Simulation Type"))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("IFFT"), false, () => AddNewSimTex(SimTexBase.TexType.IFFT));
                menu.ShowAsContext();
            }
            GUILayout.Space(200);
            EditorGUILayout.EndScrollView();

        }
#endif
#if UNITY_EDITOR
        private void DrawNoiseSettings(NoiseSettings settings)
        {
            settings.texSize = EditorGUILayout.IntField("TexSize", settings.texSize);
            settings.timeScale = EditorGUILayout.FloatField("TimeScale", settings.timeScale);
            settings.normalScale = EditorGUILayout.FloatField("NormalScale", settings.normalScale);
            settings.frequency = EditorGUILayout.FloatField("Frequency", settings.frequency);
            settings.octaves = EditorGUILayout.IntSlider("Octaves", settings.octaves, 1, 10);
            settings.amplitude = EditorGUILayout.FloatField("Amplitude", settings.amplitude);
            for (int i = 0; i < settings.lodChannels.Count; i++)
            {
                DrawChannel(settings.lodChannels[i], settings.lodChannels);
            }
            if (GUILayout.Button("AddMap", GUILayout.Width(200)))
            {
                settings.lodChannels.Add(new LodChannel());
            }
        }
        private void DrawIFFTSettings(IFFTSettings settings)
        {
            settings.showIFFTSettings = EditorGUILayout.Foldout(settings.showIFFTSettings, "IFFT Settings");

            if (settings.showIFFTSettings)
            {
                settings.texSize = EditorGUILayout.IntField("TexSize", settings.texSize);
                settings.oceanDepth = EditorGUILayout.FloatField("Ocean Depth", settings.oceanDepth);
                settings.fetch = EditorGUILayout.FloatField("Fetch", settings.fetch);
                settings.windSpeed = EditorGUILayout.FloatField("Wind Speed", settings.windSpeed);
                settings.windSpreadScale = EditorGUILayout.FloatField("Wind Spread Scale", settings.windSpreadScale);
                settings.gammar = EditorGUILayout.FloatField("Gammar", settings.gammar);
                settings.scale = EditorGUILayout.FloatField("Scale", settings.scale);
                settings.swellStrength = EditorGUILayout.FloatField("Swell Strength", settings.swellStrength);
                settings.windDir = EditorGUILayout.FloatField("Wind Direction", settings.windDir);
                settings.timeScale = EditorGUILayout.FloatField("Time Scale", settings.timeScale);
                settings.offest = EditorGUILayout.FloatField("Offest", settings.offest);
                settings.frequencyScale = EditorGUILayout.FloatField("Sim Size", settings.frequencyScale);
                settings.normalScale = EditorGUILayout.FloatField("Normal Scale", settings.normalScale);
                settings.vertScale = EditorGUILayout.FloatField("Vert Scale", settings.vertScale);
                for (int i = 0; i < settings.lodChannels.Count; i++)
                {
                    DrawChannel(settings.lodChannels[i], settings.lodChannels);
                }
                if (GUILayout.Button("AddMap", GUILayout.Width(200)))
                {
                    settings.lodChannels.Add(new LodChannel());
                }
            }

        }
        private void DrawChannel(LodChannel lodChannel, List<LodChannel> lodChannels)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Space(30);
            EditorGUILayout.BeginHorizontal();
            lodChannel.mappedSize = EditorGUILayout.FloatField("MappedSize", lodChannel.mappedSize);
            lodChannel.normalScale = EditorGUILayout.FloatField("NormalScale", lodChannel.normalScale);
            lodChannel.vertScale = EditorGUILayout.FloatField("VertScale", lodChannel.vertScale);

            EditorGUILayout.EndHorizontal();
            lodChannel.renderDistance = EditorGUILayout.FloatField("RenderDistance", lodChannel.renderDistance);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Lod Channel:", GUILayout.Width(100));
            lodChannel.channel_Lod = EditorGUILayout.Toggle("Lod Tex", lodChannel.channel_Lod, GUILayout.Width(200));
            lodChannel.channel_MaxLodIndex = EditorGUILayout.IntField("Lod Max Index", lodChannel.channel_MaxLodIndex, GUILayout.Width(200));

            lodChannel.channel_detail_0 = EditorGUILayout.ToggleLeft("Detail 0", lodChannel.channel_detail_0, GUILayout.Width(80));
            lodChannel.channel_detail_1 = EditorGUILayout.ToggleLeft("Detail 1", lodChannel.channel_detail_1, GUILayout.Width(80));
            lodChannel.physic_Enable = EditorGUILayout.ToggleLeft("Physic", lodChannel.physic_Enable, GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("RemoveMap", GUILayout.Width(200)))
            {
                lodChannels.Remove(lodChannel);
            }
            GUILayout.EndVertical();
        }
#endif
        private void AddNewSimTex(SimTexBase.TexType texType)
        {
            SimTexBase newSimTex = new SimTexBase { texType = texType };
            newSimTex.InitializeSettings();
            simTexBases.Add(newSimTex);
        }
        void Start()
        {

        }
        void Update()
        {

        }
    }
}