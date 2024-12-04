using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HigWaterSystem2
{
    public class DrawCircleRequest : IWaterSimDraw
    {
        public Vector3 centerPos;
        public float radius;
        public float height;
        public float createAttenuate;
        public static readonly int sp_NowTex = Shader.PropertyToID("_NowTex");
        public static readonly int sp_LastTex = Shader.PropertyToID("_LastTex");
        public static readonly int sp_TexSize = Shader.PropertyToID("_TexSize");
        public static readonly int sp_SimSize = Shader.PropertyToID("_SimSize");
        public static readonly int sp_PositionOffest = Shader.PropertyToID("_PositionOffest");
        public static readonly int sp_Radius = Shader.PropertyToID("_Radius");
        public static readonly int sp_Height = Shader.PropertyToID("_Height");
        public static readonly int sp_CreateAttenuate = Shader.PropertyToID("_CreateAttenuate");
        public void SetParameters(CommandBuffer cmd, ComputeShader interactiveCS, RenderTexture nowTex, RenderTexture lastTex, float texSize, float simSize, Vector3 positionOffset, float radius, float height, float createAttenuate)
        {

            cmd.SetComputeTextureParam(interactiveCS, InteractiveWaterManager.Instance.ke_DrawSquareCircle, sp_NowTex, nowTex);
            cmd.SetComputeTextureParam(interactiveCS, InteractiveWaterManager.Instance.ke_DrawSquareCircle, sp_LastTex, lastTex);

            cmd.SetComputeFloatParam(interactiveCS, sp_TexSize, texSize);
            cmd.SetComputeFloatParam(interactiveCS, sp_SimSize, simSize);
            cmd.SetComputeVectorParam(interactiveCS, sp_PositionOffest, positionOffset);
            cmd.SetComputeFloatParam(interactiveCS, sp_Radius, radius);
            cmd.SetComputeFloatParam(interactiveCS, sp_Height, height);
            cmd.SetComputeFloatParam(interactiveCS, sp_CreateAttenuate, createAttenuate);

            int dispatchSize = Mathf.CeilToInt(texSize / 8.0f);
            cmd.DispatchCompute(interactiveCS, InteractiveWaterManager.Instance.ke_DrawSquareCircle, dispatchSize, dispatchSize, 1);
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
            SetParameters(cmd, InteractiveWaterManager.Instance.interactiveDrawCS, sim.simWaterTex, sim.simWaterTexLast, sim.simWaterTex.width, sim.simSize, centerPos - sim.simPos, radius, height, createAttenuate);
        }
        public DrawCircleRequest(Vector3 centerPos, float radius, float height, float createAttenuate)
        {
            this.centerPos = centerPos;
            this.radius = radius;
            this.height = height;
            this.createAttenuate = createAttenuate;
        }
    }
}
