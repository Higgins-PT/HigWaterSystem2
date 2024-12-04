using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class BoatControl : MonoBehaviour
    {
        private float[] forwardForces = new float[] { 0.3f, 0.6f, 1f }; 
        private float reverseForce = 0.3f; 
        public float forwardForce = 10f;  
        public float turnTorque = 5f;   
        public Rigidbody rb;           
        private int currentGear = 0;
        public bool control = true;
        void Start()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }
        }

        void FixedUpdate()
        {
            HandleMovement();
            HandleRotation();
        }
        private void Update()
        {

        }
        void ShiftUp()
        {
            if (currentGear < forwardForces.Length)
            {
                currentGear++;
            }
        }

        void ShiftDown()
        {
            if (currentGear > -1)
            {
                currentGear--;
            }
        }

        void HandleMovement()
        {
            if (control)
            {
                if (Input.GetKeyDown(KeyCode.W))
                {
                    ShiftUp();
                }
                if (Input.GetKeyDown(KeyCode.S))
                {
                    ShiftDown();
                }
            }

            if (currentGear > 0)
            {
                rb.AddForce(transform.forward * forwardForce * forwardForces[currentGear - 1], ForceMode.Force);
            }
            else if (currentGear < 0)
            {
                rb.AddForce(transform.forward * forwardForce * reverseForce, ForceMode.Force);
            }
        }

        void HandleRotation()
        {
            float turnInput = Input.GetAxis("Horizontal");
            if (!control)
            {
                return;
            }
            if (turnInput != 0)
            {
                rb.AddTorque(transform.up * turnTorque * turnInput, ForceMode.Force);
            }
        }
    }
}