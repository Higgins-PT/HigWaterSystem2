using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class BuoyancyExample : ExampleBase
    {
        public Camera originCam;
        public Camera targetCam;
        public override void Open()
        {
            base.Open();
            originCam.gameObject.SetActive(false);
            targetCam.gameObject.SetActive(true);
        }
        public override void Close()
        {
            base.Close();
            originCam.gameObject.SetActive(true);
            targetCam.gameObject.SetActive(false);
        }
    }
}