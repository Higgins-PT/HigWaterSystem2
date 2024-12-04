using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    [System.Serializable]
    public class ExampleBase : MonoBehaviour
    {
        public string Name = "Example";
        public Sprite Icon = null;
        public ConfigLoad configLoad;
        public virtual void Open()
        {
            configLoad.Load();
            gameObject.SetActive(true);
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);
        }
    }
}