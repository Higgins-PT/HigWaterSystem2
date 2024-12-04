using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static Unity.Burst.Intrinsics.X86.Avx;
using Color = UnityEngine.Color;
namespace HigWaterSystem2
{

    public class CameraMapManager : HigSingleInstance<CameraMapManager>
    {
        [HideInInspector]
        public Camera heightMapCam;
        [HideInInspector]
        public GameObject camGameObject;

        public float camHeight = 5000f;
        public float camDepth = 5000f;
        public int heightTexSize = 256;
        public int waveScaleDataTexSize = 256; // wave(IFFTScale, heightTexScale)
        public int aldobeTexSize = 512;
        public int flowmapTexSize = 256;

        public int heightLodLimit = 9;
        public int waveScaleLodLimit = 5;
        public int flowmapLodLimit = 4;
        public int aldobeLodLimit = 5;
        public bool heightEnable = true;
        public bool waveScaleEnable = true;
        public bool aldobeEnable = true;
        public bool flowmapEnable = true;
        public int cameraRendererIndex = 0;
        public float _FlowTimeSpeed = 1f;
        public float _FlowMaxDistance = 4f;
        public ComputeShader heightToNormalCS;
        private int height2NormalKernel;
        public Dictionary<WaveDataType, List<WaveDataInput>> waveDataInputs = new Dictionary<WaveDataType, List<WaveDataInput>>();
        public void AddWaveData(WaveDataInput waveDataInput)
        {
            if (!waveDataInputs.ContainsKey(waveDataInput.waveDataType))
            {
                waveDataInputs.Add(waveDataInput.waveDataType, new List<WaveDataInput>());
            }

            waveDataInputs[waveDataInput.waveDataType].Add(waveDataInput);
            waveDataInputs[waveDataInput.waveDataType].Sort((x, y) => x.weight.CompareTo(y.weight));
        }
        public void RemoveWaveData(WaveDataInput waveDataInput)
        {
            if (waveDataInputs.ContainsKey(waveDataInput.waveDataType))
            {
                waveDataInputs[waveDataInput.waveDataType]?.Remove(waveDataInput);
            }
        }
        public List<WaveDataInput> GetWaveDataCluster(WaveDataType waveDataType)
        {
            if (waveDataInputs.ContainsKey(waveDataType))
            {
                return waveDataInputs[waveDataType];
            }
            List<WaveDataInput> waveDataInputl = new List<WaveDataInput>();
            waveDataInputs.Add(waveDataType, waveDataInputl);

            return waveDataInputl;
        }
        public void ResetCam()
        {
            if (heightMapCam == null)
            {
                heightMapCam = CreateTopDownOrthoCamera(WaterPlaneControl.Instance.GetLodSize(0));
            }
            else
            {

                SetCam(heightMapCam, WaterPlaneControl.Instance.GetLodSize(0));
            }
        }
        public void RenderWaveData(CommandBuffer cmd)
        {
            MatAttribute.SetFloat("_FlowTimeSpeed", _FlowTimeSpeed);
            MatAttribute.SetFloat("_FlowMaxDistance", _FlowMaxDistance);
            for (int i = 0; i < WaterPlaneControl.Instance.waterMats.Count; i++)
            {
                SetCamRender(i, camHeight, camDepth);
                SetRenderMatrix(heightMapCam.projectionMatrix, heightMapCam.worldToCameraMatrix, cmd);
                if (heightEnable)
                {
                    RenderHeightTex(cmd, i);
                }
                else
                {
                    MatAttribute.SetInt("_Height_On", 0);
                }
                if (waveScaleEnable)
                {
                    RenderWaveScaleTex(cmd, i);
                }
                else
                {
                    MatAttribute.SetInt("_WaveData_On", 0);
                }
                if (aldobeEnable)
                {
                    RenderAldobeTex(cmd, i);
                }
                else
                {
                    MatAttribute.SetInt("_Aldobe_On", 0);
                }
                if (flowmapEnable)
                {
                    RenderFlowMapTex(cmd, i);
                }
                else
                {
                    MatAttribute.SetInt("_Flow_On", 0);
                }
            }



        }
        public static bool TestCull(WaveDataInput waveDataInput, Camera camera)
        {
            Renderer renderer = waveDataInput.GetRenderer();
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            Bounds objectBounds = renderer.bounds;
            if (GeometryUtility.TestPlanesAABB(planes, objectBounds))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool SetRenderer(CommandBuffer cmd, WaveDataType waveDataType, Camera targetCam)
        {

            List<WaveDataInput> waveDataInputs = GetWaveDataCluster(waveDataType);
            if (waveDataInputs.Count == 0)
            {
                return false;
            }
            if (!(waveDataInputs == null))
            {
                bool error = false;
                foreach (WaveDataInput waveDataInput in waveDataInputs)
                {
                    if (waveDataInput != null)
                    {
                        if (TestCull(waveDataInput, targetCam))
                        {
                            waveDataInput.RenderWaveScaleTex(cmd);
                        }

                    }
                    else
                    {
                        error = true;
                    }

                }
                if (error)
                {
                    List<WaveDataInput> noErrorList = new List<WaveDataInput>();
                    for (int i = 0; i < waveDataInputs.Count; i++)
                    {
                        if (waveDataInputs[i] != null)
                        {
                            noErrorList.Add(waveDataInputs[i]);
                        }
                    }
                    waveDataInputs.Clear();
                    waveDataInputs.AddRange(noErrorList);
                }
            }
            return true;
        }
        public static void SetCmdBackGround(CommandBuffer cmd, WaveDataType waveDataType)
        {
            Color clearColor = GetDefaultWaveDataColor(waveDataType);

            cmd.ClearRenderTarget(true, true, clearColor);
        }
        public static Color GetDefaultWaveDataColor(WaveDataType waveDataType)
        {
            switch (waveDataType)
            {
                case WaveDataType.heightMap:
                    return new Color(0, 1, 0, 0);
                case WaveDataType.flowMap:
                    return new Color(0, 0, 0, 0);
                case WaveDataType.scale:
                    return new Color(1, 1, 1, 1); 
                case WaveDataType.albedo:
                    return new Color(0, 0, 0, 0);
                default:
                    return Color.black; 
            }
        }

        public void RenderFlowMapTex(CommandBuffer cmd, int lod)
        {

            RenderTexture renderTexture = RTManager.Instance.GetRT("flowMapTex" + lod.ToString(), flowmapTexSize, RenderTextureFormat.ARGBFloat, TextureWrapMode.Clamp);


            cmd.SetRenderTarget(renderTexture);
            SetCmdBackGround(cmd, WaveDataType.flowMap);
            if (SetRenderer(cmd, WaveDataType.flowMap, heightMapCam) || lod <= flowmapLodLimit - WaterPlaneControl.Instance.lod_Offest)
            {
                WaterPlaneControl.Instance.waterMats[lod].SetInt("_Flow_On", 1);
            }
            else
            {

                WaterPlaneControl.Instance.waterMats[lod].SetInt("_Flow_On", 0);
            }


            WaterPlaneControl.Instance.waterMats[lod].SetTexture("_FlowMapTex", renderTexture);



        }
        public void RenderAldobeTex(CommandBuffer cmd, int lod)
        {

            RenderTexture renderTexture = RTManager.Instance.GetRT("aldobeTex" + lod.ToString(), aldobeTexSize, RenderTextureFormat.ARGBFloat, TextureWrapMode.Clamp);


            cmd.SetRenderTarget(renderTexture);
            SetCmdBackGround(cmd, WaveDataType.albedo);

            if (lod <= aldobeLodLimit - WaterPlaneControl.Instance.lod_Offest)
            {
                if (SetRenderer(cmd, WaveDataType.albedo, heightMapCam))
                {
                    WaterPlaneControl.Instance.waterMats[lod].SetInt("_Aldobe_On", 1);
                }
                else
                {

                    WaterPlaneControl.Instance.waterMats[lod].SetInt("_Aldobe_On", 0);
                }

            }
            WaterPlaneControl.Instance.waterMats[lod].SetTexture("_AldobeTex", renderTexture);



        }
        public void RenderWaveScaleTex(CommandBuffer cmd, int lod)
        {

            RenderTexture renderTexture = RTManager.Instance.GetRT("waveDataTex" + lod.ToString(), waveScaleDataTexSize, RenderTextureFormat.ARGBFloat, TextureWrapMode.Clamp);


            cmd.SetRenderTarget(renderTexture);
            SetCmdBackGround(cmd, WaveDataType.scale);
            if (lod <= waveScaleLodLimit - WaterPlaneControl.Instance.lod_Offest)
            {
                if(SetRenderer(cmd, WaveDataType.scale, heightMapCam))
                {
                    WaterPlaneControl.Instance.waterMats[lod].SetInt("_WaveData_On", 1);
                }
                else
                {

                    WaterPlaneControl.Instance.waterMats[lod].SetInt("_WaveData_On", 0);
                }
            }
;
            WaterPlaneControl.Instance.waterMats[lod].SetTexture("_WaveDataTex", renderTexture);



        }
        public void RenderHeightTex(CommandBuffer cmd, int lod)
        {

            RenderTexture renderTexture = RTManager.Instance.GetRT("heightTex" + lod.ToString(), heightTexSize, RenderTextureFormat.ARGBFloat, TextureWrapMode.Clamp);
            RenderTexture temTex = RTManager.Instance.GetRT("temTex" + lod.ToString(), heightTexSize, RenderTextureFormat.ARGBFloat, TextureWrapMode.Clamp);
            cmd.SetRenderTarget(temTex);
            SetCmdBackGround(cmd, WaveDataType.heightMap);
            if (lod <= heightLodLimit - WaterPlaneControl.Instance.lod_Offest)
            {
                if(SetRenderer(cmd, WaveDataType.heightMap, heightMapCam))
                {
                    WaterPlaneControl.Instance.waterMats[lod].SetInt("_Height_On", 1);
                }
                else
                {

                    WaterPlaneControl.Instance.waterMats[lod].SetInt("_Height_On", 0);
                }

            }
            Height2Normal(cmd, WaterPlaneControl.Instance.GetLodSize(lod) / 2f, heightTexSize, temTex, renderTexture);

            WaterPlaneControl.Instance.waterMats[lod].SetFloat("_HeightTexSize", heightTexSize);
            WaterPlaneControl.Instance.waterMats[lod].SetTexture("_HeightTex", renderTexture);
            if (lod > 0)
            {
                WaterPlaneControl.Instance.waterMats[lod - 1].SetTexture("_HeightTex_Next", renderTexture);

            }





        }
        public void Height2Normal(CommandBuffer cmd, float worldSize, int texSize, RenderTexture source, RenderTexture target)
        {
            cmd.SetComputeTextureParam(heightToNormalCS, height2NormalKernel, "_MainTex", source);
            cmd.SetComputeTextureParam(heightToNormalCS, height2NormalKernel, "_ResultTexture", target);
            cmd.SetComputeIntParam(heightToNormalCS, "_TexSize", texSize);
            cmd.SetComputeFloatParam(heightToNormalCS, "_CellSize", worldSize / texSize);
            int numthreads = Mathf.CeilToInt(texSize / 8);
            cmd.DispatchCompute(heightToNormalCS, height2NormalKernel, numthreads, numthreads, 1);
        }
        public void SetRenderMatrix(Matrix4x4 projectionMatrix, Matrix4x4 viewMatrix, CommandBuffer cmd)
        {
            cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

        }
        public void SetCamRender(int lod, float height, float depth)
        {
            Vector3 pos = WaterPlaneControl.Instance.ringGrid[lod].transform.position;
            camGameObject.transform.position = new Vector3(pos.x, height + WaterPlaneControl.Instance.transform.position.y, pos.z);
            heightMapCam.farClipPlane = height + depth;

            heightMapCam.orthographicSize = WaterPlaneControl.Instance.GetLodSize(lod) / 2f;
        }
        public void SetCam(Camera orthoCamera, float size)
        {
            orthoCamera.orthographic = true;
            orthoCamera.aspect = 1;

            orthoCamera.orthographicSize = size;

            orthoCamera.enabled = false;
            orthoCamera.clearFlags = CameraClearFlags.SolidColor;

            orthoCamera.tag = "HigWater";
            orthoCamera.cameraType = CameraType.Game;
          
            var cameraData = orthoCamera.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData != null)
            {
                cameraData.volumeLayerMask = 0;
                cameraData.renderShadows = false;
            }
            orthoCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
        public Camera CreateTopDownOrthoCamera(float size)
        {
            GameObject cameraObject = camGameObject;
            Camera orthoCamera = null;
            if (cameraObject.GetComponent<Camera>() != null)
            {
                orthoCamera = cameraObject.GetComponent<Camera>();
            }
            else
            {
                orthoCamera = cameraObject.AddComponent<Camera>();
            }

            SetCam(orthoCamera, size);


            return orthoCamera;
        }
        public void OnDisable()
        {

        }
        public void ResetAllWaveDataInput()
        {
            waveDataInputs.Clear();
            foreach (WaveDataInput waveDataInput in FindObjectsOfType<WaveDataInput>())
            {
                if (waveDataInput.enabled && waveDataInput.gameObject.activeInHierarchy)
                {
                    AddWaveData(waveDataInput);
                }
            }
        }
        public override void ResetSystem()
        {
            if (camGameObject == null)
            {
                camGameObject = new GameObject("CamGameObject");
            }
            ResetCam();
            height2NormalKernel = heightToNormalCS.FindKernel("HeightToNormal");
            CameraManager.ChangeCameraRenderer(heightMapCam, OtherSettingManager.Instance.cameraRendererIndex);
            ResetAllWaveDataInput();

        }
        private void LateUpdate()
        {
        }
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}