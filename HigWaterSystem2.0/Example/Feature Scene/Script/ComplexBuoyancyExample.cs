using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class ComplexBuoyancyExample : ExampleBase
    {
        public override void Open()
        {
            base.Open();
            OceanPhysics.Instance.simpleGetHeight = false;
        }
        public override void Close()
        {
            base.Close();
            OceanPhysics.Instance.simpleGetHeight = true;
        }
    }
}