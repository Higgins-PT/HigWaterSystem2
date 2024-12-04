using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HigWaterSystem2
{

    [ExecuteInEditMode]
    public class WaveDataInput : MonoBehaviour
    {
        public float weight = 0;
        public virtual WaveDataType waveDataType { get { return WaveDataType.heightMap; } }
        private bool isQuitting;
        public virtual Renderer GetRenderer()
        {
            return GetComponent<Renderer>();
        }
        public virtual Material GetMat()
        {
            return GetRenderer().sharedMaterial;
        }
        public virtual List<Material> GetMats()
        {
            List<Material> mats = new List<Material>();
            GetRenderer().GetSharedMaterials(mats);
            return mats;

        }
        public virtual void RenderWaveScaleTex(CommandBuffer cmd)
        {
            Renderer renderer = GetRenderer();
            List<Material> materials = GetMats();

            for (var i = 0; i < materials.Count; i++)
            {
                cmd.DrawRenderer(renderer, materials[i], submeshIndex: i, 0);
            }

        }
        public virtual void EndRenderHandle(CommandBuffer cmd, RenderTexture renderTexture)
        {

        }
        void OnApplicationQuit()
        {
            isQuitting = true;
        }
        public void Start()
        {
            isQuitting = false;
        }
        public virtual void OnEnable()
        {
            GetRenderer().enabled = false;
            try
            {
                CameraMapManager.Instance.AddWaveData(this);
            }
            catch
            {

            }
        }
        public virtual void OnDisable()
        {
            if (isQuitting == false)
            {
                try
                {
                    CameraMapManager.Instance.RemoveWaveData(this);
                }
                catch
                {

                }
            }
        }
    }
    public class HeightDataInput : WaveDataInput
    {
        public override WaveDataType waveDataType { get { return WaveDataType.heightMap; } }


    }
    public class WaveScaleDataInput : WaveDataInput
    {
        public override WaveDataType waveDataType { get { return WaveDataType.scale; } }


    }
    public class AlbedoDataInput : WaveDataInput
    {
        public override WaveDataType waveDataType { get { return WaveDataType.albedo; } }


    }
    public class FlowMapDataInput : WaveDataInput
    {
        public override WaveDataType waveDataType { get { return WaveDataType.flowMap; } }


    }
}