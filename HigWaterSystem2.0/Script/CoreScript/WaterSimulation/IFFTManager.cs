using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HigWaterSystem2
{
    [System.Serializable]
    public class IFFTSettings 
    {
        public int texSize = 512;
        public float oceanDepth = 50f;
        public float fetch = 100000;
        public float windSpeed = 30;
        public float windSpreadScale = 1f;
        public float gammar = 3.3f;
        public float scale = 2f;
        public float swellStrength = 0.9f;  
        public float windDir = 10f;
        public float timeScale = 2f;
        public float offest = 1f;
        public float foamThreshold = 0.00005f;
        public float foamIntensity = 1f;
        public float foamAttenuation = 0.9f;
        public float frequencyScale = 0.000001f;
        public float normalScale = 100f;
        public float vertScale = 1f;
        public bool showIFFTSettings = true;
        public List<LodChannel> lodChannels = new List<LodChannel>();
        public void DeepCopyIFFTSetting(IFFTSettings iFFTSettings)
        {
            texSize = iFFTSettings.texSize;
            oceanDepth = iFFTSettings.oceanDepth;
            fetch = iFFTSettings.fetch;
            windSpeed = iFFTSettings.windSpeed;
            windSpreadScale = iFFTSettings.windSpreadScale;
            gammar = iFFTSettings.gammar;
            scale = iFFTSettings.scale;
            swellStrength = iFFTSettings.swellStrength;
            windDir = iFFTSettings.windDir;
            timeScale = iFFTSettings.timeScale;
            offest = iFFTSettings.offest;
            foamIntensity = iFFTSettings.foamIntensity;
            foamAttenuation = iFFTSettings.foamAttenuation;
            foamThreshold = iFFTSettings.foamThreshold;
            frequencyScale = iFFTSettings.frequencyScale;
            normalScale = iFFTSettings.normalScale;
            vertScale = iFFTSettings.vertScale;
            lodChannels.Clear();
            foreach (LodChannel channel in iFFTSettings.lodChannels)
            {
                LodChannel lodChannel = new LodChannel();
                lodChannel.DeepCopy(channel);
                lodChannels.Add(lodChannel);
            }

        }
    }

    public class IFFTManager : WaterSimulationBase<IFFTManager> , ISimManager
    {
        public ComputeShader PhillCS;
        public ComputeShader IFFTCS;
        public Texture2D noiseTex;
        public List<SimTexBase> simTexBases = new List<SimTexBase>();

        public IFFTSettings IFFTSettings;
        private float globalTime;
        [HideInInspector]
        public float time;
        public static readonly int sp_OceanDepth = Shader.PropertyToID("_OceanDepth");
        public static readonly int sp_Fetch = Shader.PropertyToID("_Fetch");
        public static readonly int sp_WindSpeed = Shader.PropertyToID("_WindSpeed");
        public static readonly int sp_WindSpreadScale = Shader.PropertyToID("_WindSpreadScale");
        public static readonly int sp_Gammar = Shader.PropertyToID("_Gammar");
        public static readonly int sp_Scale = Shader.PropertyToID("_Scale");
        public static readonly int sp_SwellStrength = Shader.PropertyToID("_SwellStrength");
        public static readonly int sp_WindDir = Shader.PropertyToID("_WindDir");
        public static readonly int sp_Time = Shader.PropertyToID("_Time");
        public static readonly int sp_T0SpecTex = Shader.PropertyToID("_T0SpecTex");
        public static readonly int sp_WaveData = Shader.PropertyToID("_WaveData");
        public static readonly int sp_NoiseTex = Shader.PropertyToID("_NoiseTex");
        public static readonly int sp_PhillipsOut_X_Z = Shader.PropertyToID("_PhillipsOut_X_Z");
        public static readonly int sp_PhillipsOut_Y_Dzx = Shader.PropertyToID("_PhillipsOut_Y_Dzx");
        public static readonly int sp_PhillipsOut_Dyz_Dyx = Shader.PropertyToID("_PhillipsOut_Dyz_Dyx");
        public static readonly int sp_PhillipsOut_Dxx_Dzz = Shader.PropertyToID("_PhillipsOut_Dxx_Dzz");
        public static readonly int sp_TexSize = Shader.PropertyToID("_TexSize");
        public static readonly int sp_IFFTCenterValue_1 = Shader.PropertyToID("_IFFTCenterValue_1");
        public static readonly int sp_IFFTCenterValue_2 = Shader.PropertyToID("_IFFTCenterValue_2");
        public static readonly int sp_ButterflyGraph = Shader.PropertyToID("_ButterflyGraph");
        public static readonly int sp_Step = Shader.PropertyToID("_Step");
        public static readonly int sp_IFFTType = Shader.PropertyToID("_IFFTType");
        public static readonly int sp_Offest = Shader.PropertyToID("_Offest");
        public static readonly int sp_FoamThreshold = Shader.PropertyToID("_FoamThreshold");
        public static readonly int sp_FoamIntensity = Shader.PropertyToID("_FoamIntensity");
        public static readonly int sp_FoamAttenuation = Shader.PropertyToID("_FoamAttenuation");
        public static readonly int sp_deltaTime = Shader.PropertyToID("_deltaTime");
        public static readonly int sp_formTex = Shader.PropertyToID("_formTex");
        public static readonly int sp_posOffestTex = Shader.PropertyToID("_posOffestTex");
        public static readonly int sp_norOffestTex = Shader.PropertyToID("_norOffestTex");
        public static readonly int sp_frequencyScale = Shader.PropertyToID("_FrequencyScale");
        public static readonly int sp_normalScale = Shader.PropertyToID("_NormalScale");
        public static readonly int sp_vertScale = Shader.PropertyToID("_VertScale");
        public static int ke_SpawnPhillipsBase;
        public static int ke_CalculateTimeOffest;
        public static int ke_IFFT_H;
        public static int ke_IFFT_V;
        public static int ke_buttlerflyCreate;
        public static int ke_fixValue;
        public static int ke_HandleResult;
        public static readonly int ke_ExecuteIFFT = 4;
        public void ResetT0Tex()
        {
            InitT0Tex();
        }

        public void InitT0Tex()
        {
            simTexBases.Clear();
            foreach (var baseTex in OceanWaveformBase.Instance.simTexBases)
            {
                if(baseTex.texType == SimTexBase.TexType.IFFT)
                {
                    simTexBases.Add(baseTex); 

                }
            }
        }
        public void RefreshT0Value()
        {
            for (int i = 0; i < simTexBases.Count; i++)
            {
                //creat Phillips tex
                SimTexBase T0 = simTexBases[i];
                IFFTSettings settings = T0.iFFTSettings;
                RenderTexture T0SpecTex = RTManager.Instance.GetRT("T0SpecTex" + i.ToString(), T0.iFFTSettings.texSize, RenderTextureFormat.RGFloat);
                RenderTexture WaveData = RTManager.Instance.GetRT("WaveData" + i.ToString(), T0.iFFTSettings.texSize, RenderTextureFormat.ARGBFloat);
                PhillCS.SetFloat(sp_OceanDepth, settings.oceanDepth);
                PhillCS.SetFloat(sp_Fetch, settings.fetch);
                PhillCS.SetFloat(sp_WindSpeed, settings.windSpeed);
                PhillCS.SetFloat(sp_WindSpreadScale, settings.windSpreadScale);
                PhillCS.SetFloat(sp_Gammar, settings.gammar);
                PhillCS.SetFloat(sp_Scale, settings.scale);
                PhillCS.SetFloat(sp_SwellStrength, settings.swellStrength);
                PhillCS.SetFloat(sp_WindDir, settings.windDir / 180 * Mathf.PI);
                PhillCS.SetFloat(sp_frequencyScale, (2f * Mathf.PI) / settings.frequencyScale);
                PhillCS.SetInt(sp_TexSize, settings.texSize);
                PhillCS.SetTexture(ke_SpawnPhillipsBase, sp_T0SpecTex, T0SpecTex);
                PhillCS.SetTexture(ke_SpawnPhillipsBase, sp_WaveData, WaveData);
                PhillCS.SetTexture(ke_SpawnPhillipsBase, sp_NoiseTex, noiseTex);

                PhillCS.Dispatch(ke_SpawnPhillipsBase, settings.texSize / 8, settings.texSize / 8, 1);

                //create IFFT buttlerfly
                int lenght = (int)Mathf.Log(T0.iFFTSettings.texSize, 2);
                RenderTexture butterflyTex = RTManager.Instance.GetRT("butterflyTex", lenght, T0.iFFTSettings.texSize, RenderTextureFormat.ARGBFloat);
                IFFTCS.SetInt(sp_TexSize, settings.texSize);
                IFFTCS.SetTexture(ke_buttlerflyCreate, sp_ButterflyGraph, butterflyTex);
                IFFTCS.Dispatch(ke_buttlerflyCreate, lenght, T0.iFFTSettings.texSize / 8 / 2, 1);

            }

        }
        public void Update()
        {
            globalTime = Time.time;
        }
        public void UpdateSimTex(CommandBuffer commandBuffer)
        {   
            for (int i = 0; i < simTexBases.Count; i++)
            {
                if (simTexBases[i].waterSimEnable)
                {
                    for (int j = 0; j < simTexBases[i].iFFTSettings.lodChannels.Count; j++)
                    {
                        IFFTSpawn(simTexBases[i], i, commandBuffer, out RenderTexture outVertex, out RenderTexture outNormal, out RenderTexture foamTex);
                        TextureManager.Instance.HandleSimResult(outVertex, outNormal, simTexBases[i].iFFTSettings.lodChannels[j], simTexBases[i].iFFTSettings.lodChannels[j].mappedSize, commandBuffer, WaterPlaneControl.Instance.GetPlanePos(), simTexBases[i].iFFTSettings.lodChannels[j].vertScale, simTexBases[i].iFFTSettings.lodChannels[j].normalScale);
                        TextureManager.Instance.HandleSimResult(foamTex, simTexBases[i].iFFTSettings.lodChannels[j], simTexBases[i].iFFTSettings.lodChannels[j].mappedSize, commandBuffer, WaterPlaneControl.Instance.GetPlanePos(), simTexBases[i].iFFTSettings.lodChannels[j].normalScale);
                    }
                }

            }
        }
        public void IFFTSpawn(SimTexBase simTexBase, int index, CommandBuffer commandBuffer, out RenderTexture outVertex, out RenderTexture outNormal, out RenderTexture foamTex)
        {
            outVertex = RTManager.Instance.GetRT("outVertex" + index.ToString(), simTexBase.iFFTSettings.texSize, RenderTextureFormat.ARGBFloat);
            outNormal = RTManager.Instance.GetRT("outNormal" + index.ToString(), simTexBase.iFFTSettings.texSize, RenderTextureFormat.ARGBFloat);
            RenderTexture T0SpecTex = RTManager.Instance.GetRT("T0SpecTex" + index.ToString(), simTexBase.iFFTSettings.texSize, RenderTextureFormat.RGFloat);
            RenderTexture WaveData = RTManager.Instance.GetRT("WaveData" + index.ToString(), simTexBase.iFFTSettings.texSize, RenderTextureFormat.ARGBFloat);

            RenderTexture phillipsOut_X_Z = RTManager.Instance.GetRT("phillipsOut_X_Z", simTexBase.iFFTSettings.texSize, RenderTextureFormat.RGFloat);
            RenderTexture phillipsOut_Y_Dzx = RTManager.Instance.GetRT("phillipsOut_Y_Dzx", simTexBase.iFFTSettings.texSize, RenderTextureFormat.RGFloat);
            RenderTexture phillipsOut_Dyz_Dyx = RTManager.Instance.GetRT("phillipsOut_Dyz_Dyx", simTexBase.iFFTSettings.texSize, RenderTextureFormat.RGFloat);
            RenderTexture phillipsOut_Dxx_Dzz = RTManager.Instance.GetRT("phillipsOut_Dxx_Dzz", simTexBase.iFFTSettings.texSize, RenderTextureFormat.RGFloat);

            foamTex = RTManager.Instance.GetRT("formTex" + index.ToString(), simTexBase.iFFTSettings.texSize, RenderTextureFormat.RFloat);
            commandBuffer.SetComputeFloatParam(PhillCS, sp_Time, globalTime * simTexBase.iFFTSettings.timeScale);
            commandBuffer.SetComputeIntParam(PhillCS, sp_TexSize, simTexBase.iFFTSettings.texSize);
            commandBuffer.SetComputeTextureParam(PhillCS, ke_CalculateTimeOffest, sp_T0SpecTex, T0SpecTex);
            commandBuffer.SetComputeTextureParam(PhillCS, ke_CalculateTimeOffest, sp_WaveData, WaveData);

            commandBuffer.SetComputeTextureParam(PhillCS, ke_CalculateTimeOffest, sp_PhillipsOut_X_Z, phillipsOut_X_Z);
            commandBuffer.SetComputeTextureParam(PhillCS, ke_CalculateTimeOffest, sp_PhillipsOut_Y_Dzx, phillipsOut_Y_Dzx);
            commandBuffer.SetComputeTextureParam(PhillCS, ke_CalculateTimeOffest, sp_PhillipsOut_Dyz_Dyx, phillipsOut_Dyz_Dyx);
            commandBuffer.SetComputeTextureParam(PhillCS, ke_CalculateTimeOffest, sp_PhillipsOut_Dxx_Dzz, phillipsOut_Dxx_Dzz);
            commandBuffer.DispatchCompute(PhillCS, ke_CalculateTimeOffest, simTexBase.iFFTSettings.texSize / 8, simTexBase.iFFTSettings.texSize / 8, 1);

            //IFFT
            ExecuteIFFT(simTexBase, commandBuffer, phillipsOut_X_Z);
            ExecuteIFFT(simTexBase, commandBuffer, phillipsOut_Y_Dzx);
            ExecuteIFFT(simTexBase, commandBuffer, phillipsOut_Dyz_Dyx);
            ExecuteIFFT(simTexBase, commandBuffer, phillipsOut_Dxx_Dzz);
            //Get 8 parameter X Y Z Dzx Dyz Dyx Dxx Dzz

            commandBuffer.SetComputeTextureParam(PhillCS, ke_HandleResult, sp_PhillipsOut_X_Z, phillipsOut_X_Z);
            commandBuffer.SetComputeTextureParam(PhillCS, ke_HandleResult, sp_PhillipsOut_Y_Dzx, phillipsOut_Y_Dzx);
            commandBuffer.SetComputeTextureParam(PhillCS, ke_HandleResult, sp_PhillipsOut_Dyz_Dyx, phillipsOut_Dyz_Dyx);
            commandBuffer.SetComputeTextureParam(PhillCS, ke_HandleResult, sp_PhillipsOut_Dxx_Dzz, phillipsOut_Dxx_Dzz);

            commandBuffer.SetComputeFloatParam(PhillCS, sp_deltaTime, globalTime * Time.deltaTime);
            commandBuffer.SetComputeFloatParam(PhillCS, sp_Offest, simTexBase.iFFTSettings.offest);

            commandBuffer.SetComputeFloatParam(PhillCS, sp_FoamAttenuation, simTexBase.iFFTSettings.foamAttenuation);
            commandBuffer.SetComputeFloatParam(PhillCS, sp_FoamIntensity, simTexBase.iFFTSettings.foamIntensity);
            commandBuffer.SetComputeFloatParam(PhillCS, sp_FoamThreshold, simTexBase.iFFTSettings.foamThreshold);

            commandBuffer.SetComputeTextureParam(PhillCS, ke_HandleResult, sp_formTex, foamTex);
            commandBuffer.SetComputeTextureParam(PhillCS, ke_HandleResult, sp_posOffestTex, outVertex);
            commandBuffer.SetComputeTextureParam(PhillCS, ke_HandleResult, sp_norOffestTex, outNormal);
            commandBuffer.SetComputeFloatParam(PhillCS, sp_normalScale, simTexBase.iFFTSettings.normalScale);
            commandBuffer.SetComputeFloatParam(PhillCS, sp_vertScale, simTexBase.iFFTSettings.vertScale);
            commandBuffer.DispatchCompute(PhillCS, ke_HandleResult, simTexBase.iFFTSettings.texSize / 8, simTexBase.iFFTSettings.texSize / 8, 1);

        }
        public void ExecuteIFFT(SimTexBase simTexBase, CommandBuffer commandBuffer, RenderTexture source)
        {

            RenderTexture temTex_1 = RTManager.Instance.GetRT("temTex_1", simTexBase.iFFTSettings.texSize, RenderTextureFormat.RGFloat);
            RenderTexture temTex_2 = RTManager.Instance.GetRT("temTex_2", simTexBase.iFFTSettings.texSize, RenderTextureFormat.RGFloat);
            int lenght = (int)Mathf.Log(simTexBase.iFFTSettings.texSize, 2);
            RenderTexture butterflyTex = RTManager.Instance.GetRT("butterflyTex", lenght, simTexBase.iFFTSettings.texSize, RenderTextureFormat.ARGBFloat);
            commandBuffer.Blit(source, temTex_1);
            int kernelIndex = ke_ExecuteIFFT + (lenght - 3) * 2;

            if (kernelIndex < 4 || kernelIndex > 24)
            {
                Debug.LogError("Kernel index out of range");
            }
            commandBuffer.SetComputeTextureParam(IFFTCS, kernelIndex, sp_IFFTCenterValue_1, temTex_1);
            commandBuffer.SetComputeTextureParam(IFFTCS, kernelIndex, sp_IFFTCenterValue_2, temTex_2);
            commandBuffer.SetComputeTextureParam(IFFTCS, kernelIndex, sp_ButterflyGraph, butterflyTex);
            commandBuffer.DispatchCompute(IFFTCS, kernelIndex, 1, simTexBase.iFFTSettings.texSize, 1);
            commandBuffer.SetComputeTextureParam(IFFTCS, kernelIndex + 1, sp_IFFTCenterValue_1, temTex_1);
            commandBuffer.SetComputeTextureParam(IFFTCS, kernelIndex + 1, sp_IFFTCenterValue_2, temTex_2);
            commandBuffer.SetComputeTextureParam(IFFTCS, kernelIndex + 1, sp_ButterflyGraph, butterflyTex);
            commandBuffer.DispatchCompute(IFFTCS, kernelIndex + 1, simTexBase.iFFTSettings.texSize, 1, 1);
            commandBuffer.Blit(temTex_1, source);
        }
        public void IFFT(SimTexBase simTexBase, CommandBuffer commandBuffer, RenderTexture source)
        {
            RenderTexture temTex_1 = RTManager.Instance.GetRT("temTex_1", simTexBase.iFFTSettings.texSize, RenderTextureFormat.RGFloat);
            RenderTexture temTex_2 = RTManager.Instance.GetRT("temTex_2", simTexBase.iFFTSettings.texSize, RenderTextureFormat.RGFloat);
            int lenght = (int)Mathf.Log(simTexBase.iFFTSettings.texSize, 2);
            RenderTexture butterflyTex = RTManager.Instance.GetRT("butterflyTex", lenght, simTexBase.iFFTSettings.texSize, RenderTextureFormat.ARGBFloat);
            commandBuffer.Blit(source, temTex_1);
            //commandBuffer.Blit(source, temTex_2);
            commandBuffer.SetComputeTextureParam(IFFTCS, ke_IFFT_H, sp_IFFTCenterValue_1, temTex_1);
            commandBuffer.SetComputeTextureParam(IFFTCS, ke_IFFT_H, sp_IFFTCenterValue_2, temTex_2);
            commandBuffer.SetComputeTextureParam(IFFTCS, ke_IFFT_V, sp_IFFTCenterValue_1, temTex_1);
            commandBuffer.SetComputeTextureParam(IFFTCS, ke_IFFT_V, sp_IFFTCenterValue_2, temTex_2);

            commandBuffer.SetComputeTextureParam(IFFTCS, ke_fixValue, sp_IFFTCenterValue_1, temTex_1);
            commandBuffer.SetComputeTextureParam(IFFTCS, ke_fixValue, sp_IFFTCenterValue_2, temTex_2);

            commandBuffer.SetComputeTextureParam(IFFTCS, ke_IFFT_H, sp_ButterflyGraph, butterflyTex);
            commandBuffer.SetComputeTextureParam(IFFTCS, ke_IFFT_V, sp_ButterflyGraph, butterflyTex);
            commandBuffer.SetComputeIntParam(IFFTCS, sp_TexSize, simTexBase.iFFTSettings.texSize);
            bool IFFTType = false;

            for (int i = 0; i < lenght; i++)//IFFT_H
            {
                commandBuffer.SetComputeIntParam(IFFTCS, sp_Step, i);
                commandBuffer.SetComputeIntParam(IFFTCS, sp_IFFTType, IFFTType ? 1 : 0);
                commandBuffer.DispatchCompute(IFFTCS, ke_IFFT_H, simTexBase.iFFTSettings.texSize / 8, simTexBase.iFFTSettings.texSize / 8, 1);
                IFFTType = !IFFTType;
            }
            for (int i = 0; i < lenght; i++)//IFFT_V
            {
                commandBuffer.SetComputeIntParam(IFFTCS, sp_Step, i);
                commandBuffer.SetComputeIntParam(IFFTCS, sp_IFFTType, IFFTType ? 1 : 0);
                commandBuffer.DispatchCompute(IFFTCS, ke_IFFT_V, simTexBase.iFFTSettings.texSize / 8, simTexBase.iFFTSettings.texSize / 8, 1);
                IFFTType = !IFFTType;
            }
            commandBuffer.SetComputeIntParam(IFFTCS, sp_IFFTType, IFFTType ? 1 : 0);
            commandBuffer.DispatchCompute(IFFTCS, ke_fixValue, simTexBase.iFFTSettings.texSize / 8, simTexBase.iFFTSettings.texSize / 8, 1);//fixValue

            if (IFFTType)
            {
                commandBuffer.Blit(temTex_2, source);
            }
            else
            {
                commandBuffer.Blit(temTex_1, source);
            }

        }
        public override void DestroySystem()
        {
            simTexBases.Clear();
        }
        public override void ResetSystem()
        {
            ke_SpawnPhillipsBase = PhillCS.FindKernel("SpawnPhillipsBase");
            ke_CalculateTimeOffest = PhillCS.FindKernel("CalculateTimeOffest");
            ke_IFFT_H = IFFTCS.FindKernel("IFFT_H");
            ke_IFFT_V = IFFTCS.FindKernel("IFFT_V");
            ke_buttlerflyCreate = IFFTCS.FindKernel("buttlerflyCreate");
            ke_fixValue = IFFTCS.FindKernel("fixValue");
            ke_HandleResult = PhillCS.FindKernel("HandleResult");
            
            ResetT0Tex();
            RefreshT0Value();

        }
    }

}