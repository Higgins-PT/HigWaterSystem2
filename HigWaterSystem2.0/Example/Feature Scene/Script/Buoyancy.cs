using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class Buoyancy : MonoBehaviour
    {
        Rigidbody rigidbodyB;
        public float maxForce;
        public float depth;
        public float flowForce;

        void Start()
        {

        }

        public float GetWaterHeight()
        {
            return OceanPhysics.Instance.GetOceanHeight(transform.position);
        }
        public float GetForce()
        {
            float height = GetWaterHeight();
            return Mathf.Clamp((height - transform.position.y) / depth, 0, 1) * maxForce;
        }
        private void FixedUpdate()
        {
            if (rigidbodyB == null)
            {
                rigidbodyB = GetComponentInParent<Rigidbody>();
            }
            Vector3 force = new Vector3(0, GetForce(), 0);
            if (flowForce != 0)
            {
                force += OceanPhysics.Instance.GetProbeFlow(transform.position) * flowForce;
            }
            rigidbodyB.AddForceAtPosition(force, transform.position);
        }

        void Update()
        {

        }
    }
}