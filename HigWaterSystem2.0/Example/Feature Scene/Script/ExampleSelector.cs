using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HigWaterSystem2
{
    public class ExampleSelector : MonoBehaviour
    {
        public ExampleManager exampleManager;
        public GameObject examplePrefab;
        public RectTransform contentArea;

        private List<GameObject> exampleItems = new List<GameObject>();

        private void Start()
        {
            GenerateExampleItems();
        }


        private void GenerateExampleItems()
        {
            foreach (var example in exampleManager.examples)
            {
                GameObject item = Instantiate(examplePrefab, contentArea);
                item.SetActive(true);
                exampleItems.Add(item);

                Image image = item.transform.Find("ButtonImage/Image").GetComponent<Image>();
                Text text = item.transform.Find("BackText/Text").GetComponent<Text>();

                image.sprite = example.Icon;
                text.text = example.Name;

                Button button = item.transform.Find("ButtonImage").GetComponent<Button>();
                int index = exampleItems.Count - 1;
                button.onClick.AddListener(() => OnExampleSelected(index));
            }
        }

        private void OnExampleSelected(int index)
        {
            exampleManager.SwitchExample(index);
        }


    }
}
