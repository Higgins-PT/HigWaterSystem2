using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


namespace HigWaterSystem2
{
    public class SSAOManager : HigSingleInstance<SSAOManager>
    {
        public bool enable = true;
        public float SSAOShadowMin = 0.2f;
        public float SSAORadiusMin = 0.2f;
        public float SSAORadiusMax = 1f;
        public float SSAODistance = 100f;
        public float SSAOIntensity = 1f;
        public float SSAOIntensityFactor = 1f;
        public float sigma = 2f;
        public float normalDisturbanceIntensity;
        [Range(2, 64)]
        public int samplePointsCount = 16;
        public bool enableBlur;
        public float blurNormalThreshold = 0.8f;
        public float blurRangeFactor = 3f;
        [Range(0, 5)]
        public int resolutionScaleFactor;
        public Material SSAOMat;
        public Texture2D noiseTex;
        public Material fliterBlur;
        public Material randomMat;
        public RenderTexture randomNoise;
        private const int randomNoiseSize = 256;
        public ComputeShader noiseCS;
        private float[] _BlurWeights = new float[12]
{
        0.05f, 0.05f, 0.1f, 0.1f,
        0.1f, 0.2f, 0.1f, 0.1f,
        0.1f, 0.05f, 0.05f, 0.0f
};
        private Dictionary<Camera, int2> camLastSize = new Dictionary<Camera, int2>();

        public void SetDirNoise()
        {
            RenderTexture newNoise = new RenderTexture(randomNoiseSize, randomNoiseSize, 0, RenderTextureFormat.ARGBFloat)
            {
                dimension = TextureDimension.Tex3D,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat,
                useMipMap = false,
                autoGenerateMips = false,
                enableRandomWrite = true,
                volumeDepth = samplePointsCount
            };
            int kernel = noiseCS.FindKernel("SpawnNoise");
            noiseCS.SetTexture(kernel, "_Result", newNoise);
            noiseCS.SetTexture(kernel, "_NoiseTex", noiseTex);
            noiseCS.SetFloat("_ResultSize_X", newNoise.width);
            noiseCS.SetFloat("_ResultSize_Y", newNoise.height);
            noiseCS.SetFloat("_NoiseSize_X", noiseTex.width);
            noiseCS.SetFloat("_NoiseSize_Y", noiseTex.height);
            noiseCS.SetFloat("_Sigma", sigma);
            noiseCS.Dispatch(kernel, Mathf.CeilToInt(randomNoiseSize / 8), Mathf.CeilToInt(randomNoiseSize / 8), samplePointsCount);
            randomNoise = newNoise;
        }
        private void TryAddSize(Camera camera, int width, int height)
        {
            camLastSize?.Add(camera, new int2(width, height));
        }
        private void CheckCamSizeChange(Camera camera, int nowWidth, int nowHeight)
        {

            if (camLastSize.TryGetValue(camera, out int2 value))
            {
                int width = value.x;
                int height = value.y;
                if (value.x == nowWidth && value.y == nowHeight)
                {

                }
                else
                {
                    camLastSize.Remove(camera);

                    TryAddSize(camera, nowWidth, nowHeight);
                    RTManager.Instance.ReleaseRT("SSAODepth", width, height, RenderTextureFormat.ARGBFloat);
                    RTManager.Instance.ReleaseRT("SSAOTex", width, height, RenderTextureFormat.RFloat);
                    RTManager.Instance.ReleaseRT("SSAODepthTem", width, height, RenderTextureFormat.ARGBFloat);
                }
            }
            else
            {
                TryAddSize(camera, nowWidth, nowHeight);
            }
        }
        public override void ResetSystem()
        {
            try
            {
                RenderPipelineManager.beginCameraRendering -= RenderSSAO;
            }
            catch
            {

            }
            if (enable)
            {
                MatAttribute.SetInt("_SSAOEnable", 1);
                RenderPipelineManager.beginCameraRendering += RenderSSAO;
            }
            else
            {
                MatAttribute.SetInt("_SSAOEnable", 0);
            }
            try
            {
                randomNoise?.Release();

            }
            catch
            {

            }

            SetDirNoise();
            /*
            randomVec3RT = RTManager.Instance.GetRT("RandomRT", WaterPlaneControl.Instance.MainCamera.pixelWidth, WaterPlaneControl.Instance.MainCamera.pixelHeight, RenderTextureFormat.ARGBFloat, TextureWrapMode.Repeat);
            randomMat.SetFloat("_Sigma", sigma);
            randomMat.SetTexture("_NoiseTex", noiseTex);
            Graphics.Blit(null, randomVec3RT, randomMat);*/
        }
        public void OnDisable()
        {
            try
            {
                RenderPipelineManager.beginCameraRendering -= RenderSSAO;
            }
            catch
            {

            }
        }
        public override void DestroySystem()
        {
            try
            {
                RenderPipelineManager.beginCameraRendering -= RenderSSAO;
            }
            catch
            {

            }
        }


