using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;


using UnityEngine;
using UnityEngine.SceneManagement;

namespace HigWaterSystem2
{
    [InitializeOnLoad]
    public class SceneSaveHandler : MonoBehaviour
    {
        static SceneSaveHandler()
        {
            EditorSceneManager.sceneSaved += OnSceneSaved;
        }

        private static void OnSceneSaved(Scene scene)
        {
            HigSingleInstance<MonoBehaviour>.ResetAllSystem();
        }
    }
}
#endif