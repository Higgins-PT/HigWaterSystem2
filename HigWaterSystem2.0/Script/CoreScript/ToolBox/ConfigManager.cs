
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using System.Reflection;

namespace HigWaterSystem2
{
    public class ConfigManager : HigSingleInstance<ConfigManager>
    {
        public Config config = new Config();
        public string configName = "config";
        private string configDirectoryPath;
        public List<string> configFiles = new List<string>();
        public Dictionary<Type, bool> toolBoxReadFilter = new Dictionary<Type, bool>();
        public void SaveConfig(string fileName)
        {
            foreach(var tool in config.toolboxBasics)
            {
                tool.noNewToolBox = true;
            }
            string filePath = Path.Combine(configDirectoryPath, fileName + ".json");
            string json = JsonUtility.ToJson(config, true);
            File.WriteAllText(filePath, json);
        }
        public void DeleteConfig(string fileName)
        {
            string filePath = Path.Combine(configDirectoryPath, fileName + ".json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        public void LoadConfig(string fileName)
        {
            string filePath = Path.Combine(configDirectoryPath, fileName + ".json");
            Config lastConfig = config;
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                config = JsonUtility.FromJson<Config>(json);
            }
            config.CopyConfig(lastConfig);
            ApplyConfiguration();
        }
        public void LoadConfig(string fileName, string subfolder)
        {
            string filePath = Path.Combine(Path.Combine(Application.streamingAssetsPath, "HigAdvancedWater2", subfolder), fileName + ".json");
            Config lastConfig = config;
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                config = JsonUtility.FromJson<Config>(json);
            }
            config.CopyConfig(lastConfig);
            ApplyConfiguration();
        }
        public void ReadConfiguration()
        {
            config.ReadData();
        }
        public void ApplyConfiguration()
        {
            config.InitToolBoxBasics();
            config.ApplyData(toolBoxReadFilter);
            HigSingleInstance<MonoBehaviour>.ResetAllSystem();
        }
        public override void ResetSystem()
        {
            configDirectoryPath = Path.Combine(Application.streamingAssetsPath, "HigAdvancedWater2", "Configs");
            Directory.CreateDirectory(configDirectoryPath);
            config.InitToolBoxBasics();
            configFiles = GetConfigFiles();
            AddToolBoxReadFliter();
        }
        public void AddToolBoxReadFliter()
        {
            var previousValues = new Dictionary<Type, bool>(toolBoxReadFilter);

            toolBoxReadFilter.Clear();
            var fields = config.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                if (typeof(ToolboxBasic).IsAssignableFrom(field.FieldType))
                {
                    var type = field.FieldType;
                    toolBoxReadFilter[type] = true;
                }
            }


            foreach (var previousEntry in previousValues)
            {
                if (toolBoxReadFilter.ContainsKey(previousEntry.Key))
                {
                    toolBoxReadFilter[previousEntry.Key] = previousEntry.Value;
                }
            }
        }
        public List<string> GetConfigFiles()
        {
            List<string> configFiles = new List<string>();
            string[] files = Directory.GetFiles(configDirectoryPath, "*.json");
            foreach (string file in files)
            {
                configFiles.Add(Path.GetFileNameWithoutExtension(file));
            }
            return configFiles;
        }
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}