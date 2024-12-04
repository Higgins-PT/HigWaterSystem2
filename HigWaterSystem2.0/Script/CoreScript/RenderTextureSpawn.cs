using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public static class RenderTextureSpawn
    {
        public static RenderTexture CreateRenderTexture(int size)
        {

            RenderTexture rt = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                useMipMap = false,
                autoGenerateMips = false,
                enableRandomWrite = true
            };

            rt.Create();

            return rt;
        }
    }

}