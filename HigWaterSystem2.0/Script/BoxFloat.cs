using HigWaterSystem2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class BoxFloat : MonoBehaviour
    {

        // Start is called before the first frame update
        void Start()
        {

        }
        public float GetWaterHeight()
        {
            return OceanPhysics.Instance.GetOceanHeight(transform.position);
        }
        private void FixedUpdate()
        {
            Vector3 pos = transform.position;
            pos.y = GetWaterHeight();
            transform.position = pos;
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}