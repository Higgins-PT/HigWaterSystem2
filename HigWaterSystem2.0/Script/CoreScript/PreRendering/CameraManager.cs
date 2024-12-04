using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HigWaterSystem2
{
    public class CameraManager : HigSingleInstance<CameraManager>
    {
        public Dictionary<string, Camera> cameraDic = new Dictionary<string, Camera>();
        public static void ChangeCameraRenderer(Camera camera,int index)
        {
            UniversalAdditionalCameraData cameraData = camera.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData == null)
            {
                return;
            }
            UniversalRenderPipelineAsset urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;


            FieldInfo fieldInfo = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance);

            ScriptableRendererData[] rendererDataList = fieldInfo.GetValue(urpAsset) as ScriptableRendererData[];
            if (rendererDataList.Length > index)
            {
                cameraData.SetRenderer(index);
            }
            else
            {
                cameraData.SetRenderer(0);
            }
        }
        public Camera GetCam(Camera parentCam, string name)
        {
            string realName = name + parentCam.GetInstanceID();
            if (cameraDic.ContainsKey(realName))
            {
                return cameraDic[realName];
            }
            GameObject cam = new GameObject(name, typeof(Camera), typeof(Skybox), typeof(UniversalAdditionalCameraData));
            Camera result = cam.GetComponent<Camera>();
            result.enabled = false;
            result.gameObject.hideFlags = HideFlags.HideAndDontSave;
            cameraDic[realName] = result;

            return result;
        }
        public Camera GetCam(string name)
        {
            string realName = name;
            if (cameraDic.ContainsKey(realName))
            {
                return cameraDic[realName];
            }
            GameObject cam = new GameObject(name, typeof(Camera), typeof(Skybox), typeof(UniversalAdditionalCameraData));
            Camera result = cam.GetComponent<Camera>();
            cam.gameObject.SetActive(false);
            result.enabled = false;
            result.gameObject.hideFlags = HideFlags.HideAndDontSave;
            cameraDic[realName] = result;

            return result;
        }
        public void ResetDic()
        {
            foreach (var dic in cameraDic)
            {
                DestroyImmediate(dic.Value.gameObject);
            }
            cameraDic.Clear();


        }
        public override void ResetSystem()
        {
            ResetDic();
        }
        public override void DestroySystem()
        {
            ResetDic();
        }
    }

}