        public void RenderSSAO(ScriptableRenderContext context, Camera cam)
        {
            if (!(WaterPlaneControl.Instance.enableWaterPlane))
            {
                return;
            }
            if (cam.pixelWidth <= 0 || cam.pixelHeight <= 0)
            {
                return;
            }
#pragma warning disable CS0618
            if (!cam || !enabled || cam.tag == "HigWater") return;

            Profiler.BeginSample("SSAO_Water");
            int width = cam.pixelWidth >> resolutionScaleFactor;
            int height = cam.pixelHeight >> resolutionScaleFactor;
            CheckCamSizeChange(cam, width, height);

            RenderTexture depthTex = RTManager.Instance.GetRT("SSAODepth", width, height, RenderTextureFormat.ARGBFloat);
            Camera SSAOCam = CameraManager.Instance.GetCam(cam, "SSAO");

            SSAOCam.tag = "HigWater";
            CopyCameraAttribute(cam, SSAOCam);
            SSAOCam.cullingMask = OtherSettingManager.Instance.waterLayerMask;
            if (SSAOCam.targetTexture != depthTex)
            {
                SSAOCam.targetTexture = depthTex;
            }

            SSAOCam.clearFlags = CameraClearFlags.Nothing;
            SSAOCam.depthTextureMode = DepthTextureMode.Depth;
            CameraManager.ChangeCameraRenderer(SSAOCam, OtherSettingManager.Instance.cameraRendererIndex);
            MatAttribute.SetFloat("_SSAONormalDisturbanceIntensity", normalDisturbanceIntensity);
            Shader.SetGlobalTexture("_WaterDepthTexture", depthTex);
            Profiler.BeginSample("SSAO_Depth");
            MatAttribute.SwitchMod(MatAttribute.OceanRenderMod.SSAOMOD);
            UniversalRenderPipeline.RenderSingleCamera(context, SSAOCam);
            MatAttribute.SwitchMod(MatAttribute.OceanRenderMod.RENDERINGMOD);
            Profiler.EndSample();


            RenderTexture SSAOTex = RTManager.Instance.GetRT("SSAOTex", width, height, RenderTextureFormat.R8);
            CommandBuffer SSAOCmd = new CommandBuffer { name = "SSAO" };
            SSAOCmd.SetRenderTarget(SSAOTex);

            SSAOCmd.ClearRenderTarget(true, true, Color.clear);

            SSAOCmd.SetGlobalFloat("_SSAORadiusMin", SSAORadiusMin);
            SSAOCmd.SetGlobalFloat("_SSAORadiusMax", SSAORadiusMax);
            SSAOCmd.SetGlobalFloat("_SSAODistance", SSAODistance);
            SSAOCmd.SetGlobalFloat("_SSAOIntensity", SSAOIntensity);
            SSAOCmd.SetGlobalFloat("_SSAOIntensityFactor", SSAOIntensityFactor);
            SSAOCmd.SetGlobalFloat("_SSAOShadowMin", SSAOShadowMin);
            
            SSAOCmd.SetGlobalFloat("_Sigma", sigma);
            SSAOCmd.SetGlobalFloat("_NearPlane", SSAOCam.nearClipPlane);
            SSAOCmd.SetGlobalFloat("_FarPlane", SSAOCam.farClipPlane);

            SSAOCmd.SetGlobalInt("_SamplePointsCount", samplePointsCount);
            
            SSAOCmd.SetGlobalMatrix("_ViewMatrix", SSAOCam.worldToCameraMatrix);
            SSAOCmd.SetGlobalMatrix("_ProjectionMatrix", SSAOCam.projectionMatrix);
            SSAOCmd.SetGlobalMatrix("_InverViewMatrix", SSAOCam.cameraToWorldMatrix);
            SSAOCmd.SetGlobalMatrix("_InverProjectionMatrix", SSAOCam.projectionMatrix.inverse);

            SSAOCmd.SetGlobalTexture("_NormalDepthTex", depthTex);
            SSAOCmd.SetGlobalTexture("_NoiseTex", noiseTex);
            //SSAOCmd.SetGlobalTexture("_RandomRT", randomVec3RT);
            SSAOCmd.SetGlobalVector("_NoiseScale", new Vector2((float)randomNoise.width / (float)SSAOCam.pixelWidth, (float)randomNoise.height / (float)SSAOCam.pixelHeight));
            SSAOCmd.SetGlobalTexture("_RandomDirTex", randomNoise);
            SSAOCmd.Blit(depthTex, SSAOTex, SSAOMat);
            Profiler.BeginSample("SSAO_Blit");
            context.ExecuteCommandBuffer(SSAOCmd);
            context.Submit();
            Profiler.EndSample();
            SSAOCmd.Release();
            if (enableBlur)
            {
                CommandBuffer bilateralfilterCmd = new CommandBuffer { name = "Bilateral Filter" };
                RenderTexture temTex = RTManager.Instance.GetRT("SSAODepthTem", width, height, RenderTextureFormat.ARGBFloat);

                bilateralfilterCmd.SetRenderTarget(temTex);
                bilateralfilterCmd.ClearRenderTarget(true, true, Color.clear);
                bilateralfilterCmd.SetGlobalVector("_TexSize", new Vector4(width, height, 0, 0));
                bilateralfilterCmd.SetGlobalTexture("_NormalDepthTex", depthTex);
                bilateralfilterCmd.SetGlobalInt("_HorizontalBlur", 0);
                bilateralfilterCmd.SetGlobalInt("_BlurRadius", 5);
                bilateralfilterCmd.SetGlobalFloat("_SSAORadiusMin", SSAORadiusMin);
                bilateralfilterCmd.SetGlobalFloat("_SSAORadiusMax", SSAORadiusMax);
                bilateralfilterCmd.SetGlobalFloat("_SSAODistance", SSAODistance);
                bilateralfilterCmd.SetGlobalFloat("_BlurNormalThreshold", blurNormalThreshold);
                bilateralfilterCmd.SetGlobalFloat("_BlurRangeFactor", blurRangeFactor);
                
                bilateralfilterCmd.SetGlobalFloatArray("_BlurWeights", _BlurWeights);
                bilateralfilterCmd.Blit(SSAOTex, temTex, fliterBlur);
                bilateralfilterCmd.SetGlobalInt("_HorizontalBlur", 1);
                bilateralfilterCmd.Blit(temTex, SSAOTex, fliterBlur);
                context.ExecuteCommandBuffer(bilateralfilterCmd);
                context.Submit();
                bilateralfilterCmd.Release();
            }
            MatAttribute.SetTexture("_SSAOTex", SSAOTex);
#pragma warning restore CS0618
            Profiler.EndSample();
        }
        void CopyCameraAttribute(Camera src, Camera dest)
        {
            if (dest == null)
            {
                return;
            }
            dest.clearFlags = src.clearFlags;
            dest.backgroundColor = src.backgroundColor;
            if (src.clearFlags == CameraClearFlags.Skybox)
            {
                Skybox sky = src.GetComponent<Skybox>();
                Skybox mysky = dest.GetComponent<Skybox>();
                if (!sky || !sky.material)
                {
                    mysky.enabled = false;
                }
                else
                {
                    mysky.enabled = true;
                    mysky.material = sky.material;
                }
            }

            dest.farClipPlane = src.farClipPlane;
            dest.nearClipPlane = src.nearClipPlane;
            dest.orthographic = src.orthographic;
            dest.fieldOfView = src.fieldOfView;
            dest.aspect = src.aspect;
            dest.orthographicSize = src.orthographicSize;
            dest.transform.position = src.transform.position;
            dest.transform.rotation = src.transform.rotation;
            dest.worldToCameraMatrix = src.worldToCameraMatrix;
            dest.projectionMatrix = src.projectionMatrix;
        }
        void Start()
        {

        }


        void Update()
        {

        }

    }
}