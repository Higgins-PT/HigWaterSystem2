using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace HigWaterSystem2
{
    public class RTFunction : HigSingleInstance<RTFunction>
    {
        public ComputeShader RTBlit;
        public Material blitMaterialShared;
        public Material blitVertMaterialShared;
        private Material blitMaterial;
        private Material blitVertMaterial;
        public static readonly int sp_Tex1 = Shader.PropertyToID("Texture_1");
        public static readonly int sp_Tex2 = Shader.PropertyToID("Texture_2");
        public static readonly int sp_LerpT = Shader.PropertyToID("lerpT");
        public static readonly int sp_FirstSize = Shader.PropertyToID("FirstSize");
        public static readonly int sp_SecSize = Shader.PropertyToID("SecSize"); 
        public static readonly int sp_FirstWorldSize = Shader.PropertyToID("FirstWorldSize");
        public static readonly int sp_SecWorldSize = Shader.PropertyToID("SecWorldSize");
        public static readonly int sp_WorldPosOffest = Shader.PropertyToID("WorldPosOffest");
        public static readonly int sp_AddScale = Shader.PropertyToID("_AddScale");
        public static readonly int sp_CamHeight = Shader.PropertyToID("_CamHeight");
        public static readonly int sp_RenderDistance = Shader.PropertyToID("_RenderDistance");

        public static readonly int sp_Value = Shader.PropertyToID("_Value");
        public static int kernelBlitSame;
        public static int kernelBlitLerp;
        public static int kernelBlitMap;
        public static int kernelMapTex;
        public static int kernelMapTex_Normal;
        public static int kernelResetValue;
        public const int stride = 8;

        public override void ResetSystem()
        {
            kernelBlitSame = RTBlit.FindKernel("Blit_Same");
            kernelBlitLerp = RTBlit.FindKernel("Blit_Lerp");
            kernelBlitMap = RTBlit.FindKernel("Blit_Map");
            kernelMapTex = RTBlit.FindKernel("MapTex");
            kernelResetValue = RTBlit.FindKernel("ResetValue");
            blitMaterial = new Material(blitMaterialShared);
            blitVertMaterial = new Material(blitVertMaterialShared);
        }
        public static int GetThreadGroupCount(int texSize)
        {
            int threadGroups = Mathf.CeilToInt(texSize / (float)stride);
            return threadGroups;
        }
        private static bool TextureSameSize(RenderTexture RT_1, RenderTexture RT_2)
        {
            if (RT_1.width == RT_2.width)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void ResetValue(RenderTexture target, Vector4 value)
        {

            RTBlit.SetTexture(kernelResetValue, sp_Tex1, target);
            RTBlit.SetVector(sp_Value, value);
            RTBlit.Dispatch(kernelResetValue, GetThreadGroupCount(target.width), GetThreadGroupCount(target.height), 1);
        }
        public void ResetValue(CommandBuffer cmd, RenderTexture target,Vector4 value)
        {

            cmd.SetComputeTextureParam(RTBlit, kernelResetValue, sp_Tex1, target);
            cmd.SetComputeVectorParam(RTBlit, sp_Value, value);
            cmd.DispatchCompute(RTBlit, kernelResetValue, GetThreadGroupCount(target.width), GetThreadGroupCount(target.height), 1);
        }
        public void MapTexture(CommandBuffer cmd, RenderTexture soure, RenderTexture target, float sourceSize, float targerSize, Vector2 posOffest, bool normal, float addScale, float renderDistance)
        {
            if (normal == false)
            {
                RenderTexture temRT = RTManager.Instance.GetRT("mapTemTex", target.width, RenderTextureFormat.ARGBFloat);
                cmd.SetGlobalTexture(sp_Tex1, soure);
                cmd.SetGlobalTexture(sp_Tex2, target);
                cmd.SetGlobalInt(sp_FirstSize, soure.width);
                cmd.SetGlobalInt(sp_SecSize, target.width);
                cmd.SetGlobalFloat(sp_FirstWorldSize, sourceSize);
                cmd.SetGlobalFloat(sp_SecWorldSize, targerSize);
                cmd.SetGlobalFloat(sp_AddScale, addScale);
                cmd.SetGlobalVector(sp_WorldPosOffest, posOffest);
                cmd.SetGlobalFloat(sp_RenderDistance, renderDistance);
                cmd.SetGlobalFloat(sp_CamHeight, WaterPlaneControl.Instance.CamH);
                if (renderDistance == 0) 
                {
                    cmd.DisableKeyword(blitVertMaterial, new LocalKeyword(blitVertMaterial.shader, "RenderScaleEnable"));
                }
                else
                {
                    cmd.EnableKeyword(blitVertMaterial, new LocalKeyword(blitVertMaterial.shader, "RenderScaleEnable"));
                }

                cmd.Blit(soure, temRT, blitVertMaterial);
                cmd.Blit(temRT, target);
            }
            else
            {
                RenderTexture temRT = RTManager.Instance.GetRT("mapTemTex", target.width, RenderTextureFormat.ARGBFloat);
                cmd.SetGlobalTexture(sp_Tex1, soure);
                cmd.SetGlobalTexture(sp_Tex2, target);
                cmd.SetGlobalInt(sp_FirstSize, soure.width);
                cmd.SetGlobalInt(sp_SecSize, target.width);
                cmd.SetGlobalFloat(sp_FirstWorldSize, sourceSize);
                cmd.SetGlobalFloat(sp_SecWorldSize, targerSize);
                cmd.SetGlobalFloat(sp_AddScale, addScale);
                cmd.SetGlobalVector(sp_WorldPosOffest, posOffest);
                cmd.SetGlobalFloat(sp_RenderDistance, renderDistance);
                cmd.SetGlobalFloat(sp_CamHeight, WaterPlaneControl.Instance.CamH);
                if (renderDistance == 0)
                {
                    cmd.DisableKeyword(blitMaterial, new LocalKeyword(blitMaterial.shader, "RenderScaleEnable"));
                }
                else
                {
                    cmd.EnableKeyword(blitMaterial, new LocalKeyword(blitMaterial.shader, "RenderScaleEnable"));
                }
                cmd.SetRenderTarget(temRT);
                cmd.Blit(soure, temRT, blitMaterial);
                cmd.Blit(temRT, target);
            }
        }
        /// <summary>
        /// Combines two RenderTextures using a Compute Shader and writes the result into the target RenderTexture.
        /// This method supports both cases where the source and target textures have the same size or different sizes.
        /// </summary>
        /// <param name="cmd">The CommandBuffer to which the rendering commands are added.</param>
        /// <param name="soure">The source RenderTexture to be combined with the target.</param>
        /// <param name="target">The target RenderTexture where the combined result will be stored.</param>
        public void CombineTexture(CommandBuffer cmd, RenderTexture soure, RenderTexture target)
        {

            if (TextureSameSize(soure, target))//Same Size
            {

                cmd.SetComputeTextureParam(RTBlit, kernelBlitSame, sp_Tex1, target);
                cmd.SetComputeTextureParam(RTBlit, kernelBlitSame, sp_Tex2, soure);
                cmd.DispatchCompute(RTBlit, kernelBlitSame, GetThreadGroupCount(soure.width), GetThreadGroupCount(soure.height), 1);

            }
            else
            {

                cmd.SetComputeTextureParam(RTBlit, kernelBlitMap, sp_Tex1, target);
                cmd.SetComputeTextureParam(RTBlit, kernelBlitMap, sp_Tex2, soure);
                cmd.SetComputeIntParam(RTBlit, sp_FirstSize, target.width);
                cmd.SetComputeIntParam(RTBlit, sp_SecSize, soure.width);
                cmd.DispatchCompute(RTBlit, kernelBlitMap, GetThreadGroupCount(soure.width), GetThreadGroupCount(soure.height), 1);


            }
        }
        /// <summary>
        /// Performs a linear interpolation (lerp) between two RenderTextures using a Compute Shader, 
        /// and writes the result back into the first RenderTexture.
        /// </summary>
        /// <param name="cmd">The CommandBuffer to which the rendering commands are added.</param>
        /// <param name="inoutRT">The first RenderTexture, which is both an input and the output of the operation.</param>
        /// <param name="secRT">The second RenderTexture, which is the source of the interpolation.</param>
        /// <param name="lerpT">The interpolation factor, clamped between 0 and 1.</param>
        public void LerpTexture(CommandBuffer cmd, RenderTexture inoutRT, RenderTexture secRT, float lerpT)
        {


            lerpT = Mathf.Clamp01(lerpT);
            cmd.SetComputeFloatParam(RTBlit, sp_FirstSize, lerpT);
            cmd.SetComputeTextureParam(RTBlit, kernelBlitLerp, sp_Tex1, inoutRT);
            cmd.SetComputeTextureParam(RTBlit, kernelBlitLerp, sp_Tex2, secRT);
            cmd.DispatchCompute(RTBlit, kernelBlitLerp, GetThreadGroupCount(inoutRT.width), GetThreadGroupCount(inoutRT.height), 1);
        }

    }
}