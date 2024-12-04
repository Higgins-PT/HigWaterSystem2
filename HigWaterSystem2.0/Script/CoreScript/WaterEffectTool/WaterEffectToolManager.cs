using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class WaterEffectToolManager : HigSingleInstance<WaterEffectToolManager>
    {
        public Shader riverFlowShader;
        public Shader riverHeightShader;
        public Shader riverWaterScaleShader;
        public Shader nearshoreWaveHeightShader;
        public Shader waterFlowShader;
        [HideInInspector]
        public Material flowDrawMat;
        public const float flowDrawWidthScale = 0.05f;
        public Color FlowDrawColor { get { return Color.cyan; } }
        public override void ResetSystem()
        {
            base.ResetSystem();
            waterFlowShader = Shader.Find("Unlit/WaterFlowShader");
            flowDrawMat = new Material(waterFlowShader);
        }
    }
}