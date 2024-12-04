using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class NearshoreWave : MonoBehaviour
    {
        public float waveAmplitude = 1.0f;
        public float waveFrequency = 1.0f;
        public float waveSpeed = 1.0f;
        public float waveGamma = 1.0f;
        public Vector4 oceanData = new Vector4(1, 1, 1, 1);
        public GameObject nearshoreHeight;
        public GameObject nearshoreOceanData;
        public static T GetComponentInCheck<T>(GameObject gameObject) where T : Component
        {
            T t = gameObject.GetComponent<T>();
            if (t == null)
            {
                return gameObject.AddComponent<T>();
            }
            else
            {
                return t;
            }
        }
        public void SpawnRiverMap()
        {

            if (nearshoreHeight != null)
            {
                DestroyImmediate(nearshoreHeight);
            }
            nearshoreHeight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nearshoreHeight.transform.parent = transform;
            nearshoreHeight.transform.localPosition = Vector3.zero;
            nearshoreHeight.transform.localScale = Vector3.one;
            MeshRenderer heightMeshRenderer = GetComponentInCheck<MeshRenderer>(nearshoreHeight);
            Material riverHeightMat = new Material(WaterEffectToolManager.Instance.riverFlowShader);
            riverHeightMat.SetFloat("_WaveAmplitude", waveAmplitude);
            riverHeightMat.SetFloat("_WaveFrequency", waveFrequency);
            riverHeightMat.SetFloat("_WaveSpeed", waveSpeed);
            riverHeightMat.SetFloat("_WaveGamma", waveGamma);
            heightMeshRenderer.sharedMaterial = riverHeightMat;
            heightMeshRenderer.enabled = false;



        }
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}