using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HigWaterSystem2
{

    public class OceanPhysics : HigSingleInstance<OceanPhysics>
    {
        public bool enableOceanPhysics = true;
        public bool simpleGetHeight;
        public Dictionary<LodChannel, OceanPhysicRequest> requests = new Dictionary<LodChannel, OceanPhysicRequest>();
        public Vector3 GetOceanNormal(Vector2 worldPos, float deltaLength = 0.3f)
        {
            float heightCenter = GetOceanHeightVariation(worldPos);

            Vector2 offsetX = new Vector2(deltaLength, 0);
            Vector2 offsetZ = new Vector2(0, deltaLength);

            float heightX = GetOceanHeightVariation(worldPos + offsetX);
            float heightZ = GetOceanHeightVariation(worldPos + offsetZ);

            Vector3 tangentX = new Vector3(deltaLength, heightX - heightCenter, 0);
            Vector3 tangentZ = new Vector3(0, heightZ - heightCenter, deltaLength);

            Vector3 normal = Vector3.Cross(tangentZ, tangentX).normalized;

            return normal;
        }

        public Vector3 GetOceanNormal(Vector3 worldPos, float deltaLength = 0.3f)
        {
            return GetOceanNormal(new Vector2(worldPos.x, worldPos.z), deltaLength);
        }
        public float GetOceanHeight(Vector2 worldPos)
        {
            return GetOceanHeightVariation(worldPos) + WaterPlaneControl.Instance.waterPlaneHeight;
        }
        public float GetOceanHeightVariation(Vector2 worldPos)
        {
            float height = 0;
            foreach (var request in requests)
            {
                height += request.Value.GetWaterHeight(worldPos, simpleGetHeight);

            }
            var waveData = GetProbeHeightAndScale(worldPos);

            height = height * waveData.scale.x + waveData.height * waveData.scale.y;
            return height;

        }
        public static Vector3 ColorToVector3Ng(Color color)
        {
            return new Vector3(-color.r, -color.g, -color.b);
        }
        public Vector3 GetProbeFlow(Vector3 worldPos)
        {
            Vector2 result = GetProbeFlow(new Vector2(worldPos.x, worldPos.z));
            return new Vector3(result.x, 0, result.y);
        }
        public Vector2 GetProbeFlow(Vector2 worldPos)
        {
            Vector3 vector3 = new Vector3(worldPos.x, 0, worldPos.y);
            if (WaveHeightProbeManager.Instance.FindWaveHeightProbeContainingCoordinate(vector3, out WaveHeightProbe probe, out WaveDataCluster cluster))
            {

                return ColorToVector3Ng(WaveHeightProbeManager.Instance.GetWaveDataAtPosition(probe, cluster, vector3, WaveDataType.flowMap));
            }

            return (new Vector3(0, 0, 0));
        }
        public (float height, Vector4 scale) GetProbeHeightAndScale(Vector2 worldPos)
        {
            Vector3 vector3 = new Vector3(worldPos.x, 0, worldPos.y);
            if (WaveHeightProbeManager.Instance.FindWaveHeightProbeContainingCoordinate(vector3, out WaveHeightProbe probe, out WaveDataCluster cluster))
            {

                return (WaveHeightProbeManager.Instance.GetWaveDataAtPosition(probe, cluster, vector3, WaveDataType.heightMap).a, WaveHeightProbeManager.Instance.GetWaveDataAtPosition(probe, cluster, vector3, WaveDataType.scale));
            }

            return (0, new Vector4(1, 1, 1, 1));
        }
        public float GetOceanHeight(Vector3 worldPos)
        {
            Vector2 vector2 = new Vector2(worldPos.x, worldPos.z);
            return GetOceanHeight(vector2);
        }
        public float GetOceanHeightVariation(Vector3 worldPos)
        {
            Vector2 vector2 = new Vector2(worldPos.x, worldPos.z);
            return GetOceanHeightVariation(vector2);

        }
        public void AddPhysicReadBackRequest(LodChannel lodChannel, RenderTexture renderTexture, float mapSize)
        {
            if (Application.isPlaying && enableOceanPhysics)
            {
                if (requests.ContainsKey(lodChannel))
                {
                    requests[lodChannel].SetReadBackRequest(renderTexture);
                }
                else
                {
                    OceanPhysicRequest oceanPhysicRequest = new OceanPhysicRequest(renderTexture.width, mapSize);
                    oceanPhysicRequest.SetReadBackRequest(renderTexture);
                    requests.Add(lodChannel, oceanPhysicRequest);
                }
            }

        }
        public void ResetRequest()
        {
            foreach(var request in requests)
            {
                DestroyImmediate(request.Value.texture);
               
            }
            requests.Clear();
        }
        public override void DestroySystem()
        {
            ResetRequest();
        }
        public override void ResetSystem()
        {
            ResetRequest();

        }
        public 
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
    public class OceanPhysicRequest
    {
        public AsyncGPUReadbackRequest request;
        public float mapSize;
        public Texture2D texture;
        bool requestInitiated = true;
        public void SetReadBackRequest(RenderTexture renderTexture)
        {
            if (requestInitiated || request.done)
            {
                requestInitiated = false;
                request = AsyncGPUReadback.Request(renderTexture, 0, TextureFormat.RGBAFloat, ReadBack);
            }
        }
        Vector2 Mod(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x % b.x, a.y % b.y);
        }
        public Vector2 GetVert2V(Vector2 worldPos)
        {
            Vector3 result = GetVert(worldPos);
            return new Vector2(result.x, result.z);
        }
        public Vector3 GetVert(Vector2 worldPos)
        {

            Vector2 mapIndex = Mod(Mod(worldPos / mapSize + new Vector2(0.5f, 0.5f), new Vector2(1.0f, 1.0f)) + new Vector2(1.0f, 1.0f), new Vector2(1.0f, 1.0f));

            Color result = texture.GetPixelBilinear(mapIndex.x, mapIndex.y);
            return new Vector3(result.r, result.g, result.b);
        }

        public float GetWaterHeight(Vector2 worldPos, bool simple)
        {

            if (texture != null)
            {

                if (simple)
                {
                    return GetVert(worldPos).y;
                }
                else
                {
                    Vector2 displacement = GetVert2V(worldPos);
                    displacement += GetVert2V(worldPos - displacement);
                    displacement += GetVert2V(worldPos - displacement);
                    return GetVert(worldPos - displacement).y;
                }
            }
            return 0.0f;
        }
        public void ReadBack(AsyncGPUReadbackRequest request) => ReadBack(request, texture);
        void ReadBack(AsyncGPUReadbackRequest request, Texture2D result)
        {
            if (request.hasError)
            {
                return;
            }
            if (result != null)
            {
                result.LoadRawTextureData(request.GetData<Color>());
                result.Apply();
            }
        }
        public OceanPhysicRequest(int size, float mapSize)
        {
            texture = new Texture2D(size, size, TextureFormat.RGBAFloat, false);
            this.mapSize = mapSize;
            requestInitiated = true;
        }
    }
}