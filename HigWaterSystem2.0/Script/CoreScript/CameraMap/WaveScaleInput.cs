using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class WaveScaleInput : WaveScaleDataInput
    {
        public override Renderer GetRenderer()
        {
            return gameObject.GetComponent<MeshRenderer>();
        }
        public override Material GetMat()
        {
            return gameObject.GetComponent<MeshRenderer>().sharedMaterial;

        }
    }
}