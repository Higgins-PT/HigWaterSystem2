using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace HigWaterSystem2
{
    [ExecuteInEditMode]
    public class WaterFlow : MonoBehaviour
    {
        public Vector3 size = new Vector3(10, 10, 10);
        public int resolution = 256;
        public float flowSpeedGlobalFactor = 1f;
        public float drawRadius = 5f;
        public float flowSpeed = 1f;
        public float weight = 1f;
        public string waterFlowTextureID;
        public Texture2D waterFlowTexture;
        public GameObject flowObj;
        public Vector3 MulVec3(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }
#if UNITY_EDITOR
        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0), Vector3.one);


            Gizmos.DrawWireCube(Vector3.zero, MulVec3(size, transform.localScale));
            Gizmos.matrix = Matrix4x4.identity;
        }
        public void SetValue(Vector3 position, Color value, bool global)
        {

            Vector2 planePos = new Vector2(position.x, position.z);
            if (global)
            {
                for (int i = 0; i < resolution; i++)
                {
                    for (int j = 0; j < resolution; j++)
                    {
                        waterFlowTexture.SetPixel(i, j, value);


                    }
                }
            }
            else
            {
                for (int i = 0; i < resolution; i++)
                {
                    for (int j = 0; j < resolution; j++)
                    {
                        Vector2 uv = new Vector2(i / (float)resolution, j / (float)resolution);
                        Vector3 worldPos = flowObj.transform.TransformPoint(-new Vector3(uv.x - 0.5f, 0, uv.y - 0.5f));
                        Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.z);
                        float length = (worldPos2D - planePos).magnitude;
                        if (length < drawRadius)
                        {

                            waterFlowTexture.SetPixel(i, j, value);
                        }


                    }
                }
            }


        }
        public void Draw(Vector3 direction, Vector3 position)
        {
            GetTexture();
            Vector2 dir = new Vector2(direction.x, direction.z);

            dir = -dir.normalized * flowSpeed;
            dir.x = Mathf.Clamp((dir.x + 1f) * 0.5f, 0f, 1f);
            dir.y = Mathf.Clamp((dir.y + 1f) * 0.5f, 0f, 1f);

            SetValue(position, new Color(dir.x, dir.y, 0, 1), false);


        }
        public void Erase(Vector3 position)
        {
            GetTexture();

            SetValue(position, new Color(0.5f, 0.5f, 0, 0), false);

            
        }
        public void CleanTexture()
        {

        }
        string WaterFlowTexturePath()
        {
            return "Assets/HigWaterSystem2.0/WaterFlowData/" + "WaterFlowTexture" + waterFlowTextureID.ToString() + ".asset";
        }

        public void SaveWaterFlowTexture()
        {
            SaveTexture2DAsAsset(waterFlowTexture, WaterFlowTexturePath());
        }

        void SaveTexture2DAsAsset(Texture2D texture, string path)
        {

            string existingAssetPath = AssetDatabase.GetAssetPath(texture);

            if (string.IsNullOrEmpty(existingAssetPath))
            {
                AssetDatabase.CreateAsset(texture, path);
            }
            else
            {
                EditorUtility.SetDirty(texture);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void GetTexture()
        {
            if (waterFlowTexture == null)
            {
                if (waterFlowTextureID == "")
                {
                    waterFlowTextureID = Guid.NewGuid().ToString();
                }

                waterFlowTexture = new Texture2D(resolution, resolution, TextureFormat.RGFloat, false);
                SetValue(Vector3.one, new Color(0.5f, 0.5f, 0, 0), true);
                SaveWaterFlowTexture();

            }
            if(waterFlowTexture.width != resolution)
            {
                DestroyImmediate(waterFlowTexture, true);
                waterFlowTexture = new Texture2D(resolution, resolution, TextureFormat.RGFloat, false);
                SetValue(Vector3.one, new Color(0.5f, 0.5f, 0, 0), true);
                SaveWaterFlowTexture();

            }
            if (flowObj == null)
            {

                flowObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                flowObj.name = "FlowObj";
                DestroyImmediate(flowObj.GetComponent<BoxCollider>());


                SetRoataion();

                flowObj.transform.parent = transform;

                if (WaterEffectToolManager.Instance.flowDrawMat != null)
                {
                    flowObj.GetComponent<Renderer>().material = Instantiate(WaterEffectToolManager.Instance.flowDrawMat);
                }
                flowObj.GetComponent<Renderer>().enabled = false;
                SetMat();
                SetInput();
            }
        }
        public void SetRoataion()
        {
            if(flowObj != null)
            {
                flowObj.transform.position = transform.position;
                flowObj.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                flowObj.transform.localScale = size;
            }
        }
        public void SetInput()
        {
            if (flowObj != null)
            {
                FlowMapInput flowMapInput = River.GetComponentInCheck<FlowMapInput>(flowObj);
                flowMapInput.weight = weight;
            }
        }
        public void SetMat()
        {
            if (flowObj != null)
            {
                Material mat = flowObj.GetComponent<Renderer>().sharedMaterial;

                mat.SetTexture("_MainTex", waterFlowTexture);
                mat.SetFloat("_GlobalFactor", flowSpeedGlobalFactor);

            }
        }
        public void Clean()
        {
            DestroyImmediate(waterFlowTexture, true);
            RefreshFlow();

            SetValue(Vector3.one, new Color(0.5f, 0.5f, 0, 0), true);
            SaveWaterFlowTexture();
        }

        public void RefreshFlow()
        {
            GetTexture();
            SetRoataion();
            SetMat();
            SetInput();
        }
#endif

        void Update()
        {

        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(WaterFlow))]
    public class WaterFlowEditor : Editor
    {
        private bool isDrawingEnabled = false;
        private bool isEraserMode = false;
        private bool isDraging = false;
        private Vector3 lastMousePosition = Vector3.zero;
        private void OnSceneGUI()
        {
            SceneView.RepaintAll();
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            WaterFlow waterFlow = (WaterFlow)target;

            if (!isDrawingEnabled) return;

            Vector3 mousePosition = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);


            float waterPlaneHeight = WaterPlaneControl.Instance.waterPlaneHeight;
            Plane waterPlane = new Plane(Vector3.up, new Vector3(0, waterPlaneHeight, 0));


            if (waterPlane.Raycast(ray, out float distance))
            {

                Vector3 hitPoint = ray.GetPoint(distance);


                Event e = Event.current;

                if (e.type == EventType.MouseDown && e.button == 0)
                {

                    isDraging = true;
                }
                else
                {

                }
                if (e.type == EventType.MouseUp && e.button == 0 || (EditorWindow.mouseOverWindow != SceneView.lastActiveSceneView))
                {
                    if (isDraging == true)
                    {
                        waterFlow.SaveWaterFlowTexture();
                        isDraging = false;
                    }

                }
                if (isDraging)
                {
                    if (isEraserMode)
                    {
                        Handles.color = Color.red;
                    }
                    else
                    {
                        Handles.color = Color.yellow;
                    }
                    Vector3 direction = (hitPoint - lastMousePosition).normalized;

                    if (direction != Vector3.zero)
                    {
                        if (isEraserMode)
                        {
                            waterFlow.Erase(hitPoint);
                        }
                        else
                        {
                            waterFlow.Draw(direction, hitPoint);
                        }
                    }
                }
                else
                {
                    Handles.color = Color.cyan;
                }


                Handles.DrawWireDisc(hitPoint, new Vector3(0, 1, 0), waterFlow.drawRadius, 2);
                lastMousePosition = hitPoint;
            }
        }



        public override void OnInspectorGUI()
        {
            WaterFlow waterFlow = (WaterFlow)target;
            DrawDefaultInspector();
            EditorGUILayout.Space();
            if (GUILayout.Button("RefreshWaterFlow"))
            {
                waterFlow.RefreshFlow();
            }
            if (GUILayout.Button("Clean"))
            {
                waterFlow.Clean();
            }

            GUI.backgroundColor = isDrawingEnabled ? Color.green : Color.red;
            if (GUILayout.Button("Enable Drawing Mode"))
            {
                isDrawingEnabled = !isDrawingEnabled;
            }
            GUI.backgroundColor = Color.white;

            if (isDrawingEnabled)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = !isEraserMode ? Color.green : Color.white;
                if (GUILayout.Button("Draw"))
                {
                    isEraserMode = false;
                }

                GUI.backgroundColor = isEraserMode ? Color.green : Color.white;
                if (GUILayout.Button("Eraser"))
                {
                    isEraserMode = true;
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }

        }
    }
#endif
}