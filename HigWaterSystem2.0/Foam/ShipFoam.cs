using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class ShipFoam : MonoBehaviour
    {
        public ParticleSystem particleSystemWave; 
        [HideInInspector]
        public Vector3 lastPos;
        public Color slowColor = Color.black;  
        public Color fastColor = Color.white;    
        public float maxSpeed = 15f;           

        private ParticleSystem.MainModule mainModule; 

        void Start()
        {
            if (particleSystemWave == null)
            {
                particleSystemWave = GetComponent<ParticleSystem>();
            }

            lastPos = transform.position;
            mainModule = particleSystemWave.main;
        }

        void Update()
        {
            float speed = (transform.position - lastPos).magnitude / Time.deltaTime;
            lastPos = transform.position;

            Color currentColor = Color.Lerp(slowColor, fastColor, Mathf.Clamp(speed / maxSpeed, 0, 1));

            mainModule.startColor = currentColor;
        }
    }
}