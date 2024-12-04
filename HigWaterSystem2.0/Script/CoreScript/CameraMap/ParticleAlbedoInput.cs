using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class ParticleAlbedoInput : AlbedoDataInput
    {
        public override Renderer GetRenderer()
        {
            return gameObject.GetComponent<ParticleSystem>().GetComponent<ParticleSystemRenderer>();
        }
        public override Material GetMat()
        {
            return gameObject.GetComponent<ParticleSystem>().GetComponent<ParticleSystemRenderer>().material;

        }
    }
}