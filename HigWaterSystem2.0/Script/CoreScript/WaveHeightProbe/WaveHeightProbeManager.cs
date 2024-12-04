using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HigWaterSystem2
{
    public class WaveHeightProbeManager : HigSingleInstance<WaveHeightProbeManager>
    {
        private bool globalEnable = false;
        public Camera probeCam;
        public GameObject camGameObject;
        public List<WaveHeightProbe> waveHeightProbes = new List<WaveHeightProbe>();
        public Dictionary<WaveHeightProbe, WaveDataCluster> waveDataClusters = new Dictionary<WaveHeightProbe, WaveDataCluster>();
        public Color GetWaveDataAtPosition(WaveHeightProbe waveHeightProbe, WaveDataCluster waveDataCluster, Vector3 worldPosition, WaveDataType waveDataType)
        {

            WaveHeightProbeRenderRequest request = waveHeightProbe.waveHeightProbeRenderRequest;
            Vector2 uv = GetUVFromWorldPosition(request, worldPosition);
            Texture2D selectedTexture = null;

            switch (waveDataType)
            {
                case WaveDataType.heightMap:
                    selectedTexture = waveDataCluster.heightTex;
                    break;
                case WaveDataType.flowMap:
                    selectedTexture = waveDataCluster.flowMapTex;
                    break;
                case WaveDataType.scale:
                    selectedTexture = waveDataCluster.waveScaleTex;
                    break;
                default:
                    return Color.clear;
            }
            if (selectedTexture == null)
            {
                return CameraMapManager.GetDefaultWaveDataColor(waveDataType);
            }

            int x = Mathf.FloorToInt(uv.x * selectedTexture.width);
            int y = Mathf.FloorToInt(uv.y * selectedTexture.height);

            Color pixelColor = CheckOverflowColor(selectedTexture.GetPixel(x, y));

            return pixelColor;
        }
        public Color CheckOverflowColor(Color color)
        {
            float overflowThreshold = 1e6f;

            if (Mathf.Abs(color.r) > overflowThreshold || Mathf.Abs(color.g) > overflowThreshold || Mathf.Abs(color.b) > overflowThreshold || Mathf.Abs(color.a) > overflowThreshold)
            {
                return new Color(0, 0, 0, 0);
            }

            return color;
        }

        public Vector2 GetUVFromWorldPosition(WaveHeightProbeRenderRequest request, Vector3 worldPosition)
        {
            // Compute half size for bounds
            Vector3 halfSize = request.size * 0.5f;

            // Transform world position into local space using the probe's rotation and position
            Quaternion rotation = Quaternion.Euler(0, request.rot, 0);
            Matrix4x4 probeMatrix = Matrix4x4.TRS(request.centerPos, rotation, Vector3.one);
            Matrix4x4 inverseProbeMatrix = probeMatrix.inverse;

            // Convert world position to local space
            Vector3 localPosition = inverseProbeMatrix.MultiplyPoint3x4(worldPosition);
            // Normalize the local position to get UV coordinates (range [0, 1])
            float u = Mathf.InverseLerp(-halfSize.x, halfSize.x, localPosition.x);
            float v = Mathf.InverseLerp(-halfSize.z, halfSize.z, localPosition.z);

            return new Vector2(u, v);
        }

        public bool FindWaveHeightProbeContainingCoordinate(Vector3 position, out WaveHeightProbe probe, out WaveDataCluster cluster)
        {
            foreach (var kvp in waveDataClusters)
            {
                var waveHeightProbe = kvp.Key;
                var waveHeightProbeRequest = waveHeightProbe.waveHeightProbeRenderRequest;

                // Compute half size for bounds
                Vector3 halfSize = waveHeightProbeRequest.size * 0.5f;

                // Transform position into the local space of the probe considering rotation
                Quaternion rotation = Quaternion.Euler(0, waveHeightProbeRequest.rot, 0);
                Matrix4x4 probeMatrix = Matrix4x4.TRS(waveHeightProbeRequest.centerPos, rotation, Vector3.one);
                Matrix4x4 inverseProbeMatrix = probeMatrix.inverse;

                // Transform the world position into the probe's local space
                Vector3 localPos = inverseProbeMatrix.MultiplyPoint3x4(position);

                // Check if the local position is within the unrotated bounds (which are now axis-aligned)
                if (Mathf.Abs(localPos.x) <= halfSize.x &&
                    Mathf.Abs(localPos.z) <= halfSize.z)
                {
                    // If the coordinate is within the probe's bounds, return the probe and cluster
                    probe = waveHeightProbe;
                    cluster = kvp.Value;
                    return true;
                }
            }

            // If no probe contains the position, return null values
            probe = null;
            cluster = null;
            return false;
        }
        public void AddProbe(WaveHeightProbe waveHeightProbe)
        {
            waveHeightProbes.Add(waveHeightProbe);
        }
        public void RemoveProbe(WaveHeightProbe waveHeightProbe)
        {
            waveHeightProbes?.Remove(waveHeightProbe);
        }
        public void UpdateAllProbe(CommandBuffer cmd)
        {
            foreach(WaveHeightProbe waveHeightProbe in waveHeightProbes)
            {

                if (waveHeightProbe.GetRefreshBool() || globalEnable)
                {
                    RenderProbe(cmd, waveHeightProbe, waveHeightProbe.waveHeightProbeRenderRequest);
                }
            }

        }
        public void RenderProbe(CommandBuffer commandBuffer, WaveHeightProbe waveHeightProbe, WaveHeightProbeRenderRequest waveHeightProbeRenderRequest)
        {
            SetCam(probeCam, waveHeightProbeRenderRequest);
            SetRenderMatrix(probeCam.projectionMatrix, probeCam.worldToCameraMatrix, commandBuffer);
            RenderPhysicToCam(commandBuffer, waveHeightProbe, waveHeightProbeRenderRequest.texSize);
        }
        public void RenderPhysicToCam(CommandBuffer commandBuffer, WaveHeightProbe waveHeightProbe, int2 size)
        {
            WaveDataCluster waveDataCluster = GetWaveDataCluster(waveHeightProbe);
            RenderWaveToTex(commandBuffer, WaveDataType.heightMap, waveHeightProbe, waveDataCluster, size);
            RenderWaveToTex(commandBuffer, WaveDataType.scale, waveHeightProbe, waveDataCluster, size);
            RenderWaveToTex(commandBuffer, WaveDataType.flowMap, waveHeightProbe, waveDataCluster, size);
            waveDataCluster.RequestAsyncReadback();

        }
        public void RenderWaveToTex(CommandBuffer cmd, WaveDataType waveDataType, WaveHeightProbe waveHeightProbe, WaveDataCluster waveDataCluster, int2 size)
        {
            RenderTexture waveDataRT = RTManager.Instance.GetRT(waveHeightProbe.GetInstanceID().ToString() + waveDataType.ToString(), size.x, size.y, RenderTextureFormat.ARGBFloat);
            waveDataCluster.SetRT(waveDataRT, waveDataType);
            cmd.SetRenderTarget(waveDataRT);
            CameraMapManager.SetCmdBackGround(cmd, waveDataType);
            waveDataCluster.SetWaveDataEnable(CameraMapManager.Instance.SetRenderer(cmd, waveDataType, probeCam), waveDataType);
        }
        public WaveDataCluster GetWaveDataCluster(WaveHeightProbe waveHeightProbe)
        {
            if (waveDataClusters.TryGetValue(waveHeightProbe, out WaveDataCluster result))
            {
                return result;
            }
            else
            {
                WaveDataCluster newWaveDataCluster = new WaveDataCluster();
                waveDataClusters.Add(waveHeightProbe, newWaveDataCluster);
                return newWaveDataCluster;
            }
        }
        public void ResetAll()
        {
            foreach(var cluster in waveDataClusters.Values)
            {
                cluster.ReleaseTex();
            }
            waveDataClusters.Clear();
            StartCoroutine(GlobalEnable());

        }
        private IEnumerator GlobalEnable()
        {
            yield return new WaitForSeconds(0.5f);
            globalEnable = true;
            yield return new WaitForSeconds(0.5f);
            globalEnable = false;
        }
        public void SetRenderMatrix(Matrix4x4 projectionMatrix, Matrix4x4 viewMatrix, CommandBuffer cmd)
        {
            cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

        }
        public void SetCam(Camera cam, WaveHeightProbeRenderRequest waveHeightProbeRenderRequest)
        {

            cam.orthographic = true;


            cam.orthographicSize = waveHeightProbeRenderRequest.size.z * 0.5f;


            cam.transform.position = new Vector3(
                waveHeightProbeRenderRequest.centerPos.x,
                waveHeightProbeRenderRequest.centerPos.y + waveHeightProbeRenderRequest.size.y / 2,
                waveHeightProbeRenderRequest.centerPos.z
            );


            cam.transform.rotation = Quaternion.Euler(90, 0, -waveHeightProbeRenderRequest.rot);

            cam.aspect = (float)waveHeightProbeRenderRequest.size.x / (float)waveHeightProbeRenderRequest.size.z;


            cam.nearClipPlane = 0f;
            cam.farClipPlane = waveHeightProbeRenderRequest.size.y * 2f;
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
        public override void ResetSystem()
        {
            probeCam = CreateTopDownOrthoCamera(1);
            ResetAll();

        }
        void Start()
        {

        }

        void Update()
        {

        }
    }
    public class WaveDataCluster
    {
        public Texture2D heightTex;
        public Texture2D flowMapTex;
        public Texture2D waveScaleTex;

        public RenderTexture heightRT;
        public RenderTexture flowMapRT;
        public RenderTexture waveScaleRT;

        public bool heightEnable;
        public bool flowEnable;
        public bool waveEnable;

        // Track ongoing AsyncGPUReadbackRequests
        private AsyncGPUReadbackRequest? heightRequest = null;
        private AsyncGPUReadbackRequest? flowRequest = null;
        private AsyncGPUReadbackRequest? waveRequest = null;
        public bool CheckReadbackStatus()
        {
            bool allCompleted = true;
            if (heightRequest.HasValue && heightRequest.Value.done)
            {
                heightRequest = null;
            }
            else if (heightRequest.HasValue)
            {
                allCompleted = false;
            }

            if (flowRequest.HasValue && flowRequest.Value.done)
            {
                flowRequest = null;
            }
            else if (flowRequest.HasValue)
            {
                allCompleted = false;
            }


            if (waveRequest.HasValue && waveRequest.Value.done)
            {
                waveRequest = null;
            }
            else if (waveRequest.HasValue)
            {
                allCompleted = false;
            }

            return allCompleted;
        }

        public void ReleaseTex()
        {
            GameObject.DestroyImmediate(heightTex);
            GameObject.DestroyImmediate(flowMapTex);
            GameObject.DestroyImmediate(waveScaleTex);
        }
        public void SetRT(RenderTexture renderTexture,WaveDataType waveDataType)
        {
            switch (waveDataType)
            {
                case WaveDataType.heightMap:
                    heightRT = renderTexture;
                    break;
                case WaveDataType.flowMap:
                    flowMapRT = renderTexture;
                    break;
                case WaveDataType.scale:
                    waveScaleRT = renderTexture;
                    break;
            }
        }
        public void SetWaveDataEnable(bool enable, WaveDataType waveDataType)
        {
            switch (waveDataType)
            {
                case WaveDataType.heightMap:
                    heightEnable = enable;
                    break;
                case WaveDataType.flowMap:
                    flowEnable = enable;
                    break;
                case WaveDataType.scale:
                    waveEnable = enable;
                    break;
            }
        }

        // Main function responsible for requesting GPU data and updating Texture2D based on the enable flags
        public void RequestAsyncReadback()
        {

            if (heightEnable && heightRT != null && heightTex == null)
            {
                heightTex = new Texture2D(heightRT.width, heightRT.height, TextureFormat.RGBAFloat, false);
            }


            if (flowEnable && flowMapRT != null && flowMapTex == null)
            {
                flowMapTex = new Texture2D(flowMapRT.width, flowMapRT.height, TextureFormat.RGBAFloat, false);
            }


            if (waveEnable && waveScaleRT != null && waveScaleTex == null)
            {
                waveScaleTex = new Texture2D(waveScaleRT.width, waveScaleRT.height, TextureFormat.RGBAFloat, false);
            }


            if (heightEnable && heightRT != null && !IsRequestPending(heightRequest))
            {
                heightRequest = RequestTextureReadback(heightRT, heightTex, "Height texture readback failed.");
            }


            if (flowEnable && flowMapRT != null && !IsRequestPending(flowRequest))
            {
                flowRequest = RequestTextureReadback(flowMapRT, flowMapTex, "Flow map texture readback failed.");
            }

            if (waveEnable && waveScaleRT != null && !IsRequestPending(waveRequest))
            {
                waveRequest = RequestTextureReadback(waveScaleRT, waveScaleTex, "Wave scale texture readback failed.");
            }
        }
        // Check if the previous request is still pending
        private bool IsRequestPending(AsyncGPUReadbackRequest? request)
        {
            return request != null && !request.Value.done && !request.Value.hasError;
        }

        // Initiates an async GPU readback request and returns the request object
        private AsyncGPUReadbackRequest RequestTextureReadback(RenderTexture renderTexture, Texture2D texture2D, string errorMessage)
        {
            return AsyncGPUReadback.Request(renderTexture, 0, TextureFormat.RGBAFloat, (AsyncGPUReadbackRequest request) =>
            {
                HandleReadbackRequest(request, texture2D, errorMessage);
            });
        }

        // Handles the async readback request callback, updating Texture2D
        private void HandleReadbackRequest(AsyncGPUReadbackRequest request, Texture2D texture2D, string errorMessage)
        {
            if (request.hasError)
            {
                Debug.LogError(errorMessage);
                return;
            }
            try
            {
                texture2D.SetPixelData(request.GetData<Color>(), 0);
                texture2D.Apply();
            }
            catch
            {

            }
        }
    }
    public struct WaveHeightProbeRenderRequest
    {
        public Vector3 size;//camRenderWorldSize
        public int2 texSize;//camTargetTexSize
        public Vector3 centerPos;//camCenter
        public float rot;//camRotation(Z)
        public WaveHeightProbeRenderRequest(Vector3 size, float maxTextureSize, Vector3 centerPos, float rot)
        {
            this.size = size;
            this.centerPos = centerPos;
            this.rot = rot;

            if (size.x >= size.z)
            {
                this.texSize = new int2((int)maxTextureSize, (int)(maxTextureSize * (size.z / size.x)));
            }
            else
            {
                this.texSize = new int2((int)(maxTextureSize * (size.x / size.z)), (int)maxTextureSize);
            }
        }
    }
}