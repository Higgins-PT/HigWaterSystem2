using HigWaterSystem2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HigWaterSystem2
{
    public class ViewManage : MonoBehaviour
    {
        // Public references to other components
        public CameraController controller;
        public CameraFollow cameraFollow;
        public BoatControl boatControl;

        private bool isFollowMode = true;
        void Start()
        {
            SetMode(isFollowMode);
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                isFollowMode = !isFollowMode;
                SetMode(isFollowMode);
            }
        }
        private void SetMode(bool followMode)
        {
            if (followMode)
            {
                if (cameraFollow != null) cameraFollow.enabled = true;
                if (boatControl != null) boatControl.control = true;
                if (controller != null) controller.enabled = false;
            }
            else
            {
                if (cameraFollow != null) cameraFollow.enabled = false;
                if (boatControl != null) boatControl.control = false;
                if (controller != null) controller.enabled = true;
            }
        }
    }
}