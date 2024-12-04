using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;


namespace HigWaterSystem2
{
    public interface ISimManager
    {
        void UpdateSimTex(CommandBuffer commandBuffer);
    }
    public class WaterSimulationManager : HigSingleInstance<WaterSimulationManager>
    {
        public CommandBuffer cmd_Reset;
        public CommandBuffer cmd_WaterBase;
        public CommandBuffer cmd_CameraMap;
        public CommandBuffer cmd_Probe;
        public CommandBuffer cmd_UnderWater;
        public CommandBuffer cmd_InteractiveWave;
        public void InitBuffer(ref CommandBuffer cmd, string name)
        {
            cmd = new CommandBuffer();
            cmd.name = name;
        }
        private void UpdateWaterSim()
        {

            IFFTManager.Instance.UpdateSimTex(cmd_WaterBase);
            InteractiveWaterManager.Instance.UpdateAllSimTex(cmd_InteractiveWave);
            Profiler.BeginSample("WaterBase");
            Graphics.ExecuteCommandBuffer(cmd_WaterBase);
            Profiler.EndSample();
            Profiler.BeginSample("InteractiveWave");
            Graphics.ExecuteCommandBuffer(cmd_InteractiveWave);
            Profiler.EndSample();

            CameraMapManager.Instance.RenderWaveData(cmd_CameraMap);


            WaveHeightProbeManager.Instance.UpdateAllProbe(cmd_Probe);

            Profiler.BeginSample("CameraMap");
            Graphics.ExecuteCommandBuffer(cmd_CameraMap);
            Profiler.EndSample();
            Profiler.BeginSample("Probe");
            Graphics.ExecuteCommandBuffer(cmd_Probe);
            Profiler.EndSample();
        }
        public override void ResetSystem()
        {
            cmd_Reset?.Release();
            cmd_WaterBase?.Release();
            cmd_CameraMap?.Release();
            cmd_Probe?.Release();
            cmd_UnderWater?.Release();
            cmd_InteractiveWave?.Release();
            InitBuffer(ref cmd_Reset, "Reset");
            InitBuffer(ref cmd_WaterBase, "WaterBase");
            InitBuffer(ref cmd_CameraMap, "CameraMap");
            InitBuffer(ref cmd_Probe, "Probe");
            InitBuffer(ref cmd_UnderWater, "RiverGenerator");
            InitBuffer(ref cmd_InteractiveWave, "InteractiveWave");

            foreach (var tex in TextureManager.Instance.vertTexture)
            {
                RTFunction.Instance.ResetValue(cmd_Reset, tex, new Vector4(0, 0, 0, 0));

            }
            foreach (var tex in TextureManager.Instance.normalTexture)
            {
                RTFunction.Instance.ResetValue(cmd_Reset, tex, new Vector4(0, 1, 0, 0));

            }

            foreach (var tex in TextureManager.Instance.detailTexture)
            {
                RTFunction.Instance.ResetValue(cmd_Reset, tex, new Vector4(0, 1, 0, 0));

            }
            foreach (var tex in TextureManager.Instance.detailTexture_Vert)
            {
                RTFunction.Instance.ResetValue(cmd_Reset, tex, new Vector4(0, 0, 0, 0));

            }
            foreach (var tex in TextureManager.Instance.foamTexture)
            {
                RTFunction.Instance.ResetValue(cmd_Reset, tex, new Vector4(0, 0, 0, 0));

            }
            Graphics.ExecuteCommandBuffer(cmd_Reset);
        }
        public override void HigUpdate()
        {
            ResetSystem();
            UpdateWaterSim();
        }
        public void Update()
        {

            
        }
    }
}