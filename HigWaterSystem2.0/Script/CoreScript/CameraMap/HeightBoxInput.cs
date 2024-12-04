using HigWaterSystem2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HigWaterSystem2
{

    public class HeightBoxInput : HeightDataInput
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