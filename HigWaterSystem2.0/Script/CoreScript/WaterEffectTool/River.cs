 using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace HigWaterSystem2
{
    [ExecuteInEditMode]
    public class River : MonoBehaviour
    {
        public List<RiverNode> riverNodes = new List<RiverNode>();
        [HideInInspector]
        public List<RiverPoint> riverPoints = new List<RiverPoint>();
        private SplineCurve splineCurve0;
        private SplineCurve splineCurve1;
        private RiverCurve riverCurve;
        public int resolution = 5;
        public float riverMinSpeed = 0.5f;
        public float riverMaxSpeed = 1f;
        public float weight = 100f;
        public Vector4 oceanData = new Vector4(1, 1, 1, 1);
        [HideInInspector]
        public GameObject riverFlowMap;
        [HideInInspector]
        public GameObject riverHeight;
        [HideInInspector]
        public GameObject riverOceanData;
        public float pointDefaultHeight = 5f;
        public enum SpawnType { DefaultHeight, DragHeight };
        public SpawnType spawnType = SpawnType.DefaultHeight;
        public void OnDrawGizmos()
        {
            try
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < riverCurve.riverPoints.Count; i++)
                {


                    Gizmos.DrawLine(riverCurve.riverWidthPoint1[i], riverCurve.riverWidthPoint2[i]);
                    if (i == 0) continue;
                    Gizmos.DrawLine(riverCurve.riverPoints[i].worldPos, riverCurve.riverPoints[i - 1].worldPos);
                }
                for (int i = 0; i < splineCurve0.splinePoints.Count - 1; i++)
                {
                    if (i == 0) continue;

                    Gizmos.DrawLine(splineCurve0.splinePoints[i], splineCurve0.splinePoints[i + 1]);
                    Gizmos.DrawLine(splineCurve1.splinePoints[i], splineCurve1.splinePoints[i + 1]);

                }
            }
            catch
            {

            }
        }
        public static T GetComponentInCheck<T>(GameObject gameObject) where T : Component
        {
            T t = gameObject.GetComponent<T>();
            if (t == null)
            {
                return gameObject.AddComponent<T>();
            }
            else
            {
                return t;
            }
        }
        public void RefreshRiver()
        {
            SetAction();
            SpawnRiverMap();
        }

        public void CreateNewPoint(Vector3 pos, float width)
        {
            GameObject point = new GameObject("Node");
            point.transform.parent = transform;
            point.transform.position = pos;
            RiverNode node = point.AddComponent<RiverNode>();
            node.width = width;
            riverNodes.Add(node);
        }
        public void DeletePoint(RiverNode node)
        {
            riverNodes?.Remove(node);
            DestroyImmediate(node.gameObject);
        }
        public void RiverNodesToPoints()
        {
            riverPoints.Clear();
            foreach (var node in riverNodes)
            {
                node.riverPoint.worldPos = node.transform.position;
                node.riverPoint.width = node.width;
                riverPoints.Add(node.riverPoint);
            }
        }
        public void SetAction()
        {
            foreach (var node in riverNodes)
            {
                node.CleanAction();
                node.PositionChanged += SpawnRiverMap;
            }
        }
        public void SpawnRiverMap()
        {
            if (riverNodes.Count < 2)
            {
                return;
            }
            RiverNodesToPoints();
            RiverCurve riverCurve = new RiverCurve(riverPoints);
            SplineCurve splineCurve0 = RiverSpawn.GenerateSpline(riverCurve.riverWidthPoint1, resolution);
            SplineCurve splineCurve1 = RiverSpawn.GenerateSpline(riverCurve.riverWidthPoint2, resolution);
            this.riverCurve = riverCurve;
            this.splineCurve0 = splineCurve0;
            this.splineCurve1 = splineCurve1;
            Mesh riverMesh = RiverSpawn.GenerateMesh(splineCurve0, splineCurve1, transform.position);

            if (riverFlowMap != null)
            {
                DestroyImmediate(riverFlowMap);
            }
            riverFlowMap = new GameObject("RiverFlowMap");
            riverFlowMap.transform.parent = transform;
            riverFlowMap.transform.localPosition = Vector3.zero;
            MeshFilter flowMeshFilter = GetComponentInCheck<MeshFilter>(riverFlowMap);
            MeshRenderer flowMeshRenderer = GetComponentInCheck<MeshRenderer>(riverFlowMap);
            Material riverFlowMat = new Material(WaterEffectToolManager.Instance.riverFlowShader);
            riverFlowMat.SetFloat("_MinSpeed", riverMinSpeed);
            riverFlowMat.SetFloat("_MaxSpeed", riverMaxSpeed);
            flowMeshFilter.sharedMesh = riverMesh;
            flowMeshRenderer.sharedMaterial = riverFlowMat;
            flowMeshRenderer.enabled = false;


            if (riverHeight != null)
            {
                DestroyImmediate(riverHeight);
            }
            riverHeight = new GameObject("RiverHeight");
            riverHeight.transform.parent = transform;
            riverHeight.transform.localPosition = new Vector3(0, 0, 0);
            MeshFilter heightMeshFilter = GetComponentInCheck<MeshFilter>(riverHeight);
            MeshRenderer heightMeshRenderer = GetComponentInCheck<MeshRenderer>(riverHeight);
            Material riverHeightMat = new Material(WaterEffectToolManager.Instance.riverHeightShader);
            heightMeshFilter.sharedMesh = riverMesh;
            heightMeshRenderer.sharedMaterial = riverHeightMat;
            heightMeshRenderer.enabled = false;


            if (riverOceanData != null)
            {
                DestroyImmediate(riverOceanData);

            }
            riverOceanData = new GameObject("RiverOceanData");
            riverOceanData.transform.parent = transform;
            riverOceanData.transform.localPosition = Vector3.zero;
            MeshFilter oceanMeshFilter = GetComponentInCheck<MeshFilter>(riverOceanData);
            MeshRenderer oceanMeshRenderer = GetComponentInCheck<MeshRenderer>(riverOceanData);
            Material riverWaterScaleMat = new Material(WaterEffectToolManager.Instance.riverWaterScaleShader);
            riverWaterScaleMat.SetVector("_Data", oceanData);
            oceanMeshFilter.sharedMesh = riverMesh;
            oceanMeshRenderer.sharedMaterial = riverWaterScaleMat;
            oceanMeshRenderer.enabled = false;


            FlowMapInput flowMapInput = GetComponentInCheck<FlowMapInput>(riverFlowMap);

            HeightBoxInput heightBoxInput = GetComponentInCheck<HeightBoxInput>(riverHeight);

            WaveScaleInput waveDataInput = GetComponentInCheck<WaveScaleInput>(riverOceanData);
            flowMapInput.weight = weight;
            heightBoxInput.weight = weight;
            waveDataInput.weight = weight;




        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(River))]
    public class RiverEditor : Editor
    {
        private River river;

        // Define an enum for the edit modes
        private enum EditMode { None, Add, Delete }
        private EditMode currentMode = EditMode.None;

        private Vector3 initialClickPoint;
        private bool isDragging = false;
        private float currentWidth = 10f; // Default width
        private float currentHeight = 1f;
        private Vector3 currentPointPosition;

        private void OnEnable()
        {
            river = (River)target;
        }

        private void OnSceneGUI()
        {
            Event e = Event.current;

            // Handle different modes
            switch (currentMode)
            {
                case EditMode.Add:
                    HandleAddMode(e);
                    SceneView.RepaintAll();
                    break;
                case EditMode.Delete:
                    HandleDeleteMode(e);
                    break;
                case EditMode.None:
                    HandleEditMode(e);
                    break;
            }

            // Always draw the nodes
            DrawNodes();
        }

        private void HandleAddMode(Event e)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Vector3 hitPoint = Vector3.zero;
            Vector3 hitNormal = Vector3.up;
            // Raycast to get the hit point in the scene
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                hitPoint = hit.point;
                hitNormal = hit.normal;
            }
            else
            {
                // If no collider is hit, project onto XZ plane at y=0
                float planeY = 0f;
                float distance = (planeY - ray.origin.y) / ray.direction.y;
                hitPoint = ray.origin + ray.direction * distance;
                hitNormal = Vector3.up;
            }

            Handles.color = Color.green;
            Handles.DrawWireDisc(hitPoint, hitNormal, 0.5f);
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                // Start dragging
                initialClickPoint = hitPoint;
                isDragging = true;
                currentPointPosition = hitPoint;
                currentWidth = 10f; // Default width
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && isDragging && e.button == 0)
            {
                // Update current width based on drag distance
                currentPointPosition = hitPoint;
                float distance = Vector3.Distance(initialClickPoint, currentPointPosition);
                currentWidth = distance * 2f;
                currentHeight = currentPointPosition.y;
                SceneView.RepaintAll();
                e.Use();
            }
            else if (e.type == EventType.MouseUp && isDragging && e.button == 0)
            {
                // Finish adding point
                Undo.RecordObject(river, "Add River Node");
                Vector3 pointPos = Vector3.zero;
                if (river.spawnType == River.SpawnType.DefaultHeight)
                {
                    pointPos = initialClickPoint + new Vector3(0, river.pointDefaultHeight, 0);
                }
                else if (river.spawnType == River.SpawnType.DragHeight)
                {
                    pointPos = initialClickPoint;
                    pointPos.y = currentHeight;
                }
                river.CreateNewPoint(pointPos, currentWidth);
                EditorUtility.SetDirty(river);
                isDragging = false;
                river.RefreshRiver(); // Refresh river after adding a point
                e.Use();
            }

            if (isDragging)
            {
                // Draw a line from initial point to current mouse position
                Handles.color = Color.yellow;
                Handles.DrawLine(initialClickPoint, currentPointPosition);
                // Display current width
                Handles.Label(initialClickPoint + (currentPointPosition - initialClickPoint) / 2 + Vector3.up * 0.5f, $"Width: {currentWidth:F2}");
            }
        }

        private void HandleDeleteMode(Event e)
        {
            // Do not show position handles when in delete mode
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }

        private void HandleEditMode(Event e)
        {
            // Default edit mode
        }

        private void DrawNodes()
        {
            for (int i = 0; i < river.riverNodes.Count; i++)
            {
                RiverNode node = river.riverNodes[i];

                // Only show position handles when not in delete mode
                if (currentMode != EditMode.Delete)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPos = Handles.PositionHandle(node.transform.position, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(node.transform, "Move River Node");
                        node.transform.position = newPos;
                        EditorUtility.SetDirty(node);
                        river.RefreshRiver(); // Refresh river after moving a node
                    }
                }

                // Display node width label
                Handles.Label(node.transform.position + Vector3.up * 0.5f, $"Width: {node.width}");

                // In delete mode, draw larger spheres for deletion
                if (currentMode == EditMode.Delete)
                {
                    Handles.color = Color.red;
                    float handleSize = HandleUtility.GetHandleSize(node.transform.position) * 0.5f;

                    if (Handles.Button(node.transform.position, Quaternion.identity, handleSize, handleSize, Handles.SphereHandleCap))
                    {
                        Undo.RecordObject(river, "Delete River Node");
                        river.DeletePoint(node);
                        EditorUtility.SetDirty(river);
                        river.RefreshRiver(); // Refresh river after deleting a node
                        // Exit the loop since the collection has been modified
                        break;
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.BeginHorizontal();

            // Add Node button
            bool addModeSelected = (currentMode == EditMode.Add);
            if (GUILayout.Toggle(addModeSelected, "Add Node", "Button"))
            {
                if (currentMode != EditMode.Add)
                {
                    currentMode = EditMode.Add;
                    SceneView.RepaintAll();
                }
            }
            else if (addModeSelected)
            {
                currentMode = EditMode.None;
                SceneView.RepaintAll();
            }

            // Delete Node button
            bool deleteModeSelected = (currentMode == EditMode.Delete);
            if (GUILayout.Toggle(deleteModeSelected, "Delete Node", "Button"))
            {
                if (currentMode != EditMode.Delete)
                {
                    currentMode = EditMode.Delete;
                    SceneView.RepaintAll();
                }
            }
            else if (deleteModeSelected)
            {
                currentMode = EditMode.None;
                SceneView.RepaintAll();
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Generate River"))
            {
                river.RefreshRiver();
            }
        }
    }
#endif
}

