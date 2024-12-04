using HigWaterSystem2;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace HigWaterSystem2
{
    public class FogEffect : ScriptableRendererFeature
    {
        class FogEffectPass : ScriptableRenderPass
        {
            public Setting m_Setting;
            public Material fogMat;
            private RenderTextureDescriptor temRTDescriptor;
            public RTHandle temRT;
            public int temRTID;
            public FogEffectPass(Setting setting, Shader shader)
            {
                m_Setting = setting;
                fogMat = CoreUtils.CreateEngineMaterial(shader);
            }
            public void GetID()
            {
                temRTID = Shader.PropertyToID("_FogTemRT");
            }
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                if (fogMat == null || !SSAOManager.Instance.enable) return;
                var desc = renderingData.cameraData.cameraTargetDescriptor;
                temRTDescriptor = new RenderTextureDescriptor(Mathf.CeilToInt(desc.width), Mathf.CeilToInt(desc.height), RenderTextureFormat.ARGBFloat, 0, 0);
                temRTDescriptor.msaaSamples = 1;
                temRTDescriptor.useMipMap = false;
                temRTDescriptor.colorFormat = renderingData.cameraData.cameraTargetDescriptor.colorFormat;
                temRTDescriptor.sRGB = false;
                RenderingUtils.ReAllocateIfNeeded(ref temRT, temRTDescriptor);

                ConfigureTarget(temRT);
                ConfigureClear(ClearFlag.None, Color.black);
            }

            // Here you can implement the rendering logic.
            // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
            // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
            // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (fogMat == null || !SSAOManager.Instance.enable) return;
                CommandBuffer cmd = CommandBufferPool.Get("Fog Post Pass");
                RTHandle cameraTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

                cmd.SetGlobalMatrix("_InverViewMat", renderingData.cameraData.camera.worldToCameraMatrix.inverse);
                cmd.SetGlobalMatrix("_InverProjMat", renderingData.cameraData.camera.projectionMatrix.inverse);
                cmd.SetGlobalFloat("_MaxFogDepth", m_Setting.distance);
                cmd.SetGlobalFloat("_MinFogDepth", m_Setting.startDistance);

                cmd.SetGlobalFloat("_Width", m_Setting.width);
                cmd.SetGlobalFloat("_FarClipPlane", renderingData.cameraData.camera.farClipPlane);
                cmd.SetGlobalFloat("_WidthPow", m_Setting.widthPow);
                cmd.SetGlobalFloat("_DepthPow", m_Setting.depthPow);

                cmd.SetGlobalColor("_FogColor", m_Setting.fogColor);
                if (cameraTarget.rt != null)
                {
                    cmd.Blit(cameraTarget, temRT);
                    cmd.Blit(temRT, cameraTarget, fogMat);
                    context.ExecuteCommandBuffer(cmd);
                }

                CommandBufferPool.Release(cmd);

            }
            public void Cleanup()
            {
                CoreUtils.Destroy(fogMat);
                temRT?.Release();
                temRT = null;
            }
            // Cleanup any allocated resources that were created during the execution of this render pass.
            public override void OnCameraCleanup(CommandBuffer cmd)
            {

            }
        }

        [System.Serializable]
        public class Setting
        {
            public Color fogColor = Color.white;
            public float distance = 10000f;
            public float startDistance = 3000f;
            [Range(0.000f, 1.000f)]
            public float width = 0.1f;
            public float widthPow = 0.1f;
            public float depthPow = 1f;

        }
        [HideInInspector]
        public Shader fogShader;
        FogEffectPass m_ScriptablePass;
        public Setting m_Setting;
        /// <inheritdoc/>
        public override void Create()
        {
            m_ScriptablePass = new FogEffectPass(m_Setting, fogShader);

            // Configures where the render pass should be injected.
            m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {

            m_ScriptablePass.GetID();
            renderer.EnqueuePass(m_ScriptablePass);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_ScriptablePass?.Cleanup();
            }
        }
    }


}