using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HigWaterSystem2
{
    public class ReflectionManager : HigSingleInstance<ReflectionManager>
    {
        public enum LightType
        {
            off,
            fake,
            SSR
        }
        public Dictionary<Camera, Camera> reflectionCameras = new Dictionary<Camera, Camera>();
        public Dictionary<Camera, Camera> refractionCameras = new Dictionary<Camera, Camera>();
        public Dictionary<string, Cubemap> cubeMap = new Dictionary<string, Cubemap>();
        public LightType reflectionType = LightType.SSR;
        public LightType refractionType = LightType.fake;
        public Color SSRReflection_Color = Color.white;
        public Color SSRRefraction_Color = Color.white;
        public float SSRIntensity_Reflection = 2f;
        public float SSRIntensity_Refraction = 2f;
        public float distortionIntensity_Reflection = 2f;
        public float distortionIntensity_Refraction = 4f;
        public float refractionIntensity = 1f;
        public float reflectionIntensity = 1f;
        [Range(0.05f, 1)]
        public float SSRReflectRoughness = 0.35f;
        [Range(0.05f, 1)]
        public float SSRRefractRoughness = 0.05f;

        [Range(0f, 1)]
        public float SkyReflectRoughness = 1f;
        public float reflectClipOffest = 0f;
        public float SSRStep_Reflection = 150f;
        public float SSRStep_Refraction = 150f;
        
        public float SSRThickness_Reflection = 5f;
        public float SSRThickness_Refraction = 5f;
        [Range(0.05f, 1)]
        public float UnderWaterRefractRoughness = 1f;
        public float UnderWaterRefractIntensity = 1f;
        public float UnderWaterSSRIntensity_Refraction = 1f;
        public int maxMipCount = 4;

        public float maxSSRLenght = 1000;
        public bool enableCaustic = true;
        public float causticDistance = 10f;
        public float causticPow = 20;
        public float causticIntensity = 20;
        public Color causticColor = Color.white;
        [HideInInspector]
        public Material waterMat;
        [HideInInspector]
        public RenderTexture reflectionTex;
        [HideInInspector]
        public RenderTexture refractionTex;
        public int texSize = 256;
        public float fReflectionInitRatio = 0;
        public bool enableBlur;
        public float blurStrength = 1f;
        public Material gaussianBlur;
        public Material depthShader;
        public Material causticMat;
        public Material hizBufferMat;
        [HideInInspector]
        public float clipPlaneOffset = 0.07f;

        public RenderTexture CreateDepthRT(ScriptableRenderContext context, string rtName, Camera camera)
        {
            CommandBuffer depthCmd = new CommandBuffer { name = "RenderRT" };
            RenderTexture finDepthTex = RTManager.Instance.GetRT(rtName, camera.pixelWidth, camera.pixelHeight, RenderTextureFormat.RFloat);
            depthCmd.Blit(reflectionTex, finDepthTex, depthShader);

            context.ExecuteCommandBuffer(depthCmd);
            context.Submit();
            depthCmd.Release();
            return finDepthTex;
        }
        public RenderTexture CreateHizBuffer(ScriptableRenderContext context, string hizBufferName, Camera camera, RenderTexture depthRT)
        {
            int mipCount = (int)Mathf.Log(texSize, 2);
            List<RenderTexture> temRenderTexture = new List<RenderTexture>();
            RenderTexture finHizDepthTex = RTManager.Instance.GetRT(hizBufferName, texSize, RenderTextureFormat.RGFloat, mipCount, FilterMode.Point);
            for (int i = 0; i < mipCount; i++)
            {
                temRenderTexture.Add(RTManager.Instance.GetRT("temMipMap", texSize >> i, RenderTextureFormat.RGFloat, FLM: FilterMode.Point));
            }

            CommandBuffer postProcessCmd = new CommandBuffer { name = "HizBuffer" };
            float x = 1 - camera.farClipPlane / camera.nearClipPlane;
            float y = camera.farClipPlane / camera.nearClipPlane;
            depthShader.SetVector("_Zbuffer", new Vector4(x, y, x / camera.farClipPlane, y / camera.farClipPlane));
            postProcessCmd.Blit(depthRT, temRenderTexture[0], depthShader);
            for (int i = 1; i < mipCount; i++)
            {
                postProcessCmd.SetGlobalInt("_Size", texSize >> (i - 1));
                postProcessCmd.SetGlobalTexture("_LastMipMap", temRenderTexture[i - 1]);
                postProcessCmd.Blit(temRenderTexture[i - 1], temRenderTexture[i], hizBufferMat);

            }
            for (int i = 0; i < mipCount; i++)
            {
                postProcessCmd.CopyTexture(temRenderTexture[i], 0, 0, finHizDepthTex, 0, i);
                temRenderTexture[i].Release();
            }

            context.ExecuteCommandBuffer(postProcessCmd);
            context.Submit();
            postProcessCmd.Release();
            return finHizDepthTex;
        }
        public void TryRenderCubeMap(Camera cam)
        {
            string realName = RTManager.Instance.GetCB_Name("SkyBoxCB" + cam.GetInstanceID().ToString(), texSize, TextureFormat.RGBA32);

            if (!cubeMap.ContainsKey(realName))
            {
                Cubemap cubemap = RTManager.Instance.GetCB("SkyBoxCB" + cam.GetInstanceID().ToString(), texSize, TextureFormat.RGBA32);
                cubeMap.Add(realName, cubemap);
                CameraClearFlags cameraClearFlags = cam.clearFlags;
                string tag = cam.tag;
                cam.tag = "HigWater";
                int mask = cam.cullingMask;
                cam.clearFlags = CameraClearFlags.Skybox;
                cam.cullingMask = 0;

                cam.RenderToCubemap(cubemap);

                cam.clearFlags = cameraClearFlags;
                cam.cullingMask = mask;
                cam.tag = tag;
            }
        }
        public static List<Camera> GetAllCameras()
        {
            List<Camera> allCameras = new List<Camera>();

            allCameras.AddRange(Camera.allCameras);
#if UNITY_EDITOR
            SceneView[] sceneViews = SceneView.sceneViews.ToArray(typeof(SceneView)) as SceneView[];
            foreach (SceneView sceneView in sceneViews)
            {
                if (sceneView.camera != null)
                {
                    allCameras.Add(sceneView.camera);
                }
            }
#endif

            return allCameras;
        }
        public void Update()
        {
            if (!Mathf.Approximately(Mathf.Log(texSize, 2), Mathf.Round(Mathf.Log(texSize, 2))))
            {
                return;
            }
            foreach (var cam in GetAllCameras())
            {
                if (cam.gameObject.activeSelf && cam.enabled == true && cam.gameObject.tag != "HigWater")
                {
                    TryRenderCubeMap(cam);
                }
            }
        }

        public void RenderReflection(ScriptableRenderContext context, Camera cam)
        {
#pragma warning disable CS0618
            if (!(WaterPlaneControl.Instance.enableWaterPlane))
            {
                return;
            }
            if (!Mathf.Approximately(Mathf.Log(texSize, 2), Mathf.Round(Mathf.Log(texSize, 2))))
            {
                return;
            }
            if (cam.pixelWidth <= 0 || cam.pixelHeight <= 0)
            {
                return;
            }
            if (!cam || !enabled) return;
            if (reflectionType == LightType.off && reflectionType == LightType.off)
            {
                MatAttribute.SetInt("_SSR_ON", 0);
                return;
            }
            else
            {
                MatAttribute.SetInt("_SSR_ON", 1);
            }
            Camera reflectionCamera, refractionCamera;
            GetCaculateCam(cam, out reflectionCamera, out refractionCamera);
            CopyCameraAttribute(cam, reflectionCamera);
            CopyCameraAttribute(cam, refractionCamera);
            reflectionCamera.tag = "HigWater";
            refractionCamera.tag = "HigWater";

            Vector3 pos = transform.position;
            pos.y = WaterPlaneControl.Instance.waterPlaneHeight + WaterPlaneControl.Instance.cameraHeightOffest;
            Vector3 normal = transform.up;

            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(normal, pos));
            Matrix4x4 reflectionMat = Matrix4x4.zero;
            CalculateReflectionMatrix(ref reflectionMat, reflectionPlane);
            Vector3 lastPos = cam.transform.position;
            Vector3 camPos = reflectionMat.MultiplyPoint(lastPos);



            Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
            Vector3 euler = cam.transform.eulerAngles;
            reflectionCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);
            reflectionCamera.transform.position = lastPos;

            Shader.SetGlobalInt("_ReflectionIng", 1);
            Shader.SetGlobalMatrix("_ViewMatrixH", cam.worldToCameraMatrix);
            Shader.SetGlobalMatrix("_ProjectionMatrixH", cam.projectionMatrix);
            Shader.SetGlobalMatrix("_InverseProjectionMatrixH", cam.projectionMatrix.inverse);
            Shader.SetGlobalMatrix("_InverseViewMatrixH", cam.cameraToWorldMatrix);
            Shader.SetGlobalVector("_ScreenDimensionsH", new Vector2(Screen.width, Screen.height));
            if (reflectionType == LightType.fake)
            {
                reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflectionMat;
                reflectionCamera.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);
                reflectionCamera.cullingMatrix = cam.projectionMatrix * cam.worldToCameraMatrix;
            }
            reflectionCamera.cullingMask = OtherSettingManager.Instance.allowedLayers.value;
            if (reflectionCamera.targetTexture != reflectionTex)
            {
                reflectionCamera.targetTexture = reflectionTex;
            }


            bool oldCulling = GL.invertCulling;


            reflectionCamera.transform.position = camPos;


            GL.invertCulling = oldCulling;

            reflectionCamera.depthTextureMode = DepthTextureMode.Depth;
            if (reflectionType != LightType.off)
            {
                UniversalRenderPipeline.RenderSingleCamera(context, reflectionCamera);
            }
            MatAttribute.SetTexture("_ReflectionTex", reflectionTex);
            RenderTexture reflectionHiz = null;
            if (reflectionType == LightType.SSR)
            {
                RenderTexture depthTextureRefl = Shader.GetGlobalTexture("_CameraDepthTexture") as RenderTexture;

                reflectionHiz = CreateHizBuffer(context, "reflectionHiz", reflectionCamera, depthTextureRefl);
            }





            refractionCamera.cullingMask = OtherSettingManager.Instance.allowedLayers.value;
            refractionCamera.targetTexture = refractionTex;
            refractionCamera.transform.position = cam.transform.position;
            refractionCamera.transform.rotation = cam.transform.rotation;
            Shader.SetGlobalMatrix("_ViewMatrixH", cam.worldToCameraMatrix);
            Shader.SetGlobalMatrix("_ProjectionMatrixH", cam.projectionMatrix);
            Shader.SetGlobalMatrix("_InverseProjectionMatrixH", cam.projectionMatrix.inverse);
            Shader.SetGlobalMatrix("_InverseViewMatrixH", cam.cameraToWorldMatrix);
            Shader.SetGlobalVector("_ScreenDimensionsH", new Vector2(Screen.width, Screen.height));
            if (refractionType != LightType.off)
            {
                UniversalRenderPipeline.RenderSingleCamera(context, refractionCamera);

            }
            MatAttribute.SetTexture("_RefractionTex", refractionTex);
            RenderTexture refractionHiz = null;
            RenderTexture depthTextureRefr = Shader.GetGlobalTexture("_CameraDepthTexture") as RenderTexture;
            refractionHiz = CreateHizBuffer(context, "refractionHiz", refractionCamera, depthTextureRefr);
            RenderTexture refractionSceneTex = refractionTex;
            if (refractionType == LightType.SSR || refractionType == LightType.fake)
            {
                MatAttribute.SetTexture("_CameraDepth", refractionHiz);
                if (enableCaustic)
                {
                    RenderTexture causticRT = RTManager.Instance.GetRT("CausticRT", refractionTex.width, refractionTex.height, refractionTex.format);
                    CommandBuffer CausticCmd = new CommandBuffer { name = "Caustic" };
                    CausticCmd.SetGlobalTexture("_RefractionTex", refractionTex);
                    CausticCmd.SetGlobalTexture("_CameraDepth", refractionHiz);
                    CausticCmd.SetGlobalMatrix("_ProjMat", cam.projectionMatrix);
                    CausticCmd.SetGlobalMatrix("_InverViewMat", cam.worldToCameraMatrix.inverse);
                    CausticCmd.SetGlobalFloat("_CausticPow", causticPow);
                    CausticCmd.SetGlobalFloat("_CausticIntensity", causticIntensity);
                    CausticCmd.SetGlobalFloat("_CausticDistance", causticDistance);
                    
                    CausticCmd.SetGlobalColor("_CausticColor", causticColor);
                    CausticCmd.Blit(refractionTex, causticRT, causticMat);
                    refractionSceneTex = causticRT;
                    context.ExecuteCommandBuffer(CausticCmd);
                    context.Submit();
                    CausticCmd.Release();
                }
            }


            //------------------------------------------------------------------SSR

            SSR(refractionCamera, cam, reflectionCamera, context, reflectionHiz, reflectionTex, refractionHiz, refractionSceneTex);
