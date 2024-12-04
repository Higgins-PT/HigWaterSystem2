using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HigWaterSystem2
{
    [System.Serializable]
    public class NoiseSettings
    {
        public int texSize = 512;
        public float timeScale = 2f;
        public float frequency = 1f;
        public float normalScale = 1f;
        public int octaves = 4;
        public float amplitude = 1f;
        public List<LodChannel> lodChannels = new List<LodChannel>();
    }
    public class NoiseManager : WaterSimulationBase<NoiseManager> , ISimManager
    {
        public ComputeShader noiseShader;
        public Texture2D noiseTex;
        public NoiseSettings noiseSettings;
        public List<SimTexBase> simTexBases = new List<SimTexBase>();
        public static readonly int sp_Result_Normal = Shader.PropertyToID("_Result_Normal");
        public static readonly int sp_Result_Vert = Shader.PropertyToID("_Result_Vert");
        public static readonly int sp_NoiseTex = Shader.PropertyToID("_NoiseTex");
        public static readonly int sp_NoiseTexSize = Shader.PropertyToID("_NoiseTexSize");
        public static readonly int sp_ResultTexSize = Shader.PropertyToID("_ResultTexSize");
        public static readonly int sp_Frequency = Shader.PropertyToID("_Frequency");
        public static readonly int sp_NormalScale = Shader.PropertyToID("_NormalScale");
        public static readonly int sp_Octaves = Shader.PropertyToID("_Octaves");
        public static readonly int sp_Amplitude = Shader.PropertyToID("_Amplitude");
        public static int ke_SpawnPerlinNoiseBase;
        public static int ke_SpawnPerlinNoiseFin;
        public override void ResetSystem()
        {
            ke_SpawnPerlinNoiseBase = noiseShader.FindKernel("SpawnPerlinNoiseBase");
            ke_SpawnPerlinNoiseFin = noiseShader.FindKernel("SpawnPerlinNoiseFin");
            InitT0Tex();
        }
        public void InitT0Tex()
        {
            simTexBases.Clear();
            foreach (var baseTex in OceanWaveformBase.Instance.simTexBases)
            {
                if (baseTex.texType == SimTexBase.TexType.Noise)
                {
                    simTexBases.Add(baseTex);

                }
            }
        }
        public void SpawnNoise(CommandBuffer cmd, SimTexBase simTexBase, int index, out RenderTexture normal, out RenderTexture vert)
        {
            int size = simTexBase.noiseSettings.texSize;
            vert = RTManager.Instance.GetRT("noiseVert" + index.ToString(), simTexBase.noiseSettings.texSize, RenderTextureFormat.ARGBFloat);
            normal = RTManager.Instance.GetRT("noiseNormal" + index.ToString(), simTexBase.noiseSettings.texSize, RenderTextureFormat.ARGBFloat);
            cmd.SetComputeTextureParam(noiseShader, ke_SpawnPerlinNoiseBase, sp_Result_Vert, vert);
            cmd.SetComputeTextureParam(noiseShader, ke_SpawnPerlinNoiseFin, sp_Result_Vert, vert);
            cmd.SetComputeTextureParam(noiseShader, ke_SpawnPerlinNoiseFin, sp_Result_Normal, normal);
            cmd.SetComputeTextureParam(noiseShader, ke_SpawnPerlinNoiseBase, sp_NoiseTex, noiseTex);
            cmd.SetComputeIntParam(noiseShader, sp_NoiseTexSize, noiseTex.width);
            cmd.SetComputeIntParam(noiseShader, sp_ResultTexSize, size);
            cmd.SetComputeFloatParam(noiseShader, sp_Frequency, simTexBase.noiseSettings.frequency);
            cmd.SetComputeFloatParam(noiseShader, sp_NormalScale, simTexBase.noiseSettings.normalScale);
            cmd.SetComputeIntParam(noiseShader, sp_Octaves, simTexBase.noiseSettings.octaves);
            cmd.SetComputeFloatParam(noiseShader, sp_Amplitude, simTexBase.noiseSettings.amplitude);
            cmd.DispatchCompute(noiseShader, ke_SpawnPerlinNoiseBase, size / 8, size / 8, 1);
            cmd.DispatchCompute(noiseShader, ke_SpawnPerlinNoiseFin, size / 8, size / 8, 1);

        }
        public void UpdateSimTex(CommandBuffer commandBuffer)
        {
            for (int i = 0; i < simTexBases.Count; i++)
            {
                if (simTexBases[i].waterSimEnable)
                {
                    for (int j = 0; j < simTexBases[i].noiseSettings.lodChannels.Count; j++)
                    {
                        SpawnNoise(commandBuffer, simTexBases[i], i, out RenderTexture normal, out RenderTexture vert);
                        TextureManager.Instance.HandleSimResult(vert, normal, simTexBases[i].noiseSettings.lodChannels[j], simTexBases[i].noiseSettings.lodChannels[j].mappedSize, commandBuffer, WaterPlaneControl.Instance.GetPlanePos(), simTexBases[i].noiseSettings.lodChannels[j].vertScale, simTexBases[i].noiseSettings.lodChannels[j].normalScale);
                    }
                }
            }
        }
    }

}