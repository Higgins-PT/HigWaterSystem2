using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class ComplexBuoyancy : MonoBehaviour
    {
        Rigidbody mainObjRigidbody;
        public float maxForce;
        public float depth;
        public float flowForce;
        public float maxTorque = 10f;
        public float alignmentThreshold = 0.1f;
        public float normalGetDelta = 1f;
        public bool enableNormalForce = false;
        void Start()
        {

        }
        public Vector3 GetWaterNormal()
        {
            return OceanPhysics.Instance.GetOceanNormal(transform.position, normalGetDelta);
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
        public Vector3 CalculateRotatedForce(Vector3 rotatedNormal, Vector3 defaultForce)
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, rotatedNormal);
            Vector3 rotatedForce = rotation * defaultForce;

            return rotatedForce;
        }
        public void ApplyRotationForceTowardsNormal(Vector3 targetNormal)
        {
            Vector3 currentUp = transform.up;
            float angle = Vector3.Angle(currentUp, targetNormal);
            Vector3 rotationAxis = Vector3.Cross(currentUp, targetNormal);
            if (angle < alignmentThreshold)
                return;

            float torqueMagnitude = Mathf.Lerp(maxTorque, 0, angle / 180f);
            Vector3 torque = rotationAxis.normalized * torqueMagnitude;
            mainObjRigidbody.AddTorque(torque, ForceMode.Force);
        }
        private void FixedUpdate()
        {
            if (mainObjRigidbody == null)
            {
                mainObjRigidbody = GetComponentInParent<Rigidbody>();
            }
            Vector3 normal = GetWaterNormal();
            Vector3 force = new Vector3(0, GetForce(), 0);
            if (flowForce != 0)
            {
                force += OceanPhysics.Instance.GetProbeFlow(transform.position) * flowForce;
            }
            if (enableNormalForce)
            {
                force = CalculateRotatedForce(normal, force);
            }
            ApplyRotationForceTowardsNormal(normal);
            mainObjRigidbody.AddForceAtPosition(force, transform.position);
        }

        void Update()
        {

        }
    }
}