using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
namespace HigWaterSystem2
{
    public class ExampleManager : MonoBehaviour
    {

        public List<ExampleBase> examples = new List<ExampleBase>();
        private ExampleBase currentExample;

        public void Start()
        {
            SwitchExample(0);
        }
        public void AddExample(ExampleBase example)
        {
            if (!examples.Contains(example))
            {
                examples.Add(example);
            }
        }

        public void SwitchExample(int index)
        {
            if (index < 0 || index >= examples.Count)
            {
                Debug.LogWarning("Out of range");
                return;
            }

            if (currentExample != null)
            {
                currentExample.Close();
            }

            currentExample = examples[index];
            currentExample.Open();
        }
    }
}