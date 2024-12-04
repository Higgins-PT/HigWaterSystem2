
using UnityEngine;


namespace HigWaterSystem2
{
    public class WaterSimulationBase<T> : HigSingleInstance<T> where T : MonoBehaviour
    {

    }
    [System.Serializable]
    public class LodChannel
    {
        public float mappedSize = 10f;
        public float normalScale = 1f;
        public float vertScale = 1f;
        public float renderDistance = 1000f;
        public bool channel_Lod;
        public int channel_MaxLodIndex = 8;
        public bool channel_detail_0;
        public bool channel_detail_1;
        public bool physic_Enable;
        public bool Enable_PhysicChannel()
        {
            return physic_Enable;
        }
        public bool ReturnChannelBool(int index)
        {
            if (channel_Lod)
            {
                if (index <= channel_MaxLodIndex)
                {
                    return true;
                }
            }
            return false;
        }
        public bool ReturnChannelBool_Detail(int index)
        {
            switch (index)
            {
                case 0: return channel_detail_0;
                case 1: return channel_detail_1;
                default:
                    return false;
            }
        }
        public void DeepCopy(LodChannel lodChannel)
        {
            mappedSize = lodChannel.mappedSize;
            normalScale = lodChannel.normalScale;
            vertScale = lodChannel.vertScale;
            renderDistance = lodChannel.renderDistance;
            channel_Lod = lodChannel.channel_Lod;
            channel_MaxLodIndex = lodChannel.channel_MaxLodIndex;
            channel_detail_0 = lodChannel.channel_detail_0;
            channel_detail_1 = lodChannel.channel_detail_1;
            physic_Enable = lodChannel.physic_Enable;
        }
    }

    [System.Serializable]
    public struct LodRange
    {
        public int start;
        public int end;
        public LodRange(int start, int end)
        {
            this.start = start;
            this.end = end;
        }
    }
    [System.Serializable]
    public class SimTexBase
    {
        public bool waterSimEnable = true;
        public enum TexType
        {
            IFFT,
            Noise
        }
        
        public IFFTSettings iFFTSettings = new IFFTSettings();
        public NoiseSettings noiseSettings = new NoiseSettings();
        public TexType texType;

        public void DeepCopy(SimTexBase source)
        {

            this.texType = source.texType;
            waterSimEnable = source.waterSimEnable;
            if (source.iFFTSettings != null)
            {
                this.iFFTSettings = new IFFTSettings();
                this.iFFTSettings.DeepCopyIFFTSetting(source.iFFTSettings);
            }

            if (source.noiseSettings != null)
            {
                this.noiseSettings = new NoiseSettings();
                this.noiseSettings.texSize = source.noiseSettings.texSize;
                this.noiseSettings.timeScale = source.noiseSettings.timeScale;
                this.noiseSettings.normalScale = source.noiseSettings.normalScale;
                this.noiseSettings.frequency = source.noiseSettings.frequency;
                this.noiseSettings.octaves = source.noiseSettings.octaves;
                this.noiseSettings.amplitude = source.noiseSettings.amplitude;
                noiseSettings.lodChannels = source.noiseSettings.lodChannels;
            }
        }
        public void InitializeSettings()
        {

            switch (texType)
            {
                case TexType.IFFT:
                    iFFTSettings = new IFFTSettings();
                    iFFTSettings.texSize = IFFTManager.Instance.IFFTSettings.texSize;
                    iFFTSettings.oceanDepth = IFFTManager.Instance.IFFTSettings.oceanDepth;
                    iFFTSettings.fetch = IFFTManager.Instance.IFFTSettings.fetch;
                    iFFTSettings.windSpeed = IFFTManager.Instance.IFFTSettings.windSpeed;
                    iFFTSettings.windSpreadScale = IFFTManager.Instance.IFFTSettings.windSpreadScale;
                    iFFTSettings.gammar = IFFTManager.Instance.IFFTSettings.gammar;
                    iFFTSettings.scale = IFFTManager.Instance.IFFTSettings.scale;
                    iFFTSettings.swellStrength = IFFTManager.Instance.IFFTSettings.swellStrength;
                    iFFTSettings.windDir = IFFTManager.Instance.IFFTSettings.windDir;
                    iFFTSettings.timeScale = IFFTManager.Instance.IFFTSettings.timeScale;
                    iFFTSettings.offest = IFFTManager.Instance.IFFTSettings.offest;
                    iFFTSettings.foamIntensity = IFFTManager.Instance.IFFTSettings.foamIntensity;
                    iFFTSettings.foamAttenuation = IFFTManager.Instance.IFFTSettings.foamAttenuation;
                    iFFTSettings.foamThreshold = IFFTManager.Instance.IFFTSettings.foamThreshold;


                    iFFTSettings.frequencyScale = IFFTManager.Instance.IFFTSettings.frequencyScale;
                    iFFTSettings.normalScale = IFFTManager.Instance.IFFTSettings.normalScale;
                    iFFTSettings.vertScale = IFFTManager.Instance.IFFTSettings.vertScale;
                    break;
                case TexType.Noise:
                    noiseSettings = new NoiseSettings();
                    noiseSettings.texSize = NoiseManager.Instance.noiseSettings.texSize;
                    noiseSettings.timeScale = NoiseManager.Instance.noiseSettings.timeScale;
                    noiseSettings.normalScale = NoiseManager.Instance.noiseSettings.normalScale;
                    noiseSettings.frequency = NoiseManager.Instance.noiseSettings.frequency;
                    noiseSettings.octaves = NoiseManager.Instance.noiseSettings.octaves;
                    noiseSettings.amplitude = NoiseManager.Instance.noiseSettings.amplitude;
                    break;
            }
        }
    }

}