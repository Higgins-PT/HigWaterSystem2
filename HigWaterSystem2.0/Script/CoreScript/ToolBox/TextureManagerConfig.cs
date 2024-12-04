using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace HigWaterSystem2
{
    [System.Serializable]
    public class TextureManagerConfig : ToolboxBasic
    {

        public List<int> vertTexSizes = new List<int>();
        public List<int> normalTexSizes = new List<int>();
        public List<int> detailTexSizes = new List<int>();
        public List<float> detailTexRenderDistance = new List<float>();
        public List<float> detailTexMapSize = new List<float>();
        public List<bool> detailTexMapVertEnable = new List<bool>();


        private Vector2 scrollPosition = Vector2.zero;


        public override void ReadData()
        {
            base.ReadData();

            TextureManager manager = TextureManager.Instance;
            if (manager == null)
            {
                Debug.LogError("TextureManager instance is null.");
                return;
            }

  
            vertTexSizes = new List<int>(manager.vertTexSizes);
            normalTexSizes = new List<int>(manager.normalTexSizes);
            detailTexSizes = new List<int>(manager.detailTexSizes);
            detailTexRenderDistance = new List<float>(manager.detailTexRenderDistance);
            detailTexMapSize = new List<float>(manager.detailTexMapSize);
            detailTexMapVertEnable = new List<bool>(manager.detailTexMapVertEnable);
        }

        public override void ApplyData()
        {
            base.ApplyData();

            TextureManager manager = TextureManager.Instance;
            if (manager == null)
            {
                Debug.LogError("TextureManager instance is null.");
                return;
            }

            manager.vertTexSizes = new List<int>(vertTexSizes);
            manager.normalTexSizes = new List<int>(normalTexSizes);
            manager.detailTexSizes = new List<int>(detailTexSizes);
            manager.detailTexRenderDistance = new List<float>(detailTexRenderDistance);
            manager.detailTexMapSize = new List<float>(detailTexMapSize);
            manager.detailTexMapVertEnable = new List<bool>(detailTexMapVertEnable);
        }
#if UNITY_EDITOR

        public override void RenderGUI(EditorWindow editorWindow)
        {
            base.RenderGUI(editorWindow);

            GUILayout.Label("Texture Manager Settings", EditorStyles.boldLabel);
            GUILayout.Space(10);


            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(editorWindow.position.width), GUILayout.Height(editorWindow.position.height - 100));


            DrawVertexAndNormalTexSizes();

            GUILayout.Space(10);


            DrawDetailTextures();

            GUILayout.Space(200);
            EditorGUILayout.EndScrollView();
        }

        private void DrawVertexAndNormalTexSizes()
        {
            GUILayout.Label("Vertex and Normal Texture Sizes", EditorStyles.boldLabel);

            int vertCount = vertTexSizes.Count;
            int normalCount = normalTexSizes.Count;
            int newCount = Mathf.Max(vertCount, normalCount);

            AdjustListSize(vertTexSizes, newCount);
            AdjustListSize(normalTexSizes, newCount);


            for (int i = 0; i < newCount; i++)
            {
                GUILayout.BeginHorizontal();
                vertTexSizes[i] = EditorGUILayout.IntField($"Vert Tex Size {i}", vertTexSizes[i]);
                normalTexSizes[i] = EditorGUILayout.IntField($"Normal Tex Size {i}", normalTexSizes[i]);
                GUILayout.EndHorizontal();
            }
        }

        private void DrawDetailTextures()
        {
            GUILayout.Label("Detail Texture Settings", EditorStyles.boldLabel);


            int detailCount = detailTexSizes.Count;
            AdjustListSize(detailTexSizes, detailCount);
            AdjustListSize(detailTexRenderDistance, detailCount);
            AdjustListSize(detailTexMapSize, detailCount);
            AdjustListSize(detailTexMapVertEnable, detailCount);


            for (int i = 0; i < detailCount; i++)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label($"Detail Texture {i}", EditorStyles.boldLabel);
                detailTexSizes[i] = EditorGUILayout.IntField("Detail Tex Size", detailTexSizes[i]);
                detailTexRenderDistance[i] = EditorGUILayout.FloatField("Render Distance", detailTexRenderDistance[i]);
                detailTexMapSize[i] = EditorGUILayout.FloatField("Map Size", detailTexMapSize[i]);
                detailTexMapVertEnable[i] = EditorGUILayout.Toggle("Map Vert Enable", detailTexMapVertEnable[i]);
                GUILayout.EndVertical();
            }
        }
#endif
        private void AdjustListSize<T>(List<T> list, int size)
        {
            if (list == null)
            {
                list = new List<T>(size);
            }

            while (list.Count < size)
            {
                list.Add(default(T));
            }
            while (list.Count > size)
            {
                list.RemoveAt(list.Count - 1);
            }
        }
    }
}
