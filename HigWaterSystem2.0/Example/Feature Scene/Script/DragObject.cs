using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HigWaterSystem2
{
    public class DragObject : MonoBehaviour
    {
        private Rigidbody selectedRigidbody;
        private Vector3 offset;
        private float zDistance;
        private Vector3 lastPosition;
        private Vector3 velocity;

        void Update()
        {
            if (Input.GetMouseButtonDown(0)) 
            {
                StartDragging();
            }

            if (Input.GetMouseButton(0) && selectedRigidbody != null)
            {
                Drag();
            }

            if (Input.GetMouseButtonUp(0) && selectedRigidbody != null) 
            {
                ThrowObject();
            }
        }

        void StartDragging()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.rigidbody != null)
                {
                    selectedRigidbody = hit.rigidbody;
                    selectedRigidbody.isKinematic = true; 
                    zDistance = Vector3.Distance(Camera.main.transform.position, hit.point);
                    offset = selectedRigidbody.transform.position - hit.point;
                    lastPosition = selectedRigidbody.position; 
                }
            }
        }

        void Drag()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 targetPosition = ray.GetPoint(zDistance) + offset;
            selectedRigidbody.MovePosition(targetPosition);

            velocity = (selectedRigidbody.position - lastPosition) / Time.deltaTime;
            lastPosition = selectedRigidbody.position;
        }

        void ThrowObject()
        {
            selectedRigidbody.isKinematic = false; 
            selectedRigidbody.velocity = velocity; 
            selectedRigidbody = null;
        }
    }
}