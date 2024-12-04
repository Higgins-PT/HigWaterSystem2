using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace HigWaterSystem2
{
    [System.Serializable]
    public class CameraMapConfig : ToolboxBasic
    {

        public float camHeight = 5000f;
        public float camDepth = 5000f;
        public int heightTexSize = 256;
        public int waveScaleDataTexSize = 256; // wave(IFFTScale, heightTexScale)
        public int aldobeTexSize = 512;
        public int flowmapTexSize = 256;

        public int heightLodLimit = 9;
        public int waveScaleLodLimit = 5;
        public int flowmapLodLimit = 4;
        public int aldobeLodLimit = 5;
        public bool heightEnable = true;
        public bool waveScaleEnable = true;
        public bool aldobeEnable = true;
        public bool flowmapEnable = true;
        public float _FlowTimeSpeed = 1f;
        public float _FlowMaxDistance = 4f;


        private Vector2 scrollPosition = Vector2.zero;


        public override void ReadData()
        {
            base.ReadData();

            CameraMapManager manager = CameraMapManager.Instance;

            camHeight = manager.camHeight;
            camDepth = manager.camDepth;
            heightTexSize = manager.heightTexSize;
            waveScaleDataTexSize = manager.waveScaleDataTexSize;
            aldobeTexSize = manager.aldobeTexSize;
            flowmapTexSize = manager.flowmapTexSize;

            heightLodLimit = manager.heightLodLimit;
            waveScaleLodLimit = manager.waveScaleLodLimit;
            flowmapLodLimit = manager.flowmapLodLimit;
            aldobeLodLimit = manager.aldobeLodLimit;
            heightEnable = manager.heightEnable;
            waveScaleEnable = manager.waveScaleEnable;
            aldobeEnable = manager.aldobeEnable;
            flowmapEnable = manager.flowmapEnable;
            _FlowTimeSpeed = manager._FlowTimeSpeed;
            _FlowMaxDistance = manager._FlowMaxDistance;
        }


        public override void ApplyData()
        {
            base.ApplyData();

            CameraMapManager manager = CameraMapManager.Instance;

            manager.camHeight = camHeight;
            manager.camDepth = camDepth;
            manager.heightTexSize = heightTexSize;
            manager.waveScaleDataTexSize = waveScaleDataTexSize;
            manager.aldobeTexSize = aldobeTexSize;
            manager.flowmapTexSize = flowmapTexSize;

            manager.heightLodLimit = heightLodLimit;
            manager.waveScaleLodLimit = waveScaleLodLimit;
            manager.flowmapLodLimit = flowmapLodLimit;
            manager.aldobeLodLimit = aldobeLodLimit;
            manager.heightEnable = heightEnable;
            manager.waveScaleEnable = waveScaleEnable;
            manager.aldobeEnable = aldobeEnable;
            manager.flowmapEnable = flowmapEnable;
            manager._FlowTimeSpeed = _FlowTimeSpeed;
            manager._FlowMaxDistance = _FlowMaxDistance;
        }

#if UNITY_EDITOR
        public override void RenderGUI(EditorWindow editorWindow)
        {
            base.RenderGUI(editorWindow);

            GUILayout.Label("Camera Map Settings", EditorStyles.boldLabel);
            GUILayout.Space(10);


            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(editorWindow.position.width), GUILayout.Height(editorWindow.position.height - 100));

            camHeight = EditorGUILayout.FloatField("Camera Height", camHeight);
            camDepth = EditorGUILayout.FloatField("Camera Depth", camDepth);
            heightTexSize = EditorGUILayout.IntField("Height Texture Size", heightTexSize);
            waveScaleDataTexSize = EditorGUILayout.IntField("Wave Scale Data Texture Size", waveScaleDataTexSize);
            aldobeTexSize = EditorGUILayout.IntField("Albedo Texture Size", aldobeTexSize);
            flowmapTexSize = EditorGUILayout.IntField("Flowmap Texture Size", flowmapTexSize);

            GUILayout.Space(10);
            GUILayout.Label("LOD Limits", EditorStyles.boldLabel);
            heightLodLimit = EditorGUILayout.IntField("Height LOD Limit", heightLodLimit);
            waveScaleLodLimit = EditorGUILayout.IntField("Wave Scale LOD Limit", waveScaleLodLimit);
            flowmapLodLimit = EditorGUILayout.IntField("Flowmap LOD Limit", flowmapLodLimit);
            aldobeLodLimit = EditorGUILayout.IntField("Albedo LOD Limit", aldobeLodLimit);

            GUILayout.Space(10);
            GUILayout.Label("Enables", EditorStyles.boldLabel);
            heightEnable = EditorGUILayout.Toggle("Height Enable", heightEnable);
            waveScaleEnable = EditorGUILayout.Toggle("Wave Scale Enable", waveScaleEnable);
            aldobeEnable = EditorGUILayout.Toggle("Albedo Enable", aldobeEnable);
            flowmapEnable = EditorGUILayout.Toggle("Flowmap Enable", flowmapEnable);

            GUILayout.Space(10);
            _FlowTimeSpeed = EditorGUILayout.FloatField("Flow Time Speed", _FlowTimeSpeed);
            _FlowMaxDistance = EditorGUILayout.FloatField("Flow Max Distance", _FlowMaxDistance);

            GUILayout.Space(200);

            EditorGUILayout.EndScrollView();
        }
#endif
    }
}
