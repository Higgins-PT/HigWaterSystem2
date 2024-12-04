using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HigWaterSystem2
{
    public class DrawBoatWakeRequest : IWaterSimDraw
    {
        public Vector3 centerPos;
        public float radius;
        public float height;
        public Vector3 direction;

        public float groupWaveWidthPow;
        public float groupWaveWidthLimit;
        public float groupWaveLengthLimit;
        public float groupWaveLengthPow;
        public static readonly int sp_NowTex = Shader.PropertyToID("_NowTex");
        public static readonly int sp_LastTex = Shader.PropertyToID("_LastTex");
        public static readonly int sp_TexSize = Shader.PropertyToID("_TexSize");
        public static readonly int sp_SimSize = Shader.PropertyToID("_SimSize");    
        public static readonly int sp_PositionOffest = Shader.PropertyToID("_PositionOffest");
        public static readonly int sp_BoatDir = Shader.PropertyToID("_BoatDir");
        public static readonly int sp_Radius = Shader.PropertyToID("_Radius");
        public static readonly int sp_Height = Shader.PropertyToID("_Height");
        public static readonly int sp_CreateAttenuate = Shader.PropertyToID("_CreateAttenuate");

        public static readonly int sp_GroupWaveWidthPow = Shader.PropertyToID("_GroupWaveWidthPow");
        public static readonly int sp_GroupWaveWidthLimit = Shader.PropertyToID("_GroupWaveWidthLimit");
        public static readonly int sp_GroupWaveLengthLimit = Shader.PropertyToID("_GroupWaveLengthLimit");
        public static readonly int sp_GroupWaveLengthPow = Shader.PropertyToID("_GroupWaveLengthPow");

        public void SetParameters(CommandBuffer cmd, ComputeShader interactiveCS, RenderTexture nowTex, RenderTexture lastTex, float texSize, float simSize, Vector3 positionOffset, float radius, float height, Vector3 direction)
        {

            cmd.SetComputeTextureParam(interactiveCS, InteractiveWaterManager.Instance.ke_DrawBoatWake, sp_NowTex, nowTex);
            cmd.SetComputeTextureParam(interactiveCS, InteractiveWaterManager.Instance.ke_DrawBoatWake, sp_LastTex, lastTex);

            cmd.SetComputeFloatParam(interactiveCS, sp_TexSize, texSize);
            cmd.SetComputeFloatParam(interactiveCS, sp_SimSize, simSize);
            cmd.SetComputeVectorParam(interactiveCS, sp_PositionOffest, positionOffset);
            cmd.SetComputeVectorParam(interactiveCS, sp_BoatDir, direction);
            cmd.SetComputeFloatParam(interactiveCS, sp_Radius, radius);
            cmd.SetComputeFloatParam(interactiveCS, sp_Height, height);

            cmd.SetComputeFloatParam(interactiveCS, sp_GroupWaveWidthPow, groupWaveWidthPow);
            cmd.SetComputeFloatParam(interactiveCS, sp_GroupWaveWidthLimit, groupWaveWidthLimit);
            cmd.SetComputeFloatParam(interactiveCS, sp_GroupWaveLengthLimit, groupWaveLengthLimit);
            cmd.SetComputeFloatParam(interactiveCS, sp_GroupWaveLengthPow, groupWaveLengthPow);


            int dispatchSize = Mathf.CeilToInt(texSize / 8.0f);
            cmd.DispatchCompute(interactiveCS, InteractiveWaterManager.Instance.ke_DrawBoatWake, dispatchSize, dispatchSize, 1);
        }
        public bool CheckInPlane(Vector3 lodPos, float simSize)
        {

            float squareCircumscribedRadius = simSize / Mathf.Sqrt(2);
            float distance = Vector2.Distance(new Vector2(lodPos.x, lodPos.z), new Vector2(centerPos.x, centerPos.z));
            bool isOverlapping = distance <= (squareCircumscribedRadius + radius);

            return isOverlapping;


        }

        public void Draw(CommandBuffer cmd, InteractiveSim sim)
        {
            SetParameters(cmd, InteractiveWaterManager.Instance.interactiveDrawCS, sim.simWaterTex, sim.simWaterTexLast, sim.simWaterTex.width, sim.simSize, centerPos - sim.simPos, radius, height, direction);
        }
        public DrawBoatWakeRequest(Vector3 centerPos, float radius, float height, Vector3 direction,
                                   float groupWaveWidthPow, float groupWaveWidthLimit, float groupWaveLengthLimit, float groupWaveLengthPow)
        {
            this.centerPos = centerPos;
            this.radius = radius;
            this.height = height;

            this.direction = direction;

            this.groupWaveWidthPow = groupWaveWidthPow;
            this.groupWaveWidthLimit = groupWaveWidthLimit;
            this.groupWaveLengthLimit = groupWaveLengthLimit;
            this.groupWaveLengthPow = groupWaveLengthPow;
        }
    }
}

