using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class DrawCircle : MonoBehaviour
    {
        public float radius = 5f;
        public float createAttenuate = 1f;
        public float timeScale = 1f;

        public float waveLength = 10f;
        public float amplitude = 1.0f;
        float kelvinAngle = 19.47f * Mathf.Deg2Rad;

        [HideInInspector]
        public Vector3 lastPos;
        private void Update()
        {
            float speed = (transform.position - lastPos).magnitude;
            lastPos = transform.position;

            float height = ShipWave.CalculateShipWave(speed * Time.deltaTime, waveLength, amplitude, kelvinAngle, transform.position.x, transform.position.z, Time.time * timeScale);

            InteractiveWaterManager.Instance.AddDrawRequest(new DrawCircleRequest(this.transform.position, radius, height, createAttenuate));
        }

    }
}