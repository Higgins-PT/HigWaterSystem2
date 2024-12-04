using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class HideUI : MonoBehaviour
    {
        public List<GameObject> uiOBJ = new List<GameObject>();
        public bool enable = true;
        void Start()
        {

        }
        public void SetAction(bool enable)
        {
            foreach(GameObject obj in uiOBJ)
            {
                obj.SetActive(enable);
            }
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                enable = !enable;
                SetAction(enable);
            }
        }
    }
}