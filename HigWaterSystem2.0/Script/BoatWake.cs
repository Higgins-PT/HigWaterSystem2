using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class BoatWake : MonoBehaviour
    {
        public float radius = 5f;
        public float timeScale = 1f;

        public float waveLength = 10f;
        public float amplitude = 1.0f;
        float kelvinAngle = 19.47f * Mathf.Deg2Rad;

        public float groupWaveWidthPow = 4;
        public float groupWaveWidthLimit = 0.3f;
        public float groupWaveLengthLimit = 0.1f;
        public float groupWaveLengthPow = 2f;
        [HideInInspector]
        public Vector3 lastPos;
        private void Start()
        {
            lastPos = transform.position;
        }
        private void Update()
        {
            Vector3 dir = transform.position - lastPos;
            float speed = (transform.position - lastPos).magnitude;
            lastPos = transform.position;
            float height = ShipWave.CalculateShipWave(speed * Time.deltaTime, waveLength, amplitude, kelvinAngle, transform.position.x, transform.position.z, Time.time * timeScale) * Mathf.Clamp(speed / 30, 0, 1);
            InteractiveWaterManager.Instance.AddDrawRequest(new DrawBoatWakeRequest(this.transform.position, radius, height, dir.normalized, groupWaveWidthPow, groupWaveWidthLimit, groupWaveLengthLimit, groupWaveLengthPow));
        }
    }
}