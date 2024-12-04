using HigWaterSystem2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HigWaterSystem2
{
    public static class MatAttribute
    {
        public enum OceanRenderMod
        {
            RENDERINGMOD,
            SSRMOD,
            SSAOMOD,
            WATERMASKMOD,
            DEPTHMASKMOD
        }
        public static void SwitchMod(OceanRenderMod oceanRenderMod)
        {
            DisableKeyword("RENDERINGMOD");
            DisableKeyword("SSRMOD");
            DisableKeyword("SSAOMOD");
            DisableKeyword("WATERMASKMOD");
            DisableKeyword("DEPTHMASKMOD");
            DisableKeyword("MASKMOD");
            switch (oceanRenderMod)
            {
                case OceanRenderMod.RENDERINGMOD:
                    EnableKeyword("RENDERINGMOD");
                    break;
                case OceanRenderMod.SSRMOD:
                    EnableKeyword("SSRMOD");
                    break;
                case OceanRenderMod.SSAOMOD:
                    EnableKeyword("SSAOMOD");
                    break;
                case OceanRenderMod.WATERMASKMOD:
                    EnableKeyword("WATERMASKMOD");
                    EnableKeyword("MASKMOD");
                    break;
                case OceanRenderMod.DEPTHMASKMOD:
                    EnableKeyword("DEPTHMASKMOD");
                    EnableKeyword("MASKMOD");
                    break;
            }

        }
        public static void DisableKeyword(string keyword)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.DisableKeyword(keyword);
            }
        }
        public static void EnableKeyword(string keyword)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.EnableKeyword(keyword);
            }
        }
        public static void InitPass()
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetPass(0);
            }
        }
        public static void SetPass(int pass)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetPass(pass);
            }
        }
        public static void SetFloat(int nameID, float value)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetFloat(nameID, value);
            }
        }

        public static void SetFloat(string nameID, float value)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetFloat(nameID, value);
            }
        }

        public static void SetColor(int nameID, Color value)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetColor(nameID, value);
            }
        }

        public static void SetColor(string nameID, Color value)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetColor(nameID, value);
            }
        }
        public static void SetVectorArray(string nameID, Vector4[] value)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetVectorArray(nameID, value);
            }
        }
        public static void SetColorArray(string nameID, Color[] value)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetColorArray(nameID, value);
            }
        }
        public static void SetInt(int nameID, int value)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetInt(nameID, value);
            }
        }

        public static void SetInt(string nameID, int value)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetInt(nameID, value);
            }
        }

        public static void SetMatrix(int nameID, Matrix4x4 value)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetMatrix(nameID, value);
            }
        }

        public static void SetMatrix(string nameID, Matrix4x4 value)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetMatrix(nameID, value);
            }
        }
        public static void SetVector(int nameID, Vector2 value)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetVector(nameID, value);
            }
        }

        public static void SetVector(string nameID, Vector2 value)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetVector(nameID, value);
            }
        }
        public static void SetVector(int nameID, Vector3 value)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetVector(nameID, value);
            }
        }

        public static void SetVector(string nameID, Vector3 value)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetVector(nameID, value);
            }
        }
        public static void SetVector(int nameID, Vector4 value)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetVector(nameID, value);
            }
        }

        public static void SetVector(string nameID, Vector4 value)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetVector(nameID, value);
            }
        }
        public static void SetTexture(int nameID, Texture value)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetTexture(nameID, value);
            }
        }

        public static void SetTexture(string nameID, Texture value)
        {
            foreach (Material material in WaterPlaneControl.Instance.waterMats)
            {
                material.SetTexture(nameID, value);
            }
        }
    }
}