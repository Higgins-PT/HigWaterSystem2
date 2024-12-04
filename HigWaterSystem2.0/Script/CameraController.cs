using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.DebugUI;

namespace HigWaterSystem2
{
    public class CameraController : MonoBehaviour
    {
        public float moveSpeed = 5.0f;
        public float rotationSpeed = 5.0f;
        public float zoomSpeed = 2.0f;
        public Vector3 spaceSize = new Vector3(50, 50, 50); 
        public float speedMultiplierScale = 4f;
        private Vector3 originalPosition;
        private Quaternion originalRotation;

        void Start()
        {
            originalPosition = transform.localPosition;
            originalRotation = transform.rotation;

        }

        void Update()
        {

            MoveCamera();
            RotateCamera();
            //ZoomCamera();
            ClampPosition();
        }

        void MoveCamera()
        {
            float speedMultiplier = 1f; 
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                speedMultiplier = speedMultiplierScale;
            }

            float xMove = Input.GetAxis("Horizontal") * moveSpeed * speedMultiplier * Time.deltaTime;
            float zMove = Input.GetAxis("Vertical") * moveSpeed * speedMultiplier * Time.deltaTime;
            transform.Translate(new Vector3(xMove, 0, zMove));
        }


        void RotateCamera()
        {
            if (Input.GetMouseButton(1))
            {
                float yRotation = Input.GetAxis("Mouse X") * rotationSpeed;
                float xRotation = Input.GetAxis("Mouse Y") * rotationSpeed;
                transform.localEulerAngles += new Vector3(-xRotation, yRotation, 0);
            }
        }

        void ZoomCamera()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return; 
            }
            float scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            transform.Translate(0, 0, scroll, Space.Self);
        }

        void ClampPosition()
        {
            transform.localPosition = new Vector3(
                Mathf.Clamp(transform.localPosition.x, originalPosition.x - spaceSize.x / 2, originalPosition.x + spaceSize.x / 2),
                Mathf.Clamp(transform.localPosition.y, originalPosition.y - spaceSize.y / 2, originalPosition.y + spaceSize.y / 2),
                Mathf.Clamp(transform.localPosition.z, originalPosition.z - spaceSize.z / 2, originalPosition.z + spaceSize.z / 2));
        }
    }
}