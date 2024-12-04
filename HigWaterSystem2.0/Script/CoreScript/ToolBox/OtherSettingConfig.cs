using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace HigWaterSystem2
{
    [System.Serializable]
    public class OtherSettingConfig : ToolboxBasic
    {
        public int cameraRendererIndex = 0;
        public LayerMask waterLayerMask;
        public LayerMask allowedLayers;


        private Vector2 scrollPosition = Vector2.zero;


        public override void ReadData()
        {
            base.ReadData();

            OtherSettingManager manager = OtherSettingManager.Instance;
            if (manager == null)
            {
                Debug.LogError("OtherSettingManager instance is null.");
                return;
            }

            cameraRendererIndex = manager.cameraRendererIndex;
            waterLayerMask = manager.waterLayerMask;
            allowedLayers = manager.allowedLayers;
        }

        public override void ApplyData()
        {
            base.ApplyData();

            OtherSettingManager manager = OtherSettingManager.Instance;
            if (manager == null)
            {
                Debug.LogError("OtherSettingManager instance is null.");
                return;
            }

            manager.cameraRendererIndex = cameraRendererIndex;
            manager.waterLayerMask = waterLayerMask;
            manager.allowedLayers = allowedLayers;
        }

#if UNITY_EDITOR
        public override void RenderGUI(EditorWindow editorWindow)
        {
            base.RenderGUI(editorWindow);

            GUILayout.Label("Other Settings", EditorStyles.boldLabel);
            GUILayout.Space(10);


            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(editorWindow.position.width), GUILayout.Height(editorWindow.position.height - 100));

            cameraRendererIndex = EditorGUILayout.IntField("Camera Renderer Index", cameraRendererIndex);
            waterLayerMask = LayerMaskField("Water Layer Mask", waterLayerMask);
            allowedLayers = LayerMaskField("Allowed Layers", allowedLayers);

            GUILayout.Space(200);
            EditorGUILayout.EndScrollView();
        }

        private LayerMask LayerMaskField(string label, LayerMask selected)
        {
            var layers = UnityEditorInternal.InternalEditorUtility.layers;
            int mask = 0;
            for (int i = 0; i < layers.Length; i++)
            {
                int layer = LayerMask.NameToLayer(layers[i]);
                if (((1 << layer) & selected.value) != 0)
                    mask |= (1 << i);
            }
            mask = EditorGUILayout.MaskField(label, mask, layers);
            int newMask = 0;
            for (int i = 0; i < layers.Length; i++)
            {
                if ((mask & (1 << i)) != 0)
                    newMask |= (1 << LayerMask.NameToLayer(layers[i]));
            }
            selected.value = newMask;
            return selected;
        }
#endif
    }
}
