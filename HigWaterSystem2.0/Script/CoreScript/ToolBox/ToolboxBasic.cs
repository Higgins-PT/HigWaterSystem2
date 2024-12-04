using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace HigWaterSystem2
{
    [System.Serializable]
    public class ToolboxBasic
    {
        public bool noNewToolBox = false;
        public void AddBasic(List<ToolboxBasic> toolboxBasics)
        {
            toolboxBasics.Add(this);
        }
        public virtual void InitData()
        {

        }

        public virtual void ReadData()
        {
            InitData();
        }
        public virtual void LoadData(ToolboxBasic toolboxBasic)
        {
            InitData();
        }
        public virtual void SaveData()
        {

        }
        public virtual void ApplyData()
        {

        }
#if UNITY_EDITOR
        public virtual void RenderGUI(EditorWindow editorWindow)
        {

        }
#endif
    }
}