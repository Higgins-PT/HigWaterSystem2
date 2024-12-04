using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace HigWaterSystem2
{
    [ExecuteInEditMode]
    public class ConfigLoad : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField]
        private List<ToolBoxReadFilterEntry> toolBoxReadFilterList = new List<ToolBoxReadFilterEntry>();
        public string configName = "config";
        public string subFolder = "Example";
        public Dictionary<Type, bool> toolBoxReadFilter = new Dictionary<Type, bool>();
        public void Load()
        {
            RefreshUI();
            DictionarySync.SyncDictionariesSafe(toolBoxReadFilter, ConfigManager.Instance.toolBoxReadFilter);
            ConfigManager.Instance.LoadConfig(configName, subFolder);
        }
        IEnumerator LateStart()
        {
            yield return new WaitForEndOfFrame();
            RefreshUI();
            DictionarySync.SyncDictionaries(ConfigManager.Instance.toolBoxReadFilter, toolBoxReadFilter);
            Refresh();
        }
        private void Start()
        {
            StartCoroutine(LateStart());
        }
        public void RefreshUI()
        {
            toolBoxReadFilter.Clear();
            foreach (var entry in toolBoxReadFilterList)
            {
                toolBoxReadFilter[entry.GetTypeFromName()] = entry.enable;
            }
        }
        public void Refresh()
        {
            toolBoxReadFilterList.Clear();
            foreach (var kvp in toolBoxReadFilter)
            {
                toolBoxReadFilterList.Add(new ToolBoxReadFilterEntry(kvp.Key, kvp.Value));
            }
        }
    }
    [System.Serializable]
    public struct ToolBoxReadFilterEntry
    {
        public string typeName;   
        public bool enable;
        public ToolBoxReadFilterEntry(Type type, bool enable)
        {
            typeName = "";
            this.enable = enable;
            SetType(type);

        }

        public Type GetTypeFromName()
        {
            return Type.GetType(typeName);
        }

        public void SetType(Type type)
        {
            typeName = type.AssemblyQualifiedName;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ConfigLoad))]
    public class ConfigLoadEditor : Editor
    {
        ConfigLoad configLoad;
        private void OnEnable()
        {
            configLoad = (ConfigLoad)target;
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (configLoad.toolBoxReadFilter.Count == 0)
            {
                configLoad.RefreshUI();
            }
            foreach (var tool in configLoad.toolBoxReadFilter.ToList())
            {
                bool value = EditorGUILayout.ToggleLeft(tool.Key.Name, tool.Value);
                if (value != configLoad.toolBoxReadFilter[tool.Key])
                {
                    configLoad.toolBoxReadFilter[tool.Key] = value;
                    configLoad.Refresh();
                }

            }
        }
    }
#endif
    public class DictionarySync
    {
        public static void SyncDictionariesSafe(Dictionary<Type, bool> source, Dictionary<Type, bool> target)
        {
            var sourceTypeNames = source.Keys.ToDictionary(type => type.FullName, type => type);
            var targetTypeNames = target.Keys.ToDictionary(type => type.FullName, type => type);
            foreach (var sourceEntry in source)
            {
                string typeName = sourceEntry.Key.FullName;

                if (targetTypeNames.ContainsKey(typeName))
                {

                    target[targetTypeNames[typeName]] = sourceEntry.Value;
                }
            }
        }
        public static void SyncDictionaries(Dictionary<Type, bool> source, Dictionary<Type, bool> target)
        {
            var sourceTypeNames = source.Keys.ToDictionary(type => type.FullName, type => type);
            var targetTypeNames = target.Keys.ToDictionary(type => type.FullName, type => type);

            
            foreach (var targetTypeName in targetTypeNames.Keys.ToList())
            {
                if (!sourceTypeNames.ContainsKey(targetTypeName))
                {
                    target.Remove(targetTypeNames[targetTypeName]);
                }
            }


            foreach (var sourceEntry in source)
            {
                string typeName = sourceEntry.Key.FullName;
                if (!targetTypeNames.ContainsKey(typeName))
                {
                    target[sourceTypeNames[typeName]] = sourceEntry.Value;
                }
            }
        }
    }
}