#pragma warning restore  CS0618
        }

        public void SSR(Camera refractionCamera, Camera mainCam, Camera reflectionCamera, ScriptableRenderContext context, RenderTexture hizBuffer_Refl, RenderTexture hizBuffer_Screen_Refl, RenderTexture hizBuffer_Refr, RenderTexture hizBuffer_Screen_Refr)
        {
            CommandBuffer SSRCmd = new CommandBuffer { name = "Scene Space Relfection" };
            Camera SSRCam = CameraManager.Instance.GetCam(mainCam,"SSRCam");
            CopyCameraAttribute(mainCam, SSRCam);
            CameraManager.ChangeCameraRenderer(SSRCam, OtherSettingManager.Instance.cameraRendererIndex);
            SSRCam.transform.position = mainCam.transform.position;
            SSRCam.transform.rotation = mainCam.transform.rotation;
            RenderTexture ssrTex = RTManager.Instance.GetRT("SSR", texSize, RenderTextureFormat.ARGBFloat);
            refractionCamera.projectionMatrix = mainCam.projectionMatrix;
            refractionCamera.cullingMask = OtherSettingManager.Instance.waterLayerMask.value;
            if (SSRCam.targetTexture != ssrTex)
            {
                SSRCam.targetTexture = ssrTex;
            }
            SSRCmd.SetRenderTarget(ssrTex);
            SSRCmd.ClearRenderTarget(true, true, Color.clear);

            SSRCmd.SetViewMatrix(mainCam.worldToCameraMatrix);
            SSRCmd.SetProjectionMatrix(mainCam.projectionMatrix);
            context.ExecuteCommandBuffer(SSRCmd);
            context.Submit();

            SSRCmd.Release();

            MatAttribute.SetFloat("_Size", texSize);
            MatAttribute.SetInt("_TexSize", texSize);

            if (reflectionType == LightType.SSR)
            {
                MatAttribute.SetInt("_MipCount", Mathf.Min(hizBuffer_Refl.mipmapCount, maxMipCount));
            }
            else if (refractionType == LightType.SSR)
            {
                MatAttribute.SetInt("_MipCount", Mathf.Min(hizBuffer_Refr.mipmapCount, maxMipCount));
            }
            MatAttribute.SetColor("_SSRReflection_Color", SSRReflection_Color);
            MatAttribute.SetColor("_SSRRefraction_Color", SSRRefraction_Color);
            MatAttribute.SetFloat("_FReflectionInitRatio", fReflectionInitRatio);

            MatAttribute.SetFloat("_DistortionIntensity_Re", distortionIntensity_Reflection);
            MatAttribute.SetFloat("_SSRStep_Refl", SSRStep_Reflection);
            MatAttribute.SetFloat("_SSRThickness_Refl", SSRThickness_Reflection);

            MatAttribute.SetFloat("_RefractionIntensity", refractionIntensity);
            MatAttribute.SetFloat("_ReflectionIntensity", reflectionIntensity);

            MatAttribute.SetFloat("_DistortionIntensity_Refr", distortionIntensity_Refraction);
            MatAttribute.SetFloat("_SSRStep_Refr", SSRStep_Refraction);
            MatAttribute.SetFloat("_SSRThickness_Refr", SSRThickness_Refraction);
            MatAttribute.SetFloat("_SSRReflectRoughness", SSRReflectRoughness);
            MatAttribute.SetFloat("_SSRRefractRoughness", SSRRefractRoughness);
            
            MatAttribute.SetFloat("_SkyReflectRoughness", SkyReflectRoughness);
            MatAttribute.SetFloat("_DepthMax", maxSSRLenght);
            MatAttribute.SetFloat("_SSRIntensity_Reflection", SSRIntensity_Reflection);
            MatAttribute.SetFloat("_SSRIntensity_Refraction", SSRIntensity_Refraction);
            MatAttribute.SetFloat("_ReflectClipOffest", reflectClipOffest);
            MatAttribute.SetFloat("_UnderWaterRefractRoughness", UnderWaterRefractRoughness);
            MatAttribute.SetFloat("_UnderWaterRefractIntensity", UnderWaterRefractIntensity);
            MatAttribute.SetFloat("_UnderWaterSSRIntensity_Refraction", UnderWaterSSRIntensity_Refraction);
            
            MatAttribute.SetMatrix("_ViewMatrixRe", reflectionCamera.worldToCameraMatrix);
            MatAttribute.SetMatrix("_ProjectionMatrixRe", reflectionCamera.projectionMatrix);
            MatAttribute.SetMatrix("_InverViewMatrixRe", reflectionCamera.worldToCameraMatrix.inverse);
            MatAttribute.SetMatrix("_InverProjectionMatrixRe", reflectionCamera.projectionMatrix.inverse);

            MatAttribute.SetMatrix("_ViewMatrixRefra", refractionCamera.worldToCameraMatrix);
            MatAttribute.SetMatrix("_ProjectionMatrixRefra", refractionCamera.projectionMatrix);
            MatAttribute.SetMatrix("_InverViewMatrixRefra", refractionCamera.worldToCameraMatrix.inverse);
            MatAttribute.SetMatrix("_InverProjectionMatrixRefra", refractionCamera.projectionMatrix.inverse);

            MatAttribute.SetTexture("_ReflectionHizBuffer", hizBuffer_Refl);
            MatAttribute.SetTexture("_ReflectionScreenTex", hizBuffer_Screen_Refl);

            MatAttribute.SetTexture("_RefractionHizBuffer", hizBuffer_Refr);
            MatAttribute.SetTexture("_RefractionScreenTex", hizBuffer_Screen_Refr);
            MatAttribute.SetVector("_CamPos_Refr", refractionCamera.transform.position);
            MatAttribute.SetVector("_CamPos_Refl", reflectionCamera.transform.position);
            Cubemap cubemap = RTManager.Instance.GetCB("SkyBoxCB" + mainCam.GetInstanceID().ToString(), texSize, TextureFormat.RGBA32);
            if (cubemap != null)
            {
                MatAttribute.SetTexture("_SkyBox", cubemap);




                MatAttribute.DisableKeyword("REFLECTION_OFF");
                MatAttribute.DisableKeyword("REFLECTION_FAKE");
                MatAttribute.DisableKeyword("REFLECTION_SSR");
                if (reflectionType == LightType.off)
                {
                    MatAttribute.EnableKeyword("REFLECTION_OFF");
                }
                else if (reflectionType == LightType.fake)
                {
                    MatAttribute.EnableKeyword("REFLECTION_FAKE");
                }
                else if (reflectionType == LightType.SSR)
                {
                    MatAttribute.EnableKeyword("REFLECTION_SSR");
                }

                MatAttribute.DisableKeyword("REFRACTION_OFF");
                MatAttribute.DisableKeyword("REFRACTION_FAKE");
                MatAttribute.DisableKeyword("REFRACTION_SSR");
                if (refractionType == LightType.off)
                {
                    MatAttribute.EnableKeyword("REFRACTION_OFF");
                }
                else if (refractionType == LightType.fake)
                {
                    MatAttribute.EnableKeyword("REFRACTION_FAKE");
                }
                else if (refractionType == LightType.SSR)
                {
                    MatAttribute.EnableKeyword("REFRACTION_SSR");
                }
#pragma warning disable CS0618
                MatAttribute.SwitchMod(MatAttribute.OceanRenderMod.SSRMOD);
                 
                UniversalRenderPipeline.RenderSingleCamera(context, SSRCam);
                MatAttribute.SwitchMod(MatAttribute.OceanRenderMod.RENDERINGMOD);
#pragma warning restore CS0618
                if (enableBlur)
                {
                    CommandBuffer gaussianBlurCmd = new CommandBuffer { name = "Gaussian Blur" };
                    RenderTexture temTex = RTManager.Instance.GetRT("temTex", texSize, RenderTextureFormat.ARGBFloat);

                    
                    gaussianBlurCmd.SetGlobalFloatArray("_GaussianKernel", FlattenKernel(GenerateGaussianKernel(3, blurStrength)));
                    gaussianBlurCmd.SetRenderTarget(temTex);
                    gaussianBlurCmd.ClearRenderTarget(true, true, Color.clear);
                    gaussianBlurCmd.Blit(ssrTex, temTex, gaussianBlur);
                    gaussianBlurCmd.Blit(temTex, ssrTex);
                    context.ExecuteCommandBuffer(gaussianBlurCmd);
                    context.Submit();
                    gaussianBlurCmd.Release();
                }
                MatAttribute.SetTexture("_SSRTex", ssrTex);
            }
        }
        void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMat.m01 = (-2F * plane[0] * plane[1]);
            reflectionMat.m02 = (-2F * plane[0] * plane[2]);
            reflectionMat.m03 = (-2F * plane[3] * plane[0]);

            reflectionMat.m10 = (-2F * plane[1] * plane[0]);
            reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMat.m12 = (-2F * plane[1] * plane[2]);
            reflectionMat.m13 = (-2F * plane[3] * plane[1]);

            reflectionMat.m20 = (-2F * plane[2] * plane[0]);
            reflectionMat.m21 = (-2F * plane[2] * plane[1]);
            reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMat.m23 = (-2F * plane[3] * plane[2]);

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;
        }
        float[] FlattenKernel(float[,] kernel)
        {
            int size = kernel.GetLength(0);
            float[] flatKernel = new float[size * size];
            int index = 0;

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    flatKernel[index++] = kernel[i, j];
                }
            }

            return flatKernel;
        }
        static float[,] GenerateGaussianKernel(int size, float sigma)
        {
            float[,] kernel = new float[size, size];
            float sum = 0.0f;
            int halfSize = size / 2;
            float sigma2 = 2 * sigma * sigma;

            for (int x = -halfSize; x <= halfSize; x++)
            {
                for (int y = -halfSize; y <= halfSize; y++)
                {
                    float exponent = -(x * x + y * y) / sigma2;
                    kernel[x + halfSize, y + halfSize] = (float)(Mathf.Exp(exponent) / (Mathf.PI * sigma2));
                    sum += kernel[x + halfSize, y + halfSize];
                }
            }

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    kernel[x, y] /= sum;
                }
            }

            return kernel;
        }
        Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + normal * clipPlaneOffset;
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
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
        }
        public void GetCaculateCam(Camera camera, out Camera reflectionCamera, out Camera refractionCamera)
        {
            if (reflectionTex)
            {
                DestroyImmediate(reflectionTex);
            }
            reflectionTex = new RenderTexture(texSize, texSize, 16);
            reflectionTex.name = "_WaterReflection" + GetInstanceID();
            reflectionTex.isPowerOfTwo = true;
            reflectionTex.hideFlags = HideFlags.DontSave;
            reflectionTex.filterMode = FilterMode.Trilinear;
            if (refractionTex)
            {
                DestroyImmediate(refractionTex);
            }
            refractionTex = new RenderTexture(texSize, texSize, 16);
            refractionTex.name = "_WaterRefractionTex" + GetInstanceID();
            refractionTex.isPowerOfTwo = true;
            refractionTex.hideFlags = HideFlags.DontSave;
            refractionTex.filterMode = FilterMode.Trilinear;


            reflectionCameras.TryGetValue(camera, out reflectionCamera);
            if (!reflectionCamera)
            {
                GameObject cam = new GameObject("reflectionCamera" + camera.GetInstanceID(), typeof(Camera), typeof(Skybox), typeof(UniversalAdditionalCameraData));
                cam.gameObject.SetActive(false);
                reflectionCamera = cam.GetComponent<Camera>();
                reflectionCamera.enabled = false;
                CameraManager.ChangeCameraRenderer(reflectionCamera, OtherSettingManager.Instance.cameraRendererIndex);
                reflectionCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
                reflectionCameras[camera] = reflectionCamera;
            }

            refractionCameras.TryGetValue(camera, out refractionCamera);
            if (!refractionCamera)
            {
                GameObject cam = new GameObject("refractionCamera" + camera.GetInstanceID(), typeof(Camera), typeof(Skybox), typeof(UniversalAdditionalCameraData));
                cam.gameObject.SetActive(false);
                refractionCamera = cam.GetComponent<Camera>();
                refractionCamera.enabled = false;
                CameraManager.ChangeCameraRenderer(refractionCamera, OtherSettingManager.Instance.cameraRendererIndex);
                refractionCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
                refractionCameras[camera] = refractionCamera;
            }
        }
        public override void DestroySystem()
        {
            Destory();
        }
        public void Destory()
        {
            if (reflectionTex)
            {
                DestroyImmediate(reflectionTex);
                reflectionTex = null;
            }
            if (refractionTex)
            {
                DestroyImmediate(refractionTex);
                refractionTex = null;
            }
            foreach (var cam in reflectionCameras)
            {
                DestroyImmediate((cam.Value).gameObject);
            }
            reflectionCameras.Clear();
            foreach (var cam in refractionCameras)
            {
                DestroyImmediate((cam.Value).gameObject);
            }
            refractionCameras.Clear();
            foreach (var cube in cubeMap)
            {
                RTManager.Instance.ReleaseCB(cube.Value);
            }
            cubeMap.Clear();
            try
            {
                RenderPipelineManager.beginCameraRendering -= RenderReflection;
            }
            catch
            {

            }
        }
        public void OnDisable()
        {
            try
            {
                RenderPipelineManager.beginCameraRendering -= RenderReflection;
            }
            catch
            {

            }
        }
        public override void ResetSystem()
        {
            Destory();
            RenderPipelineManager.beginCameraRendering += RenderReflection;

        }


    }

}