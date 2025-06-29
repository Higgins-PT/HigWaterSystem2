using HigWaterSystem2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class BoxFloat : MonoBehaviour
    {

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
    }
}
