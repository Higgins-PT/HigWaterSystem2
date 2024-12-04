using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
namespace HigWaterSystem2
{
    [System.Serializable]
    public class RTManager : HigSingleInstance<RTManager>
    {
        public Dictionary<string, RenderTexture> CaculateRT = new Dictionary<string, RenderTexture>();
        public Dictionary<string, Cubemap> CaculateCB = new Dictionary<string, Cubemap>();
        public RenderTexture GetRT(string name, int width, int height, RenderTextureFormat RTF, TextureWrapMode TWM = TextureWrapMode.Repeat, FilterMode FLM = FilterMode.Trilinear)
        {
            string realName = name + width.ToString() + "W" + height.ToString() + RTF.ToString() + TWM.ToString() + FLM.ToString();
            if (CaculateRT.ContainsKey(realName))
            {
                return CaculateRT[realName];
            }
            RenderTexture newRT = new RenderTexture(width, height, 0, RTF)
            {
                wrapMode = TWM,
                filterMode = FLM,
                useMipMap = false,
                autoGenerateMips = false,
                enableRandomWrite = true
            };
            newRT.name = realName;
            newRT.Create();
            CaculateRT[realName] = newRT;

            return newRT;
        }
        public void ReleaseRT(string name, int width, int height, RenderTextureFormat RTF)
        {
            string realName = name + width.ToString() + "W" + height.ToString() + RTF.ToString();
            if (CaculateRT.ContainsKey(realName))
            {
                CaculateRT[realName].Release();
                CaculateRT?.Remove(realName);
            }

        }
        public void ReleaseRT(RenderTexture renderTexture)
        {
            string key = "";
            foreach (var pair in CaculateRT)
            {
                if (pair.Value == renderTexture)
                {
                    pair.Value.Release();
                    key = pair.Key;
                }
            }

            CaculateRT?.Remove(key);
        }
        public void ReleaseCB(Cubemap cubemap)
        {
            string key = "";
            foreach (var pair in CaculateCB)
            {
                if (pair.Value == cubemap)
                {
                    key = pair.Key;
                }
            }

            CaculateCB?.Remove(key);
        }
        public void ReleaseCB(string name, int size, TextureFormat RTF)
        {
            string realName = name + size.ToString() + RTF.ToString();
            if (CaculateCB.ContainsKey(realName))
            {

                CaculateCB.Remove(realName,out Cubemap cubemap);
                DestroyImmediate(cubemap);
            }
        }
        public string GetCB_Name(string name, int size, TextureFormat RTF)
        {
            string realName = name + size.ToString() + RTF.ToString();
            return realName;
        }
        public Cubemap GetCB(string name, int size, TextureFormat RTF)
        {
            string realName = name + size.ToString() + RTF.ToString();
            if (CaculateCB.ContainsKey(realName))
            {
                return CaculateCB[realName];
            }
            Cubemap newRT = new Cubemap(size, RTF, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Trilinear,
            };
            CaculateCB[realName] = newRT;

            return newRT;
        }
        public RenderTexture GetRT(string name, int size, RenderTextureFormat RTF, TextureWrapMode TWM)
        {
            string realName = name + size.ToString() + RTF.ToString() + TWM.ToString();
            if (CaculateRT.ContainsKey(realName))
            {
                return CaculateRT[realName];
            }
            RenderTexture newRT = new RenderTexture(size, size, 0, RTF, RenderTextureReadWrite.Linear)
            {

                wrapMode = TWM,
                filterMode = FilterMode.Trilinear,
                useMipMap = false,
                autoGenerateMips = false,
                enableRandomWrite = true
            };
            newRT.name = realName;
            newRT.Create();
            CaculateRT[realName] = newRT;

            return newRT;
        }
        public RenderTexture GetRT(string name, int size, RenderTextureFormat RTF, FilterMode FLM = FilterMode.Trilinear)
        {
            string realName = name + size.ToString() + RTF.ToString() + FLM.ToString();
            if (CaculateRT.ContainsKey(realName))
            {
                return CaculateRT[realName];
            }
            RenderTexture newRT = new RenderTexture(size, size, 0, RTF, RenderTextureReadWrite.Linear)
            {

                wrapMode = TextureWrapMode.Repeat,
                filterMode = FLM,
                useMipMap = false,
                autoGenerateMips = false,
                enableRandomWrite = true
            };
            newRT.name = realName;
            newRT.Create();
            CaculateRT[realName] = newRT;

            return newRT;
        }

        public RenderTexture GetRT(string name, int size, RenderTextureFormat RTF, int mipCount, FilterMode FLM = FilterMode.Point)
        {
            string realName = name + size.ToString() + RTF.ToString() + mipCount.ToString() + FLM.ToString();
            if (CaculateRT.ContainsKey(realName))
            {
                return CaculateRT[realName];
            }
            RenderTexture newRT = new RenderTexture(size, size, 0, RTF, mipCount)
            {
                
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FLM,
                useMipMap = true,
                autoGenerateMips = false,
                enableRandomWrite = true
            };
            newRT.name = realName;
            newRT.Create();
            CaculateRT[realName] = newRT;

            return newRT;
        }
        public void ResetDic()
        {
            foreach (var dic in CaculateRT)
            {
                dic.Value.Release();
            }
            CaculateRT.Clear();
            foreach (var dic in CaculateCB)
            {
                DestroyImmediate(dic.Value);
            }
            CaculateCB.Clear();
            
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