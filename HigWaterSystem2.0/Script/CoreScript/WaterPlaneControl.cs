using System.Collections;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace HigWaterSystem2
{
    public class WaterPlaneControl : HigSingleInstance<WaterPlaneControl>
    {
        public bool enableWaterPlane = true;
        public bool enableDynamicHeightDetection = true;
        public float waterPlaneHeight = 0f;
        public float boundsSizeXZAdd = 10f;
        public float boundsHeight = 100f;
        public float baseLodHeight = 250f;
        public int gridSize = 128;
        public float minSize = 10f;
        public float stretchAmount = 100000f;
        [Range(2, 9)]
        public int lodCount = 5;
        [Range(0, 2)]
        public int detailCount = 2;

        public float GridMinScale
        {
            get { return minSize / gridSize; }
        }
        public GameObject gridGO;

        public Shader waterShader;
        public GameObject gridCluster;
        public Texture2D foamTex;
        public Vector2 foamTexST = Vector2.one;
        public bool enableGlobalShaderProperties = true;
        [HideInInspector]
        public float cameraHeightOffest = 0;
        [HideInInspector]
        public List<GameObject> ringGrid = new List<GameObject>();
        [HideInInspector]
        public List<GameObject> grids = new List<GameObject>();
        [HideInInspector]
        public List<Material> waterMats = new List<Material>();
        [HideInInspector]
        public int lod_Offest;
        [HideInInspector]
        public float camDepth;
        private WaveHeightProbe waveHeightProbe;
        private GetCamHeight getCamHeight;
        private Camera lastCam;
        public Camera MainCamera { get { if (Camera.main != null) { lastCam = Camera.main; return Camera.main; } else { return lastCam; } } }
        public float CamH { get { try { return MainCamera.transform.position.y - waterPlaneHeight - cameraHeightOffest; } catch { return waterPlaneHeight; } } }

        // Start is called before the first frame update
        private Vector2[] basePositions_first = new Vector2[]
    {
            new Vector2(3, 3),
            new Vector2(1, 3),
            new Vector2(-1, 3),
            new Vector2(-3, 3),
            new Vector2(3, 1),
            new Vector2(1, 1),
            new Vector2(-3, 1),
            new Vector2(-1, 1),
            new Vector2(3, -1),
            new Vector2(1, -1),
            new Vector2(-3, -1),
            new Vector2(-1, -1),
            new Vector2(3, -3),
            new Vector2(1,-3),
            new Vector2(-1, -3),
            new Vector2(-3, -3)
    };
        private Vector2[] basePositions = new Vector2[]
            {
            new Vector2(3, 3),
            new Vector2(1, 3),
            new Vector2(-1, 3),
            new Vector2(-3, 3),
            new Vector2(3, 1),
            new Vector2(-3, 1),
            new Vector2(3, -1),
            new Vector2(-3, -1),
            new Vector2(3, -3),
            new Vector2(1,-3),
            new Vector2(-1, -3),
            new Vector2(-3, -3)
            };
        private StretchDirection[] baseDirections = new StretchDirection[]
{
    StretchDirection.Northeast,  // (3, 3)
    StretchDirection.North,      // (1, 3)
    StretchDirection.North,  // (-1, 3)
    StretchDirection.Northwest,       // (-3, 3)
    StretchDirection.East,       // (3, 1)
    StretchDirection.West,       // (-3, 1)
    StretchDirection.East,  // (3, -1)
    StretchDirection.West,  // (-3, -1)
    StretchDirection.Southeast,  // (3, -3)
    StretchDirection.South,      // (1, -3)
    StretchDirection.South,      // (-1, -3)
    StretchDirection.Southwest   // (-3, -3)
};
        public void ResetAll()
        {
            HigSingleInstance<MonoBehaviour>.ResetAllSystem();
        }
        public float GetLodSize(int index)
        {
            return minSize * Mathf.Pow(2, GetLodIndex(index)) * 4;

        }
        public float GetLodSize_Base(int index)
        {
            return Mathf.Pow(minSize, index);

        }
        public int GetLodIndex(int index)
        {
            return lod_Offest + index;
        }
        public override void ResetSystem()
        {
            cameraHeightOffest = 0;
            Shader.SetGlobalFloat("_OceanBasicHeight", waterPlaneHeight);
            waterShader = Shader.Find("HigWater/WaterPlane");
            ClearGrid();
            GenerateGridMesh(gridSize, 2, gridGO);
            gridGO.SetActive(false);
            GenerateGridObjects(lodCount);
            try
            {
                DestroyImmediate(waveHeightProbe.gameObject);
                DestroyImmediate(getCamHeight);
            }
            catch
            {

            }
        }

        private void ClearGrid()
        {
            foreach (var obj in grids)
            {
                DestroyImmediate(obj);
            }
            grids.Clear();
            foreach (var obj in ringGrid)
            {
                DestroyImmediate(obj);
            }
            ringGrid.Clear();
        }
        public override void HigUpdate()
        {

            SetScale();
            SetPosition();

            RefreshAllMatData();
            camDepth = Mathf.Max(OceanPhysics.Instance.GetOceanHeight(MainCamera.transform.position) - MainCamera.transform.position.y, 0);
        }
        public void SetSafeCode()
        {
            if (waterMats[0] != null)
            {
                waterMats[0].SetFloat("_SafeCode", 1);
            }
        }
        private void LateUpdate()
        {

        }
        void Update()
        {
            if (waterMats[0] != null)
            {
                try
                {
                    if (waterMats[0].HasProperty("_SafeCode"))
                    {
                        if (waterMats[0].GetFloat("_SafeCode") != 1)
                        {
                            HigSingleInstance<MonoBehaviour>.ResetAllSystem();
                        }
                    }
                    else
                    {
                        HigSingleInstance<MonoBehaviour>.ResetAllSystem();
                    }
                }
                catch
                {
                    HigSingleInstance<MonoBehaviour>.ResetAllSystem();
                }
            }
            if (enableWaterPlane)
            {
                gridCluster.SetActive(true);
                UpdateAll();

                if (enableDynamicHeightDetection)
                {
                    DynamicHeightDetection();
                }
                else
                {
                    waterPlaneHeight = 0;
                    if (waveHeightProbe != null)
                    {
                        DestroyImmediate(waveHeightProbe.gameObject);

                    }
                    if (getCamHeight != null)
                    {
                        DestroyImmediate(getCamHeight);
                    }

                }
            }
            else
            {
                gridCluster.SetActive(false);
            }
        }
        void DynamicHeightDetection()
        {
            if (waveHeightProbe == null)
            {
                waveHeightProbe = gameObject.GetComponentInChildren<WaveHeightProbe>();
                if (waveHeightProbe == null)
                {
                    GameObject gameObject = new GameObject("Probe");
                    gameObject.transform.parent = this.transform;
                    gameObject.hideFlags = HideFlags.DontSave;
                    waveHeightProbe = gameObject.AddComponent<WaveHeightProbe>();
                }
                waveHeightProbe.probeSize = new Vector3(4, boundsHeight * 2, 4);
                waveHeightProbe.maxTexSize = 2;
                return;
            }
            if (getCamHeight == null)
            {
                getCamHeight = gameObject.GetComponent<GetCamHeight>();
                if (getCamHeight == null)
                {
                    getCamHeight = gameObject.AddComponent<GetCamHeight>();
                }
                getCamHeight.WaveHeightProbe = waveHeightProbe;
            }

            Vector3 pos = MainCamera.transform.position;
            waveHeightProbe.gameObject.transform.position = pos;
        }
        private void Start()
        {
            ResetAllSystem();
            UpdateAll();
        }
        public Vector2 GetPlanePos()
        {
            Vector3 pos = MainCamera.transform.position;
            return new Vector2(pos.x, pos.z);
        }
        public Vector2 GetSnapToGrid(int i, Vector2 pos)
        {

            int lod = lod_Offest + i;
            float lodScale = Mathf.Pow(2, lod);
            Vector3 planePos = SnapToGrid(new Vector3(pos.x, 0, pos.y), GridMinScale * lodScale * 2);//Multiply by two to align with the higher-level plane
            return new Vector2(planePos.x, planePos.z);

        }
        void SetPosition()
        {
            Vector3 pos = MainCamera.transform.position;
            pos.y = gridCluster.transform.position.y;




            for (int i = 0; i < lodCount; i++)
            {
                int lod = lod_Offest + i;
                float lodScale = Mathf.Pow(2, lod);
                Vector3 planePos = SnapToGrid(pos, GridMinScale * lodScale * 2);//Multiply by two to align with the higher-level plane
                ringGrid[i].transform.position = planePos;
            }



        }
        public Vector3 SnapToGrid(Vector3 pos, float cellSize)
        {
            Vector2 posPlane;
            posPlane.x = Mathf.Floor(pos.x / cellSize) * cellSize;
            posPlane.y = Mathf.Floor(pos.z / cellSize) * cellSize;


            pos.x = posPlane.x;
            pos.z = posPlane.y;
            return pos;

        }
        void SetScale()
        {
            lod_Offest = GetLodLevel_Int();
            gridCluster.transform.localScale = Vector3.one * Mathf.Pow(2, lod_Offest);
        }
        float GetBaseLodScale()
        {
            float level = GetLodLevel_Int() - 1;
            float heightBase = baseLodHeight * Mathf.Pow(2, level);
            if (level == -1)
            {
                heightBase = 0f;
            }
            float height = baseLodHeight * Mathf.Pow(2, level + 1);
            return Mathf.Clamp(1 - ((Mathf.Abs(CamH) - heightBase) / (height - heightBase)), 0, 1);

        }
        int GetLodLevel_Int()
        {
            return Mathf.FloorToInt(GetLodLevel_Float());
        }
        float GetLodLevel_Float()
        {
            return Mathf.Max(Mathf.Log(Mathf.Abs(CamH) / baseLodHeight, 2f), -1f) + 1f;
        }
        public float ReturnJacobianMaxValue(int lod)
        {
            float jacobianMaxValue = 0f;
            for (int i = 0; i < IFFTManager.Instance.simTexBases.Count; i++)
            {
                if (IFFTManager.Instance.simTexBases[i].waterSimEnable)
                {
                    for (int j = 0; j < IFFTManager.Instance.simTexBases[i].iFFTSettings.lodChannels.Count; j++)
                    {
                        LodChannel lodChannel = IFFTManager.Instance.simTexBases[i].iFFTSettings.lodChannels[j];
                        if (lodChannel.ReturnChannelBool(lod))
                        {
                            jacobianMaxValue += lodChannel.normalScale;
                        }
                        if (lodChannel.ReturnChannelBool_Detail(0))
                        {
                            jacobianMaxValue += lodChannel.normalScale;
                        }
                        if (lodChannel.ReturnChannelBool_Detail(1))
                        {
                            jacobianMaxValue += lodChannel.normalScale;
                        }
                    }
                }

            }

            return jacobianMaxValue;
        }
        void RefreshAllMatData()
        {
            
            int maxLod = waterMats.Count - 1 + lod_Offest;
            float lodMaxScale = Mathf.Pow(2, maxLod) * 2;
            MatAttribute.SetFloat("gridMaxSize", lodMaxScale * minSize * 1f);
            MatAttribute.SetVector("_CamPos", MainCamera.transform.position);
            MatAttribute.SetFloat("_WaterTime", Time.time);

            if (enableGlobalShaderProperties)
            {
                Shader.SetGlobalInt("detailCount", detailCount);
            }

            for (int i = 0; i < waterMats.Count; i++)
            {
                int lod = i + lod_Offest;
                float lodScale = Mathf.Pow(2, lod) * 2;
                waterMats[i].SetVector("planePos", ringGrid[i].transform.position);
                if (i != waterMats.Count - 1)
                {

                    waterMats[i].SetVector("neiborPlaneOffestPos", ringGrid[i + 1].transform.position - ringGrid[i].transform.position);

                }
                if (i == 0)
                {
                    waterMats[i].SetFloat("gridSize", lodScale * minSize * GetBaseLodScale() * 1f);
                    waterMats[i].SetFloat("gridMinSize", lodScale * minSize * GetBaseLodScale() * 0.5f);
                }
                else
                {
                    waterMats[i].SetFloat("gridSize", lodScale * minSize * 1f);
                    waterMats[i].SetFloat("gridMinSize", lodScale * minSize * 0.5f);
                }
                waterMats[i].SetFloat("planeSize", lodScale * minSize * 2f);
                
                waterMats[i].SetFloat("planeSize_Next", Mathf.Pow(2, lod + 1) * 2 * minSize * 2f);
                
                waterMats[i].SetFloat("cellSize", lodScale * GridMinScale);
                waterMats[i].SetFloat("detailCount", detailCount);
                waterMats[i].SetFloat("_JacobianMaxValue", ReturnJacobianMaxValue(lod));
                waterMats[i].SetTexture("_FoamDetail", foamTex);
                waterMats[i].SetVector("_FoamDetail_Scale", foamTexST);
                
                if (i == waterMats.Count - 1)
                {
                    waterMats[i].SetInt("_MaxLod", 1);

                }

            }

        }
        public Mesh DeepCopyMesh(Mesh originalMesh)
        {
            Mesh newMesh = new Mesh();

            newMesh.vertices = originalMesh.vertices;
            newMesh.normals = originalMesh.normals;
            newMesh.tangents = originalMesh.tangents;
            newMesh.colors = originalMesh.colors;
            newMesh.uv = originalMesh.uv;
            newMesh.uv2 = originalMesh.uv2;
            newMesh.uv3 = originalMesh.uv3;
            newMesh.uv4 = originalMesh.uv4;
            newMesh.triangles = originalMesh.triangles;

            int subMeshCount = originalMesh.subMeshCount;
            newMesh.subMeshCount = subMeshCount;
            for (int i = 0; i < subMeshCount; i++)
            {
                newMesh.SetTriangles(originalMesh.GetTriangles(i), i);
            }

            newMesh.bindposes = originalMesh.bindposes;
            newMesh.boneWeights = originalMesh.boneWeights;
            newMesh.RecalculateBounds();


            return newMesh;
        }
        public static void AdjustMeshBounds(Mesh mesh, Vector3 scale, Vector3 worldExpansion)
        {

            Bounds localBounds = mesh.bounds;
            Vector3 inverseScale = new Vector3(1.0f / scale.x, 1.0f / scale.y, 1.0f / scale.z);
            Vector3 localExpansion = new Vector3(
                worldExpansion.x * inverseScale.x,
                worldExpansion.y * inverseScale.y,
                worldExpansion.z * inverseScale.z
            );
            Vector3 newSize = localBounds.size + localExpansion;
            localBounds.size = newSize;
            mesh.bounds = localBounds;
        }
        void GenerateGridObjects(int n)
        {
            waterMats.Clear();
            gridCluster.transform.position = new Vector3(0, waterPlaneHeight, 0);
            gridCluster.transform.localScale = Vector3.one;
            for (int i = 1; i <= n; i++)
            {
                Material material = new Material(waterShader);
                material.hideFlags = HideFlags.HideAndDontSave;
                material.SetFloat("_SafeCode", 1);
                material.enableInstancing = true;
                waterMats.Add(material);
                GameObject ring = new GameObject();
                ring.hideFlags = HideFlags.HideAndDontSave;
                ring.transform.parent = gridCluster.transform;
                ring.layer = gameObject.layer;
                ringGrid.Add(ring);
                float deltaPos = Mathf.Pow(2, i - 1);
                float scale = GridMinScale * deltaPos;
                Vector2[] ringPos = basePositions;
                if (i == 1)
                {
                    ringPos = basePositions_first;
                }
                Mesh shareMesh = null;
                for (int j = 0; j < ringPos.Length; j++)
                {
                    Vector3 position = new Vector3(ringPos[j].x * deltaPos * minSize / 2f, 0, ringPos[j].y * deltaPos * minSize / 2f);
                    GameObject obj = Instantiate(gridGO, position, Quaternion.identity);
                    obj.layer = gameObject.layer;
                    Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;
                    if (i == n)//Outermost ring
                    {
                        mesh = DeepCopyMesh(mesh);
                        obj.GetComponent<MeshFilter>().sharedMesh = mesh;
                        StretchMesh(mesh, baseDirections[j], stretchAmount, gridSize);
                    }
                    else
                    {
                        if (shareMesh == null)
                        {
                            mesh = DeepCopyMesh(mesh);
                            shareMesh = mesh;
                            AdjustMeshBounds(shareMesh, new Vector3(scale, scale, scale), new Vector3(boundsSizeXZAdd, boundsHeight, boundsSizeXZAdd));
                        }
                        obj.GetComponent<MeshFilter>().sharedMesh = shareMesh;

                    }
                    obj.transform.localScale = new Vector3(scale, scale, scale);
                    obj.SetActive(true);
                    obj.hideFlags = HideFlags.HideAndDontSave;
                    obj.GetComponent<MeshRenderer>().material = material;
                    obj.transform.parent = ring.transform;
                    obj.transform.localPosition = position;
                    grids.Add(obj);
                }

            }
        }
        public enum StretchDirection
        {
            North,
            South,
            East,
            West,
            Northeast,
            Northwest,
            Southeast,
            Southwest
        }
        public void StretchMesh(Mesh mesh, StretchDirection direction, float stretchAmount, int size)
        {
            Vector3[] vertices = mesh.vertices;


            float minX = -size / 2.0f;
            float maxX = size / 2.0f;
            float minZ = -size / 2.0f;
            float maxZ = size / 2.0f;


            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = vertices[i];


                switch (direction)
                {
                    case StretchDirection.North:
                        if (Mathf.Approximately(vertex.z, maxZ))
                            vertex.z += stretchAmount;
                        break;

                    case StretchDirection.South:
                        if (Mathf.Approximately(vertex.z, minZ))
                            vertex.z -= stretchAmount;
                        break;

                    case StretchDirection.East:
                        if (Mathf.Approximately(vertex.x, maxX))
                            vertex.x += stretchAmount;
                        break;

                    case StretchDirection.West:
                        if (Mathf.Approximately(vertex.x, minX))
                            vertex.x -= stretchAmount;
                        break;

                    case StretchDirection.Northeast:
                        if (Mathf.Approximately(vertex.z, maxZ))
                        {
                            vertex.z += stretchAmount;
                        }
                        if (Mathf.Approximately(vertex.x, maxX))
                        {
                            vertex.x += stretchAmount;
                        }
                        break;

                    case StretchDirection.Northwest:
                        if (Mathf.Approximately(vertex.z, maxZ))
                        {
                            vertex.z += stretchAmount;
                        }
                        if (Mathf.Approximately(vertex.x, minX))
                        {
                            vertex.x -= stretchAmount;
                        }
                        break;

                    case StretchDirection.Southeast:
                        if (Mathf.Approximately(vertex.z, minZ))
                        {
                            vertex.z -= stretchAmount;
                        }
                        if (Mathf.Approximately(vertex.x, maxX))
                        {
                            vertex.x += stretchAmount;
                        }
                        break;

                    case StretchDirection.Southwest:
                        if (Mathf.Approximately(vertex.z, minZ))
                        {
                            vertex.z -= stretchAmount;
                        }
                        if (Mathf.Approximately(vertex.x, minX))
                        {
                            vertex.x -= stretchAmount;
                        }
                        break;


                    default:
                        Debug.LogError("Unknown direction: " + direction);
                        break;
                }


                vertices[i] = vertex;
            }


            mesh.vertices = vertices;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }
        public void GenerateGridMesh(int size, int offest, GameObject target)
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
            size += offest;
            Vector3[] vertices = new Vector3[(size + 1) * (size + 1)];
            int[] triangles = new int[size * size * 6];
            Vector2[] uv = new Vector2[(size + 1) * (size + 1)];
            float halfSize = size / 2f;

            // Generate vertices and UVs
            for (int i = 0, y = 0; y <= size; y++)
            {
                for (int x = 0; x <= size; x++, i++)
                {
                    vertices[i] = new Vector3(x - halfSize, 0, y - halfSize);
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
            mesh.RecalculateBounds();
            // Assign mesh to the mesh filter
            meshFilter.mesh = mesh;
        }
        public void EnterTrianglesValue(int[] ints, int ti, int left_up, int right_up, int left_down, int right_down)
        {
            ints[ti] = left_up;
            ints[ti + 1] = left_down;
            ints[ti + 2] = right_up;
            ints[ti + 3] = right_up;
            ints[ti + 4] = left_down;
            ints[ti + 5] = right_down;
        }
        // Update is called once per frame

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WaterPlaneControl))]
    public class WaterPlaneControlEditor : Editor
    {
        public override void OnInspectorGUI()
        {

            DrawDefaultInspector();


            WaterPlaneControl waterPlaneControl = (WaterPlaneControl)target;


            if (GUILayout.Button("ResetSystem"))
            {

                HigSingleInstance<MonoBehaviour>.ResetAllSystem();
            }
        }
    }
#endif
}