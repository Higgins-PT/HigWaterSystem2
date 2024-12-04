using HigWaterSystem2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HigWaterSystem2
{
    public class TextureManager : HigSingleInstance<TextureManager>
    {
        [HideInInspector]
        public List<RenderTexture> vertTexture = new List<RenderTexture>();
        [HideInInspector]
        public List<RenderTexture> normalTexture = new List<RenderTexture>();
        public List<RenderTexture> foamTexture = new List<RenderTexture>();
        public List<RenderTexture> detailTexture = new List<RenderTexture>();

        public List<RenderTexture> detailTexture_Vert = new List<RenderTexture>();
        public List<int> vertTexSizes = new List<int>(9);
        public List<int> normalTexSizes = new List<int>(9);
        public List<int> detailTexSizes = new List<int>(2);
        public List<float> detailTexRenderDistance = new List<float>(2);
        public List<float> detailTexMapSize = new List<float>(2);

        public List<bool> detailTexMapVertEnable = new List<bool>(2);

        public override void DestroySystem()
        {
            Destroy();
        }
        public void Destroy()
        {
            foreach (RenderTexture tex in vertTexture)
            {
                if (tex != null)
                {
                    tex?.Release();
                }
            }
            vertTexture.Clear();
            foreach (RenderTexture tex in normalTexture)
            {
                if (tex != null)
                {
                    tex?.Release();
                }
            }
            normalTexture.Clear();
            foreach (RenderTexture tex in foamTexture)
            {
                if (tex != null)
                {
                    tex?.Release();
                }
            }
            foamTexture.Clear();
            foreach (RenderTexture tex in detailTexture)
            {
                if (tex != null)
                {
                    tex?.Release();
                }
            }
            detailTexture.Clear();
            foreach (RenderTexture tex in detailTexture_Vert)
            {
                if (tex != null)
                {
                    tex?.Release();
                }
            }
            detailTexture_Vert.Clear();
        }
        public override void ResetSystem()
        {
            Destroy();
            for (int i = 0; i < WaterPlaneControl.Instance.lodCount; i++)
            {
                RenderTexture renderTexture = new RenderTexture(vertTexSizes[i], vertTexSizes[i], 0);
                renderTexture.format = RenderTextureFormat.ARGBFloat;
                renderTexture.enableRandomWrite = true;
                renderTexture.filterMode = FilterMode.Trilinear;
                renderTexture.anisoLevel = 6;
                renderTexture.Create();
                vertTexture.Add(renderTexture);
                WaterPlaneControl.Instance.waterMats[i].SetTexture("_VertTex", renderTexture);
                WaterPlaneControl.Instance.waterMats[i].SetInt("vertTexSize", vertTexSizes[i]);
                int n = i - 1;
                if (n >= 0)
                {

                    WaterPlaneControl.Instance.waterMats[n].SetTexture("_VertTex_Next", renderTexture);
                    WaterPlaneControl.Instance.waterMats[n].SetInt("vertTexSize_Next", vertTexSizes[i]);
                }
            }
            for (int i = 0; i < WaterPlaneControl.Instance.lodCount; i++)
            {
                RenderTexture renderTexture = new RenderTexture(normalTexSizes[i], normalTexSizes[i], 0);
                renderTexture.format = RenderTextureFormat.RFloat;
                renderTexture.enableRandomWrite = true;
                renderTexture.filterMode = FilterMode.Trilinear;
                renderTexture.anisoLevel = 6;
                renderTexture.Create();
                foamTexture.Add(renderTexture);
                WaterPlaneControl.Instance.waterMats[i].SetTexture("_FoamTex", renderTexture);
                WaterPlaneControl.Instance.waterMats[i].SetInt("foamTexSize", normalTexSizes[i]);
                int n = i - 1;
                if (n >= 0)
                {

                    WaterPlaneControl.Instance.waterMats[n].SetTexture("_FoamTex_Next", renderTexture);
                    WaterPlaneControl.Instance.waterMats[n].SetInt("foamTexSize_Next", normalTexSizes[i]);
                }
            }
            for (int i = 0; i < WaterPlaneControl.Instance.lodCount; i++)
            {
                RenderTexture renderTexture = new RenderTexture(normalTexSizes[i], normalTexSizes[i], 0);
                renderTexture.format = RenderTextureFormat.ARGBFloat;
                renderTexture.enableRandomWrite = true;
                renderTexture.filterMode = FilterMode.Trilinear;
                renderTexture.anisoLevel = 6;
                renderTexture.Create();
                normalTexture.Add(renderTexture);
                WaterPlaneControl.Instance.waterMats[i].SetTexture("_NormalTex", renderTexture);
                WaterPlaneControl.Instance.waterMats[i].SetInt("normalTexSize", normalTexSizes[i]);
                int n = i - 1;
                if (n >= 0)
                {

                    WaterPlaneControl.Instance.waterMats[n].SetTexture("_NormalTex_Next", renderTexture);
                    WaterPlaneControl.Instance.waterMats[n].SetInt("normalTexSize_Next", normalTexSizes[i]);
                }
            }
            for (int i = 0; i < WaterPlaneControl.Instance.detailCount; i++)
            {
                RenderTexture renderTexture = new RenderTexture(detailTexSizes[i], detailTexSizes[i], 0);
                renderTexture.format = RenderTextureFormat.ARGBFloat;
                renderTexture.enableRandomWrite = true;
                renderTexture.filterMode = FilterMode.Trilinear;
                renderTexture.anisoLevel = 6;
                renderTexture.Create();
                detailTexture.Add(renderTexture);
                MatAttribute.SetFloat("_DetailTexRenderDistance" + i.ToString(), detailTexRenderDistance[i]);
                MatAttribute.SetTexture("_DetailTex" + i.ToString(), renderTexture);
                MatAttribute.SetFloat("_DetailTexMapSize" + i.ToString(), detailTexMapSize[i]);
                if (WaterPlaneControl.Instance.enableGlobalShaderProperties)
                {
                    Shader.SetGlobalFloat("_DetailTexRenderDistance" + i.ToString(), detailTexRenderDistance[i]);
                    Shader.SetGlobalTexture("_DetailTex" + i.ToString(), renderTexture);
                    Shader.SetGlobalFloat("_DetailTexMapSize" + i.ToString(), detailTexMapSize[i]);
                }

            }
            for (int i = 0; i < WaterPlaneControl.Instance.detailCount; i++)
            {
                RenderTexture renderTexture = new RenderTexture(detailTexSizes[i], detailTexSizes[i], 0);
                renderTexture.format = RenderTextureFormat.ARGBFloat;
                renderTexture.enableRandomWrite = true;
                renderTexture.filterMode = FilterMode.Trilinear;
                renderTexture.anisoLevel = 6;
                renderTexture.Create();
                detailTexture_Vert.Add(renderTexture);
                MatAttribute.SetTexture("_DetailTex_Vert" + i.ToString(), renderTexture);
                MatAttribute.SetFloat("_DetailTexMapSize_Vert" + i.ToString(), detailTexMapSize[i]);

                MatAttribute.SetInt("_DetailTexMapVertEnable" + i.ToString(), detailTexMapVertEnable[i] ? 1 : 0);
            }
        }
        public void HandleSimResult(RenderTexture foam, LodChannel lodChannel, float mapSize, CommandBuffer cmd, Vector2 posOffest, float normalScale)
        {
            int lodOffest = WaterPlaneControl.Instance.lod_Offest;
            for (int i = 0; i < foamTexture.Count; i++)
            {
                if (lodChannel.ReturnChannelBool(i + lodOffest))
                {
                    Vector2 finePos = WaterPlaneControl.Instance.GetSnapToGrid(i, posOffest);
                    RTFunction.Instance.MapTexture(cmd, foam, foamTexture[i], mapSize, WaterPlaneControl.Instance.GetLodSize(i), finePos, false, normalScale, lodChannel.renderDistance);
                }
            }

        }
        public void HandleSimResult(RenderTexture vert, RenderTexture normal, LodChannel lodChannel, float mapSize, CommandBuffer cmd, Vector2 posOffest, float vertScale, float normalScale)//Put sim result to lod Tex
        {
            int lodOffest = WaterPlaneControl.Instance.lod_Offest;
            for (int i = 0; i < vertTexture.Count; i++)
            {
                if (lodChannel.ReturnChannelBool(i + lodOffest))
                {
                    Vector2 finePos = WaterPlaneControl.Instance.GetSnapToGrid(i, posOffest);
                    RTFunction.Instance.MapTexture(cmd, vert, vertTexture[i], mapSize, WaterPlaneControl.Instance.GetLodSize(i), finePos, false, vertScale, lodChannel.renderDistance);
                    RTFunction.Instance.MapTexture(cmd, normal, normalTexture[i], mapSize, WaterPlaneControl.Instance.GetLodSize(i), finePos, true, normalScale, lodChannel.renderDistance);
                    if (lodChannel.Enable_PhysicChannel())
                    {
                        OceanPhysics.Instance.AddPhysicReadBackRequest(lodChannel, vert, mapSize);
                    }
                }
            }
            for (int i = 0; i < detailTexture.Count; i++)
            {
                if (lodChannel.ReturnChannelBool_Detail(i))
                {
                    RTFunction.Instance.MapTexture(cmd, normal, detailTexture[i], mapSize, mapSize, new Vector2(0, 0), true, normalScale, 0);
                    RTFunction.Instance.MapTexture(cmd, vert, detailTexture_Vert[i], mapSize, mapSize, new Vector2(0, 0), false, vertScale, 0);
                    if (lodChannel.Enable_PhysicChannel())
                    {
                        OceanPhysics.Instance.AddPhysicReadBackRequest(lodChannel, vert, mapSize);
                    }
                }
            }
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            try
            {
                for (int i = 0; i < WaterPlaneControl.Instance.lodCount; i++)
                {
                    WaterPlaneControl.Instance.waterMats[i].SetTexture("_VertTex", vertTexture[i]);
                    WaterPlaneControl.Instance.waterMats[i].SetTexture("_NormalTex", normalTexture[i]);
                }
            }
            catch
            {
                // if out index range
                WaterPlaneControl.Instance.ResetSystem();
            }
        }
    }
}