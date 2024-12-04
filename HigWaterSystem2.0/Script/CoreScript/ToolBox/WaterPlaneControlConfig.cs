using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace HigWaterSystem2
{
    [System.Serializable]
    public class WaterPlaneControlConfig : ToolboxBasic
    {
        public bool enableDynamicHeightDetection = true;
        public float waterPlaneHeight = 0f;
        public float boundsSizeXZAdd = 10f;
        public float boundsHeight = 100f;
        public float baseLodHeight = 250f;
        public int gridSize = 128;
        public float minSize = 10f;
        public float stretchAmount = 100000f;
        [Range(2, 9)]
        public int lodCount = 5;
        [Range(0, 2)]
        public int detailCount = 2;


        private Vector2 scrollPosition = Vector2.zero;


        public override void ReadData()
        {
            base.ReadData();

            WaterPlaneControl manager = WaterPlaneControl.Instance;
            if (manager == null)
            {
                Debug.LogError("WaterPlaneControl instance is null.");
                return;
            }
            enableDynamicHeightDetection = manager.enableDynamicHeightDetection;
            waterPlaneHeight = manager.waterPlaneHeight;
            boundsSizeXZAdd = manager.boundsSizeXZAdd;
            boundsHeight = manager.boundsHeight;
            baseLodHeight = manager.baseLodHeight;
            gridSize = manager.gridSize;
            minSize = manager.minSize;
            stretchAmount = manager.stretchAmount;
            lodCount = manager.lodCount;
            detailCount = manager.detailCount;
        }

        public override void ApplyData()
        {
            base.ApplyData();

            WaterPlaneControl manager = WaterPlaneControl.Instance;
            if (manager == null)
            {
                Debug.LogError("WaterPlaneControl instance is null.");
                return;
            }
            manager.enableDynamicHeightDetection = enableDynamicHeightDetection;
            manager.waterPlaneHeight = waterPlaneHeight;
            manager.boundsSizeXZAdd = boundsSizeXZAdd;
            manager.boundsHeight = boundsHeight;
            manager.baseLodHeight = baseLodHeight;
            manager.gridSize = gridSize;
            manager.minSize = minSize;
            manager.stretchAmount = stretchAmount;
            manager.lodCount = lodCount;
            manager.detailCount = detailCount;
        }

#if UNITY_EDITOR
        public override void RenderGUI(EditorWindow editorWindow)
        {
            base.RenderGUI(editorWindow);

            GUILayout.Label("Water Plane Control Settings", EditorStyles.boldLabel);
            GUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(editorWindow.position.width), GUILayout.Height(editorWindow.position.height - 100));
            enableDynamicHeightDetection = EditorGUILayout.ToggleLeft("Enable Dynamic Height Detection", enableDynamicHeightDetection);
            waterPlaneHeight = EditorGUILayout.FloatField("Water Plane Height", waterPlaneHeight);
            boundsSizeXZAdd = EditorGUILayout.FloatField("Bounds Size XZ Add", boundsSizeXZAdd);
            boundsHeight = EditorGUILayout.FloatField("Bounds Height", boundsHeight);
            baseLodHeight = EditorGUILayout.FloatField("Base LOD Height", baseLodHeight);
            gridSize = EditorGUILayout.IntField("Grid Size", gridSize);
            minSize = EditorGUILayout.FloatField("Min Size", minSize);
            stretchAmount = EditorGUILayout.FloatField("Stretch Amount", stretchAmount);
            lodCount = EditorGUILayout.IntSlider("LOD Count", lodCount, 2, 9);
            detailCount = EditorGUILayout.IntSlider("Detail Count", detailCount, 0, 2);
            GUILayout.Space(200);
            EditorGUILayout.EndScrollView();
        }
#endif
    }
}
