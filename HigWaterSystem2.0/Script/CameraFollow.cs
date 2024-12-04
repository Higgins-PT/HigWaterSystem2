using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HigWaterSystem2
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;      
        public Vector3 localOffset;        
        public float smoothSpeed = 0.125f;
        public float rotationSpeed = 5f;

        private Vector3 currentOffset;

        void Start()
        {
            if (target == null)
            {
                Debug.LogError("Target not set for CameraFollow script.");
                return;
            }

            // 初始化当前偏移量
            currentOffset = target.TransformPoint(localOffset) - target.position;
        }

        void FixedUpdate()
        {
            if (target == null)
            {
                return;
            }

            HandleMouseRotation();
            HandleMovement();
        }

        void HandleMouseRotation()
        {
            // 如果按下鼠标右键
            if (Input.GetMouseButton(1))
            {
                float horizontal = Input.GetAxis("Mouse X") * rotationSpeed;
                float vertical = -Input.GetAxis("Mouse Y") * rotationSpeed;

                Quaternion camTurnAngle = Quaternion.AngleAxis(horizontal, Vector3.up);
                currentOffset = camTurnAngle * currentOffset;


            }
        }

        void HandleMovement()
        {

            Vector3 desiredPosition = target.position + currentOffset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            Vector3 directionToLook = target.position + target.forward * 10f - transform.position; 
            directionToLook.y = 0; 
            if (directionToLook != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToLook);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed);
            }
        }
    }
}