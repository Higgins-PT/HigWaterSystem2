using System;

using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HigWaterSystem2
{
    [ExecuteInEditMode]
    public class UnderWaterRenderer : MonoBehaviour
    {
        public GameObject maskObj;
        public float MaskBasicScale { get { return WaterPlaneControl.Instance.GridMinScale * Mathf.Pow(2, maskLod); } }
        public float maskDepth = 1000f;
        [Range(0, 9)]
        public int maskLod = 2;
        private Material maskMat;
        private CommandBuffer postCmd;
        private Camera cam;
        private int2 lastSize;
        public float offestClip = 20f;
        public bool dynamicControlUnderWaterRendererAction = false;
        public float dynamicThreshold = 3f;
        private bool action = false;
        private RenderTexture waterMaskTex;
        private RenderTexture depthMaskTex;
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
        public static void GetNearPlaneYExtremes(Camera cam, out float minY, out float maxY)
        {


            Vector3[] frustumCorners = new Vector3[4];


            cam.CalculateFrustumCorners(
                new Rect(0, 0, 1, 1),            
                cam.nearClipPlane,             
                Camera.MonoOrStereoscopicEye.Mono,
                frustumCorners);

            minY = float.MaxValue;
            maxY = float.MinValue;

            for (int i = 0; i < 4; i++)
            {
                Vector3 worldSpaceCorner = cam.transform.TransformPoint(frustumCorners[i]);
                float y = worldSpaceCorner.y;

                // Update min and max Y
                if (y < minY)
                    minY = y;
                if (y > maxY)
                    maxY = y;
            }
        }
        public bool TestSize(int width, int height)
        {
            if (lastSize.x == width && lastSize.y == height)
            {
                return true;
            }
            else
            {
                lastSize = new int2(width, height);
                return false;
            }
        }
        public void SetRenderMatrix(Matrix4x4 projectionMatrix, Matrix4x4 viewMatrix, CommandBuffer cmd)
        {
            cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

        }

        public void RenderWaterMask(ScriptableRenderContext context, Camera cam)
        {
            if (!action)
            {
                return;
            }
            if (!(WaterRenderingAttributeManager.Instance._UnderWaterEnable && WaterPlaneControl.Instance.enableWaterPlane))
            {
                return;
            }
            maskObj.transform.rotation = Quaternion.identity;
            Vector3 worldPos = transform.position;
            worldPos.y = WaterPlaneControl.Instance.waterPlaneHeight;
            maskObj.transform.position = worldPos;


            Camera DepthCam = CameraManager.Instance.GetCam(cam, "DepthMask");
            Camera WaterMaskCam = CameraManager.Instance.GetCam(cam, "WaterMask");

            CameraManager.ChangeCameraRenderer(DepthCam, OtherSettingManager.Instance.cameraRendererIndex);
            CameraManager.ChangeCameraRenderer(WaterMaskCam, OtherSettingManager.Instance.cameraRendererIndex);
            CopyCameraAttribute(cam, WaterMaskCam);
            CopyCameraAttribute(cam, DepthCam);
            int width = WaterMaskCam.pixelWidth;
            int height = WaterMaskCam.pixelHeight;
            if (!TestSize(width, height))
            {
                RTManager.Instance.ReleaseRT(waterMaskTex);
                RTManager.Instance.ReleaseRT(depthMaskTex);
            }
            waterMaskTex = RTManager.Instance.GetRT("WaterMask", width, height, RenderTextureFormat.RFloat);
            depthMaskTex = RTManager.Instance.GetRT("depthMaskTex", width, height, RenderTextureFormat.RGFloat);
            RenderTexture backWaterPlaneTemRT = RTManager.Instance.GetRT("BackWaterPlaneTemRT", width, height, RenderTextureFormat.RFloat);
            RenderTexture underWaterMaskTemRT = RTManager.Instance.GetRT("UnderWaterMaskTemRT", width, height, RenderTextureFormat.RFloat);
            var originalLayer = maskObj.layer;
            WaterMaskCam.targetTexture = backWaterPlaneTemRT;
            WaterMaskCam.cullingMask = OtherSettingManager.Instance.waterLayerMask;

            maskObj.layer = LayerMask.NameToLayer("Water");
            var originalClearFlags = WaterMaskCam.clearFlags;

            WaterMaskCam.clearFlags = CameraClearFlags.Depth;
            WaterMaskCam.backgroundColor = Color.black;
            MatAttribute.SwitchMod(MatAttribute.OceanRenderMod.WATERMASKMOD);
#pragma warning disable CS0618
            UniversalRenderPipeline.RenderSingleCamera(context, WaterMaskCam);
            maskObj.GetComponent<MeshRenderer>().enabled = true;
            CommandBuffer blitBuffer = new CommandBuffer();
            blitBuffer.name = "Render Under Water Mask CommandBuffer";
            blitBuffer.SetRenderTarget(underWaterMaskTemRT);
            blitBuffer.ClearRenderTarget(true, true, Color.black);
            SetRenderMatrix(WaterMaskCam.projectionMatrix, WaterMaskCam.worldToCameraMatrix, blitBuffer);
            blitBuffer.DrawRenderer(maskObj.GetComponent<MeshRenderer>(), maskObj.GetComponent<MeshRenderer>().sharedMaterial);


            context.ExecuteCommandBuffer(blitBuffer);
            context.Submit();
            blitBuffer.Release();

            maskObj.GetComponent<MeshRenderer>().enabled = false;
            maskObj.layer = originalLayer;
            DepthCam.cullingMask = OtherSettingManager.Instance.waterLayerMask;
            DepthCam.clearFlags = CameraClearFlags.SolidColor;
            DepthCam.backgroundColor = Color.black;
            if (DepthCam.targetTexture != depthMaskTex)
            {
                DepthCam.targetTexture = depthMaskTex;
            }


            MatAttribute.SwitchMod(MatAttribute.OceanRenderMod.DEPTHMASKMOD);
            UniversalRenderPipeline.RenderSingleCamera(context, DepthCam);

#pragma warning restore CS0618

            MatAttribute.SwitchMod(MatAttribute.OceanRenderMod.RENDERINGMOD);
            blitBuffer = new CommandBuffer();
            blitBuffer.name = "Render Blit CommandBuffer";
            blitBuffer.SetGlobalTexture("_DepthMaskTex", depthMaskTex);
            blitBuffer.SetGlobalTexture("_BackWaterPlaneTemRT", backWaterPlaneTemRT);
            blitBuffer.SetGlobalTexture("_UnderWaterMaskTemRT", underWaterMaskTemRT);
            blitBuffer.SetGlobalFloat("_OffestClip", offestClip);
            blitBuffer.Blit(backWaterPlaneTemRT, waterMaskTex, UnderWaterManager.Instance.blitMat);
            context.ExecuteCommandBuffer(blitBuffer);
            context.Submit();
            blitBuffer.Release();

        }
        public void ResetSystem()
        {
            DestroyImmediate(maskObj);
            SpawnUnderWaterMaskMesh();
        }
        public void Update()
        {
            if (dynamicControlUnderWaterRendererAction && WaterPlaneControl.Instance.enableDynamicHeightDetection)
            {
                GetNearPlaneYExtremes(cam, out float minY, out float maxY);
                if (minY + WaterPlaneControl.Instance.CamH - dynamicThreshold - cam.transform.position.y> 0)
                {
                    action = false;
                }
                else
                {
                    action = true;
                }

            }
            else
            {
                action = true;
            }
        }
        public void PostUnderWaterCam(ScriptableRenderContext context, Camera cam, RTHandle mainCamRT)
        {
            if (!action)
            {
                return;
            }
            if (mainCamRT == null && UnderWaterManager.Instance.renderType == UnderWaterManager.RenderType.RenderFeature)
            {
                return;
            }
            if (!(WaterRenderingAttributeManager.Instance._UnderWaterEnable && WaterPlaneControl.Instance.enableWaterPlane))
            {
                return;
            }
            CommandBuffer cmd = new CommandBuffer();
            cmd.name = "UnderWaterPostProcessing";

            float x = 1 - cam.farClipPlane / cam.nearClipPlane;
            float y = cam.farClipPlane / cam.nearClipPlane;
            cmd.SetGlobalVector("_Zbuffer", new Vector4(x, y, x / cam.farClipPlane, y / cam.farClipPlane));

            RenderTexture depthTex = RTManager.Instance.GetRT("depthTex", lastSize.x, lastSize.y, RenderTextureFormat.RGFloat);
            RenderTexture grab = RTManager.Instance.GetRT("grabPass", cam.pixelWidth, cam.pixelHeight, RenderTextureFormat.ARGBFloat);
            if (UnderWaterManager.Instance.renderType == UnderWaterManager.RenderType.RenderPipelineManager)
            {
                cmd.Blit(BuiltinRenderTextureType.CameraTarget, grab);
            }
            else
            {
                cmd.Blit(mainCamRT, grab);
            }

            cmd.Blit(depthMaskTex, depthTex, UnderWaterManager.Instance.depthShader);

            cmd.Blit(depthTex, depthMaskTex);

            context.ExecuteCommandBuffer(cmd);
            context.Submit();
            cmd.Release();

            postCmd.Clear();
            RenderTexture tempTexture = RTManager.Instance.GetRT("WaterMask", cam.pixelWidth, cam.pixelHeight, RenderTextureFormat.ARGBFloat);

            postCmd.name = "UnderWaterPostProcessing";



            postCmd.Blit(BuiltinRenderTextureType.CameraTarget, tempTexture);

            postCmd.SetGlobalTexture("_DepthMask", depthMaskTex);
            postCmd.SetGlobalTexture("_WaterMask", waterMaskTex);
            postCmd.SetGlobalTexture("_GrabPass", grab);
            postCmd.SetGlobalFloat("_UnderWaterViewDistance", UnderWaterManager.Instance.underWaterViewDistance);
            postCmd.SetGlobalFloat("_UnderWaterPow", UnderWaterManager.Instance.underWaterPow);
            postCmd.SetGlobalColor("_UnderWaterColor", UnderWaterManager.Instance.underWaterColor);
            postCmd.SetGlobalFloat("_UnderWaterLightPow", UnderWaterManager.Instance.underWaterLightPow);
            postCmd.SetGlobalFloat("_UnderWaterLightIntensity", UnderWaterManager.Instance.underWaterLightIntensity);
            postCmd.SetGlobalFloat("_UnderWaterLightBasic", UnderWaterManager.Instance.underWaterLightBasic);
            postCmd.SetGlobalFloat("_SeabedLightBasic", UnderWaterManager.Instance.seabedLightBasic);
            postCmd.SetGlobalFloat("_SeabedLightPow", UnderWaterManager.Instance.seabedLightPow);
            postCmd.SetGlobalFloat("_CameraDepth", WaterPlaneControl.Instance.camDepth);
            postCmd.SetGlobalColor("_UnderWaterLightColor", UnderWaterManager.Instance.underWaterLightColor);
            postCmd.SetGlobalFloat("_AbsorptivityOfWater", UnderWaterManager.Instance.absorptivityOfWater);
            
            postCmd.SetGlobalMatrix("_InverseProjectionMatrix", cam.projectionMatrix.inverse);
            postCmd.SetGlobalMatrix("_InverseViewMatrix", cam.worldToCameraMatrix.inverse);
            postCmd.SetGlobalFloat("_DepthColorDistance", UnderWaterManager.Instance.depthColorDistance);
            postCmd.SetGlobalFloat("_DepthColorFactor", UnderWaterManager.Instance.depthColorFactor);
            postCmd.SetGlobalVectorArray("_DepthColor", UnderWaterManager.Instance.depthColor);
            Light mainLight = RenderSettings.sun;

            if (mainLight != null && mainLight.type == LightType.Directional)
            {
                Vector3 lightDirection = -mainLight.transform.forward;
                postCmd.SetGlobalVector("_MainLightPosition", lightDirection);
            }
            else
            {
                postCmd.SetGlobalVector("_MainLightPosition", new Vector3(0, -1, 0));
            }

            if (UnderWaterManager.Instance.renderType == UnderWaterManager.RenderType.RenderPipelineManager)
            {
                postCmd.Blit(tempTexture, BuiltinRenderTextureType.CameraTarget, UnderWaterManager.Instance.postMat);
            }
            else
            {
                postCmd.Blit(tempTexture, mainCamRT, UnderWaterManager.Instance.postMat);
            }

            context.ExecuteCommandBuffer(postCmd);
            context.Submit();
        }
        public void OnEnable()
        {
            cam = GetComponent<Camera>();
            postCmd = new CommandBuffer();
            postCmd.name = "UnderWaterPostProcessing";
            SpawnUnderWaterMaskMesh();
        }

        public void OnDisable()
        {
            DestroyImmediate(maskObj);
            postCmd.Release();
        }
        public void SpawnUnderWaterMaskMesh()
        {
            maskObj = new GameObject("maskObj");
            maskObj.hideFlags = HideFlags.HideAndDontSave;
            maskObj.transform.localPosition = Vector3.zero;
            maskObj.transform.localScale = Vector3.one * MaskBasicScale;
            GenerateGridMesh(WaterPlaneControl.Instance.gridSize, maskObj);
            maskMat = WaterPlaneControl.Instance.waterMats[0];
            maskObj.GetComponent<MeshRenderer>().material = maskMat;
            maskObj.GetComponent<MeshRenderer>().enabled = false;
        }
        public void GenerateGridMesh(int size, GameObject target)
        {

            MeshFilter meshFilter = target.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = target.AddComponent<MeshFilter>();
            }

            MeshRenderer meshRenderer = target.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = target.AddComponent<MeshRenderer>();
            }

            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            Vector3[] vertices = new Vector3[(size + 1) * (size + 1)];
            int[] triangles = new int[size * size * 6];
            Vector2[] uv = new Vector2[(size + 1) * (size + 1)];
            float halfSize = size / 2f;

            // Generate vertices and UVs
            for (int i = 0, y = 0; y <= size; y++)
            {
                for (int x = 0; x <= size; x++, i++)
                {
                    Vector3 length = new Vector3(x - halfSize, 0, y - halfSize);
                    length.y = -Mathf.Cos(Mathf.Clamp(length.magnitude / halfSize, 0, 1) * Mathf.PI / 2) * halfSize * maskDepth;
                    vertices[i] = length;
                    uv[i] = new Vector2((float)x / size, (float)y / size);
                }
            }

            // Generate triangles
            for (int ti = 0, vi = 0, y = 0; y < size; y++, vi++)
            {
                for (int x = 0; x < size; x++, ti += 6, vi++)
                {
                    int left_up = vi;
                    int right_up = vi + 1;
                    int left_down = vi + size + 1;
                    int right_down = vi + size + 2;
                    if ((y % 2) == 0)
                    {
                        if ((x % 2) == 0)
                        {
                            triangles[ti] = left_up;
                            triangles[ti + 1] = left_down;
                            triangles[ti + 2] = right_up;
                            triangles[ti + 3] = right_up;
                            triangles[ti + 4] = left_down;
                            triangles[ti + 5] = right_down;
                        }
                        if ((x % 2) == 1)
                        {
                            triangles[ti] = right_down;
                            triangles[ti + 1] = right_up;
                            triangles[ti + 2] = left_up;
                            triangles[ti + 3] = left_up;
                            triangles[ti + 4] = left_down;
                            triangles[ti + 5] = right_down;
                        }
                    }
                    else
                    {
                        if ((x % 2) == 0)
                        {
                            triangles[ti] = left_up;
                            triangles[ti + 1] = left_down;
                            triangles[ti + 2] = right_down;
                            triangles[ti + 3] = right_down;
                            triangles[ti + 4] = right_up;
                            triangles[ti + 5] = left_up;
                        }
                        if ((x % 2) == 1)
                        {
                            triangles[ti] = left_up;
                            triangles[ti + 1] = left_down;
                            triangles[ti + 2] = right_up;
                            triangles[ti + 3] = right_up;
                            triangles[ti + 4] = left_down;
                            triangles[ti + 5] = right_down;
                        }
                    }





                }
            }

            // Assign to mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = -normals[i]; 
            }
            mesh.normals = normals;


            triangles = mesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int temp = triangles[i];
                triangles[i] = triangles[i + 2];
                triangles[i + 2] = temp;
            }
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            Bounds bounds = mesh.bounds;
            Vector3 sizeBounds = bounds.size;
            sizeBounds.y = maskDepth * halfSize;
            sizeBounds *= 4;
            bounds.size = sizeBounds;
            mesh.bounds = bounds;
            // Assign mesh to the mesh filter
            meshFilter.mesh = mesh;
        }
    }
}