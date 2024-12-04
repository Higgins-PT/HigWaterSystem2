using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HigWaterSystem2
{
    public class DebugSystem
    {
        public static void RenderTextureInfo()
        {
            TextureManager textureManager = TextureManager.Instance;

            if (textureManager == null) return;

            GUILayout.BeginVertical("box");
            GUILayout.Label("Vert Textures:");
            /*
            int lenght = (int)Mathf.Log(512, 2);
            T0SpecTex = RTManager.Instance.GetRT("butterflyTex", lenght, 512, RenderTextureFormat.ARGBFloat);*/

            //GUILayout.Label(T0SpecTex, GUILayout.Width(100), GUILayout.Height(100));
            foreach (var tex in textureManager.vertTexture)
            {
                if (tex != null)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label(tex, GUILayout.Width(100), GUILayout.Height(100));

                    GUILayout.BeginVertical();
                    GUILayout.Label(tex.name);
                    GUILayout.Label($"Resolution: {tex.width}x{tex.height}");
                    GUILayout.Label($"Format: {tex.format}");
                    GUILayout.EndVertical();

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.Space(10); 

            GUILayout.Label("Normal Textures:");

            foreach (var tex in textureManager.normalTexture)
            {
                if (tex != null)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label(tex, GUILayout.Width(100), GUILayout.Height(100)); 


                    GUILayout.BeginVertical();
                    GUILayout.Label(tex.name);
                    GUILayout.Label($"Resolution: {tex.width}x{tex.height}");
                    GUILayout.Label($"Format: {tex.format}");
                    GUILayout.EndVertical();

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
        }
    }
}