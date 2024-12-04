using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace HigWaterSystem2
{
    public class OtherSettingManager : HigSingleInstance<OtherSettingManager>
    {
        public int cameraRendererIndex = 0;
        public LayerMask waterLayerMask;
        public LayerMask allowedLayers;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}