using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HigWaterSystem2
{

    public class GetCamHeight : MonoBehaviour
    {
        public WaveHeightProbe WaveHeightProbe;
        private int frame;
        float lastHeight = 0;
        // Start is called before the first frame update
        void Start()
        {
            frame = 0;
        }

        // Update is called once per frame
        void Update()
        {
            if (WaveHeightProbe != null)
            {
                frame++;
                if (frame > 2)
                {
                    float height = OceanPhysics.Instance.GetOceanHeightVariation(WaterPlaneControl.Instance.MainCamera.transform.position);
                    height = Mathf.Lerp(lastHeight, height, 0.4f);
                    WaterPlaneControl.Instance.cameraHeightOffest = height;
                    lastHeight = height;
                }
            }
        }
    }
}