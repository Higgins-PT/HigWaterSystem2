
namespace HigWaterSystem2
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    [ExecuteInEditMode]
    public class HigSingleInstance<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;
        static bool IsPrefabPart(GameObject obj)
        {
#if UNITY_EDITOR
            return PrefabUtility.IsPartOfPrefabAsset(obj);
#else
return false;
#endif
        }

        public static T Instance
        {
            get
            {

                if (_applicationIsQuitting)
                {
                    if (_instance == null)
                    {

                        foreach (T obj in FindObjectsOfType(typeof(T)))
                        {
                            if (IsPrefabPart(obj.gameObject))
                            {
                                
                            }
                            else
                            {
                                _instance = obj;
                                break;
                            }
                        }

                    }

                    return _instance;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (T)FindObjectOfType(typeof(T));

                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {

                            Debug.LogError("[HigSingleInstance] Something went really wrong " +
                                " - there should never be more than 1 singleton! " +
                                "Reopening the scene might fix it.");
                            return _instance;
                        }
                        else if (_instance != null)
                        {
                            return _instance;
                        }

                        if (_instance == null && Application.isPlaying)
                        {
                            GameObject singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = typeof(T).ToString() + " (Singleton)";

                            Debug.Log("[HigSingleInstance] An instance of " + typeof(T) +
                                " is needed in the scene, so '" + singletonObject +
                                "' was created");
                        }

                    }

                    return _instance;
                }
            }
        }
        public virtual void HigUpdate()
        {

        }
        public virtual void ResetSystem()
        {

        }
        public virtual void DestroySystem()
        {

        }
        public static void UpdateAll()
        {
            WaterPlaneControl.Instance.HigUpdate();

            WaterSimulationManager.Instance.HigUpdate();

        }
        public static void ResetAllSystem()
        {
            WaterPlaneControl.Instance.SetSafeCode();
            RTManager.Instance.ResetSystem();
            CameraManager.Instance.ResetSystem();
            WaterPlaneControl.Instance.ResetSystem();
            OceanWaveformBase.Instance.ResetSystem();
            RTFunction.Instance.ResetSystem();
            OceanPhysics.Instance.ResetSystem();

            SSAOManager.Instance.ResetSystem();
            IFFTManager.Instance.ResetSystem();
            NoiseManager.Instance.ResetSystem();
            TextureManager.Instance.ResetSystem();
            WaterSimulationManager.Instance.ResetSystem();
            WaterRenderingAttributeManager.Instance.ResetSystem();
            ReflectionManager.Instance.ResetSystem();
            CameraMapManager.Instance.ResetSystem();
            WaveHeightProbeManager.Instance.ResetSystem(); 
            InteractiveWaterManager.Instance.ResetSystem();
            ConfigManager.Instance.ResetSystem();
            UnderWaterManager.Instance.ResetSystem();
            WaterEffectToolManager.Instance.ResetSystem();
        }
        private void Update()
        {

        }
        private void Start()
        {

        }
        void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
        private void OnDisable()
        {
            DestroySystem();
            _applicationIsQuitting = true;
        }
    }

}