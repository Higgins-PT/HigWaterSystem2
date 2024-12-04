using HigWaterSystem2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class BoxRun : MonoBehaviour
    {
        public Vector3 initPos;
        public float timeScale = 0.1f;
        public float size = 20f;
        void Start()
        {
            initPos = transform.position;
        }

        void Update()
        {
            Vector3 newPos = initPos + size * new Vector3(Mathf.Sin(Time.time * timeScale), 0, Mathf.Cos(Time.time * timeScale));
            newPos.y = OceanPhysics.Instance.GetOceanHeight(newPos);
            transform.position = newPos;
        }
    }
}