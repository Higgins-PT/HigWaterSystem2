using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace HigWaterSystem2
{
    [System.Serializable]
    public class InteractiveWaterConfig : ToolboxBasic
    {

        public bool enable = true;
        public float WaveLimitHeight = 20f;
        public float WaveViscosity = 1.0f;
        public float WaveSpeed = 1.0f;
        public float WaveAttenuate = 0.99f;
        public float WaveOffest = 1f;
        public float foamSpawnIntensity = 1f;
        public float foamAttenuate = 0.99f;
        public int startLodIndex = 1;
        public int endLodIndex = 10;
        public int texSize = 512;
        public int lodCount = 5;
        private Vector2 scrollPosition = Vector2.zero;


        public override void ReadData()
        {
            base.ReadData();

            InteractiveWaterManager manager = InteractiveWaterManager.Instance;

            enable = manager.enable;
            WaveLimitHeight = manager.WaveLimitHeight;
            WaveViscosity = manager.WaveViscosity;
            WaveSpeed = manager.WaveSpeed;
            WaveAttenuate = manager.WaveAttenuate;
            WaveOffest = manager.WaveOffest;
            foamSpawnIntensity = manager.foamSpawnIntensity;
            foamAttenuate = manager.foamAttenuate;
            startLodIndex = manager.startLodIndex;
            endLodIndex = manager.endLodIndex;
            texSize = manager.texSize;
            lodCount = manager.lodCount;
        }

        public override void ApplyData()
        {
            base.ApplyData();

            InteractiveWaterManager manager = InteractiveWaterManager.Instance;

            manager.enable = enable;
            manager.WaveLimitHeight = WaveLimitHeight;
            manager.WaveViscosity = WaveViscosity;
            manager.WaveSpeed = WaveSpeed;
            manager.WaveAttenuate = WaveAttenuate;
            manager.WaveOffest = WaveOffest;
            manager.foamSpawnIntensity = foamSpawnIntensity;
            manager.foamAttenuate = foamAttenuate;
            manager.startLodIndex = startLodIndex;
            manager.endLodIndex = endLodIndex;
            manager.texSize = texSize;
            manager.lodCount = lodCount;
        }
#if UNITY_EDITOR
        public override void RenderGUI(EditorWindow editorWindow)
        {
            base.RenderGUI(editorWindow);

            GUILayout.Label("Interactive Water Settings", EditorStyles.boldLabel);
            GUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(editorWindow.position.width), GUILayout.Height(editorWindow.position.height - 100));

            enable = EditorGUILayout.Toggle("Enable", enable);
            WaveLimitHeight = EditorGUILayout.FloatField("Wave Limit Height", WaveLimitHeight);
            WaveViscosity = EditorGUILayout.FloatField("Wave Viscosity", WaveViscosity);
            WaveSpeed = EditorGUILayout.FloatField("Wave Speed", WaveSpeed);
            WaveAttenuate = EditorGUILayout.FloatField("Wave Attenuate", WaveAttenuate);
            WaveOffest = EditorGUILayout.FloatField("Wave Offset", WaveOffest);
            foamSpawnIntensity = EditorGUILayout.FloatField("Foam Spawn Intensity", foamSpawnIntensity);
            foamAttenuate = EditorGUILayout.FloatField("Foam Attenuate", foamAttenuate);
            startLodIndex = EditorGUILayout.IntField("Start LOD Index", startLodIndex);
            endLodIndex = EditorGUILayout.IntField("End LOD Index", endLodIndex);
            texSize = EditorGUILayout.IntField("Texture Size", texSize);
            lodCount = EditorGUILayout.IntField("LOD Count", lodCount);

            GUILayout.Space(200);
            EditorGUILayout.EndScrollView();
        }
        #endif
    }
}
