using HigWaterSystem2;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace HigWaterSystem2
{
    public class UnderWaterFeature : ScriptableRendererFeature
    {
        class UnderWaterPass : ScriptableRenderPass
        {

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                Camera camera = renderingData.cameraData.camera;
                UnderWaterRenderer underWaterRenderer = camera.GetComponent<UnderWaterRenderer>();
                if (underWaterRenderer != null && UnderWaterManager.Instance.renderType == UnderWaterManager.RenderType.RenderFeature)
                {
                    underWaterRenderer.PostUnderWaterCam(context, camera, renderingData.cameraData.renderer.cameraColorTargetHandle);
                }
            }


            public override void OnCameraCleanup(CommandBuffer cmd)
            {
            }
        }
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

        UnderWaterPass m_ScriptablePass;

        /// <inheritdoc/>
        public override void Create()
        {
            m_ScriptablePass = new UnderWaterPass();

            m_ScriptablePass.renderPassEvent = renderPassEvent;
        }


        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }

}
