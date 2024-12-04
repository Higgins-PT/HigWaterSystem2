
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


namespace HigWaterSystem2
{
    public class InteractiveWaterManager : HigSingleInstance<InteractiveWaterManager>
    {
        public ComputeShader interactiveCS;
        public ComputeShader interactiveDrawCS;
        public bool enable = true;
        public float WaveLimitHeight = 20f;
        public float WaveViscosity = 1.0f;
        public float WaveSpeed = 1.0f;
        public float WaveAttenuate = 0.99f;
        public float WaveOffest = 1f;
        public float foamSpawnIntensity = 1f;
        public float foamAttenuate = 0.99f;
        public int startLodIndex = 1;
        public int endLodIndex = 10;
        public int texSize = 512;
        public int lodCount = 5;
        [HideInInspector]
        public List<InteractiveSim> interactiveSims = new List<InteractiveSim>();
        private List<InteractiveSim> interactiveSimsPingPong = new List<InteractiveSim>();
        private List<IWaterSimDraw> waterSimDraws = new List<IWaterSimDraw>();
        private InteractiveSim emptySim;
        private int nowLodLevel;
        [HideInInspector]
        public int ke_HandleVert;
        [HideInInspector]
        public int ke_EnterValue;
        [HideInInspector]
        public int ke_EnterValue2;
        [HideInInspector]
        public int ke_CaculateNextWave;
        [HideInInspector]
        public int ke_PosMove;
        [HideInInspector]
        public int ke_ChangeLevel;
        [HideInInspector]
        public int ke_DrawSquareCircle;
        [HideInInspector]
        public int ke_DrawBoatWake;
        public static readonly int sp_WaveLimitHeight = Shader.PropertyToID("_WaveLimitHeight");
        public static readonly int sp_FoamSpawnIntensity = Shader.PropertyToID("_FoamSpawnIntensity");
        public static readonly int sp_FoamAttenuate = Shader.PropertyToID("_FoamAttenuate");
        public static readonly int sp_FoamSpreadSpeed = Shader.PropertyToID("_FoamSpreadSpeed");
        
        public static readonly int sp_WaveOffest = Shader.PropertyToID("_WaveOffest");
        public static readonly int sp_Tex_Now = Shader.PropertyToID("_Tex_Now");
        public static readonly int sp_FoamTex = Shader.PropertyToID("_FoamTex");
        public static readonly int sp_Tex_LevelUP = Shader.PropertyToID("_Tex_LevelUP");
        public static readonly int sp_Tex_LevelDown = Shader.PropertyToID("_Tex_LevelDown");
        public static readonly int sp_Tex_TargetLevel = Shader.PropertyToID("_Tex_TargetLevel");
        public static readonly int sp_Tex_Last = Shader.PropertyToID("_Tex_Last");
        public static readonly int sp_Tex_Last_LevelUP = Shader.PropertyToID("_Tex_Last_LevelUP");
        public static readonly int sp_Tex_Last_TargetLevel = Shader.PropertyToID("_Tex_Last_TargetLevel");
        public static readonly int sp_OutputTex = Shader.PropertyToID("_OutputTex");
        public static readonly int sp_OutputTexLast = Shader.PropertyToID("_OutputTexLast");
        public static readonly int sp_OutputNormal = Shader.PropertyToID("_OutputNormal");
        public static readonly int sp_NormalFoamTex = Shader.PropertyToID("_NormalFoamTex");
        public static readonly int sp_NormalFoamTex_LevelUP = Shader.PropertyToID("_NormalFoamTex_LevelUP");
        public static readonly int sp_NormalFoamTex_LevelDown = Shader.PropertyToID("_NormalFoamTex_LevelDown");
        public static readonly int sp_NormalFoamTex_TargetLevel = Shader.PropertyToID("_NormalFoamTex_TargetLevel");
        public static readonly int sp_OutputFloat4 = Shader.PropertyToID("_OutputFloat4");
        
        public static readonly int sp_CellSize = Shader.PropertyToID("_CellSize");
        private bool initLevel = false;
        public void AddDrawRequest(IWaterSimDraw waterSimDraw)
        {
            waterSimDraws.Add(waterSimDraw);
        }
        private void ExecuteAllDrawRequest(CommandBuffer cmd)
        {
            foreach (var draw in waterSimDraws)
            {
                for (var i = 0; i < lodCount; i++)
                {
                    if (draw.CheckInPlane(interactiveSims[i].simPos, interactiveSims[i].simSize))
                    {
                        draw.Draw(cmd, interactiveSims[i]);
                    }
                }

            }
            waterSimDraws.Clear();
        }
        private bool CheckLodInSimRange(int index)
        {
            int realIndex = nowLodLevel + index;
            if (realIndex >= startLodIndex && realIndex <= endLodIndex && index < lodCount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckSimInRange(int index)
        {
            int realIndex = nowLodLevel + index;
            if (realIndex >= startLodIndex && realIndex <= endLodIndex)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void CaculateWaveParams(CommandBuffer cmd, float size)
        {
            double blockStep = texSize;
            double dt = Time.fixedDeltaTime;
            double lodMul = size / 1024f;

            double maxWaveStepVisosity = blockStep / (2 * dt) * (double)(Mathf.Sqrt(WaveViscosity * (float)dt + 2));
            double curWaveSpeed = maxWaveStepVisosity * WaveSpeed / (double)lodMul;
            double curWaveSpeedSqr = curWaveSpeed * curWaveSpeed;
            double blockStepSqr = blockStep * blockStep;
            double ut = WaveViscosity * dt;

            double ctdSqr = curWaveSpeedSqr * dt * dt / blockStepSqr;
            float curWave = (float)((4 - 8 * ctdSqr) / (ut + 2));
            float lastWave = (float)((ut - 2) / (ut + 2));
            float aroundWave = (float)((2 * ctdSqr) / (ut + 2));
            try
            {
                cmd.SetComputeFloatParam(interactiveCS, "_CurWave", curWave);

                cmd.SetComputeFloatParam(interactiveCS, "_LastWave", lastWave);
                cmd.SetComputeFloatParam(interactiveCS, "_AroundWave", aroundWave);

                cmd.SetComputeFloatParam(interactiveCS, "_WaveAttenuate", WaveAttenuate);
            }
            catch
            {
                HigSingleInstance<MonoBehaviour>.ResetAllSystem();
            }

        }
        private void SwapSim(InteractiveSim source, InteractiveSim target)
        {
            RenderTexture simWaterTex = source.simWaterTex;
            RenderTexture simWaterTexLast = source.simWaterTexLast;
            RenderTexture simWaterTexNormalFoam = source.simWaterTexNormalFoam;
            target.simSize = source.simSize;
            target.simPos = source.simPos;
            source.simWaterTex = target.simWaterTex;
            source.simWaterTexLast = target.simWaterTexLast;
            source.simWaterTexNormalFoam = target.simWaterTexNormalFoam;
            target.simWaterTex = simWaterTex;
            target.simWaterTexLast = simWaterTexLast;
            target.simWaterTexNormalFoam = simWaterTexNormalFoam;
        }
        private void DeepCopy(CommandBuffer cmd, InteractiveSim source, InteractiveSim target)
        {

            cmd.Blit(source.simWaterTex, target.simWaterTex);
            cmd.Blit(source.simWaterTexLast, target.simWaterTexLast);
            cmd.Blit(source.simWaterTexNormalFoam, target.simWaterTexNormalFoam);

            target.simPos = source.simPos;
        }

        private void SwapTex()
        {
            for (int i = 0; i < interactiveSims.Count; i++)
            {
                SwapSim(interactiveSimsPingPong[i], interactiveSims[i]);
            }

        }
        private void MapLevel(CommandBuffer cmd, InteractiveSim source, InteractiveSim target)
        {
            cmd.SetComputeTextureParam(interactiveCS, ke_ChangeLevel, sp_Tex_Now, target.simWaterTex);
            cmd.SetComputeTextureParam(interactiveCS, ke_ChangeLevel, sp_Tex_Last, target.simWaterTexLast);
            cmd.SetComputeTextureParam(interactiveCS, ke_ChangeLevel, sp_NormalFoamTex, target.simWaterTexNormalFoam);

            cmd.SetComputeTextureParam(interactiveCS, ke_ChangeLevel, sp_Tex_TargetLevel, source.simWaterTex);
            cmd.SetComputeTextureParam(interactiveCS, ke_ChangeLevel, sp_Tex_Last_TargetLevel, source.simWaterTexLast);
            cmd.SetComputeTextureParam(interactiveCS, ke_ChangeLevel, sp_NormalFoamTex_TargetLevel, source.simWaterTexNormalFoam);

            cmd.SetComputeFloatParam(interactiveCS, "_SimSizeTarget", target.simSize);
            cmd.SetComputeFloatParam(interactiveCS, "_SimSize", source.simSize);
            cmd.SetComputeFloatParam(interactiveCS, "_TexSize", texSize);
            target.simPos = source.simPos;
            ExecuteCS(cmd, ke_ChangeLevel);
        }
        private void ChangeLevel(CommandBuffer cmd, int targetlevel)
        {

            int offestLevel = targetlevel - nowLodLevel;
            if (offestLevel != 0 || initLevel)
            {
                initLevel = false;
                for (int i = 0; i < lodCount; i++)
                {
                    int source = i + offestLevel;
                    try
                    {
                        interactiveSimsPingPong[i].simSize = WaterPlaneControl.Instance.GetLodSize(i);
                    }
                    catch
                    {
                        HigSingleInstance<MonoBehaviour>.ResetAllSystem();
                        return;
                    }
                    if (source >= 0 && source < lodCount)
                    {
                        DeepCopy(cmd, interactiveSims[source], interactiveSimsPingPong[i]);

                    }
                    else
                    {
                        if (source < 0)
                        {
                            MapLevel(cmd, interactiveSims[0], interactiveSimsPingPong[i]);
                        }
                        else
                        {
                            MapLevel(cmd, interactiveSims[interactiveSims.Count - 1], interactiveSimsPingPong[i]);
                        }


                    }
                }
                nowLodLevel = targetlevel;
                SwapTex();
            }

        }
        public void UpdateAllSimTex(CommandBuffer cmd)
        {
            if (enable)
            {
                ChangeLevel(cmd, WaterPlaneControl.Instance.lod_Offest);

                for (int i = 0; i < lodCount; i++)
                {
                    if (CheckSimInRange(i))
                    {
                        InteractiveSim levelUpsim;
                        if (i + 1 >= lodCount)
                        {
                            levelUpsim = emptySim;
                        }
                        else
                        {
                            levelUpsim = interactiveSims[i + 1];
                        }
                        Vector3 ringPos = WaterPlaneControl.Instance.ringGrid[i].transform.position;
                        Vector3 newPos = WaterPlaneControl.Instance.SnapToGrid(ringPos, interactiveSims[i].simSize / texSize);
                        CaculatePosChange(cmd, interactiveSims[i], levelUpsim, newPos - interactiveSims[i].simPos);
                        interactiveSims[i].simPos = newPos;
                        CaculateSimNext(cmd, interactiveSims[i], levelUpsim);
                    }
                    else
                    {
                        Vector3 ringPos = WaterPlaneControl.Instance.ringGrid[i].transform.position;
                        try
                        {
                            interactiveSims[i].simPos = ringPos;
                        }
                        catch
                        {
                            HigSingleInstance<MonoBehaviour>.ResetAllSystem();
                        }
                    }


                }


                ExecuteAllDrawRequest(cmd);
                for (int i = 0; i < WaterPlaneControl.Instance.lodCount; i++)//Set Sim Tex to Water Shader
                {
                    if (CheckLodInSimRange(i))
                    {
                        SetMatAttribute(i, interactiveSims[i], cmd);
                    }
                    else
                    {
                        if (i < startLodIndex)
                        {
                            if (startLodIndex - nowLodLevel >= 0)
                            {
                                SetMatAttribute(i, interactiveSims[startLodIndex - nowLodLevel], cmd);
                            }
                            else
                            {
                                SetMatAttribute(i, emptySim, cmd);
                            }
                        }
                        else
                        {
                            SetMatAttribute(i, emptySim, cmd);
                        }

                    }
    ;
                }
            }

        }
        private static Vector2 Vec3ToVec2(Vector3 pos)
        {
            return new Vector2 (pos.x, pos.z);
        }
        private void SetMatAttribute(int lod, InteractiveSim interactiveSim, CommandBuffer cmd)
        {
            try
            {
                Material mat = WaterPlaneControl.Instance.waterMats[lod];
                RenderTexture outRT = RTManager.Instance.GetRT("interactiveRT" + lod.ToString(), texSize, RenderTextureFormat.ARGBFloat);
                RenderTexture outFoamRT = RTManager.Instance.GetRT("interactiveFoamRT" + lod.ToString(), texSize, RenderTextureFormat.RFloat);
                cmd.SetComputeTextureParam(interactiveCS, ke_HandleVert, sp_NormalFoamTex, interactiveSim.simWaterTexNormalFoam);

                cmd.SetComputeTextureParam(interactiveCS, ke_HandleVert, sp_Tex_Now, interactiveSim.simWaterTex);
                cmd.SetComputeFloatParam(interactiveCS, sp_WaveOffest, WaveOffest);
                cmd.SetComputeTextureParam(interactiveCS, ke_HandleVert, sp_OutputFloat4, outRT);
                cmd.SetComputeTextureParam(interactiveCS, ke_HandleVert, sp_FoamTex, outFoamRT);
                ExecuteCS(cmd, ke_HandleVert);
                Vector2 offestPos = -Vec3ToVec2(interactiveSim.simPos - WaterPlaneControl.Instance.ringGrid[lod].transform.position);
                RTFunction.Instance.MapTexture(cmd, outRT, TextureManager.Instance.vertTexture[lod], interactiveSim.simSize, WaterPlaneControl.Instance.GetLodSize(lod), offestPos, false, 1, 0);
                RTFunction.Instance.MapTexture(cmd, interactiveSim.simWaterTexNormalFoam, TextureManager.Instance.normalTexture[lod], interactiveSim.simSize, WaterPlaneControl.Instance.GetLodSize(lod), offestPos, true, 1, 0);
                RTFunction.Instance.MapTexture(cmd, outFoamRT, TextureManager.Instance.foamTexture[lod], interactiveSim.simSize, WaterPlaneControl.Instance.GetLodSize(lod), offestPos, false, 1, 0);
                mat.SetFloat("_InteractiveWaterSimSize", interactiveSim.simSize);
                mat.SetVector("_InteractiveWaterSimPos", Vec3ToVec2(interactiveSim.simPos - WaterPlaneControl.Instance.ringGrid[lod].transform.position));
                mat.SetTexture("_InteractiveWaterNormalTex", interactiveSim.simWaterTexNormalFoam);
            }
            catch
            {
                HigSingleInstance<MonoBehaviour>.ResetAllSystem();
            }

        }
        private void ExecuteCS(CommandBuffer cmd, int kernel)
        {
            cmd.DispatchCompute(interactiveCS, kernel, texSize / 8, texSize / 8, 1);
        }
        private void CaculatePosChange(CommandBuffer cmd, InteractiveSim targetSim, InteractiveSim upLevelSim, Vector3 worldPosOffest)
        {
            RenderTexture outputTex = RTManager.Instance.GetRT("temRT", texSize, RenderTextureFormat.RFloat);
            RenderTexture outputTexLast = RTManager.Instance.GetRT("temRT2", texSize, RenderTextureFormat.RFloat);
            RenderTexture outputTexNormal = RTManager.Instance.GetRT("temRTNormal", texSize, RenderTextureFormat.ARGBFloat);
            try
            {
                cmd.SetComputeTextureParam(interactiveCS, ke_PosMove, sp_Tex_Now, targetSim.simWaterTex);
                cmd.SetComputeTextureParam(interactiveCS, ke_PosMove, sp_Tex_Last, targetSim.simWaterTexLast);
                cmd.SetComputeTextureParam(interactiveCS, ke_PosMove, sp_NormalFoamTex, targetSim.simWaterTexNormalFoam);
                cmd.SetComputeTextureParam(interactiveCS, ke_PosMove, sp_Tex_LevelUP, upLevelSim.simWaterTex);
                cmd.SetComputeTextureParam(interactiveCS, ke_PosMove, sp_Tex_Last_LevelUP, upLevelSim.simWaterTexLast);
                cmd.SetComputeTextureParam(interactiveCS, ke_PosMove, sp_NormalFoamTex_LevelUP, upLevelSim.simWaterTexNormalFoam);
                cmd.SetComputeTextureParam(interactiveCS, ke_PosMove, sp_OutputTex, outputTex);
                cmd.SetComputeTextureParam(interactiveCS, ke_PosMove, sp_OutputTexLast, outputTexLast);
                cmd.SetComputeTextureParam(interactiveCS, ke_PosMove, sp_OutputNormal, outputTexNormal);
                cmd.SetComputeVectorParam(interactiveCS, "_PosOffest", worldPosOffest);
                cmd.SetComputeFloatParam(interactiveCS, "_SimSize", targetSim.simSize);
                cmd.SetComputeFloatParam(interactiveCS, "_TexSize", texSize);

                ExecuteCS(cmd, ke_PosMove);
                cmd.SetComputeTextureParam(interactiveCS, ke_EnterValue2, sp_Tex_Now, targetSim.simWaterTex);
                cmd.SetComputeTextureParam(interactiveCS, ke_EnterValue2, sp_Tex_Last, targetSim.simWaterTexLast);
                cmd.SetComputeTextureParam(interactiveCS, ke_EnterValue2, sp_NormalFoamTex, targetSim.simWaterTexNormalFoam);
                cmd.SetComputeTextureParam(interactiveCS, ke_EnterValue2, sp_OutputTex, outputTex);
                cmd.SetComputeTextureParam(interactiveCS, ke_EnterValue2, sp_OutputTexLast, outputTexLast);
                cmd.SetComputeTextureParam(interactiveCS, ke_EnterValue2, sp_OutputNormal, outputTexNormal);
                ExecuteCS(cmd, ke_EnterValue2);
            }
            catch
            {
                HigSingleInstance<MonoBehaviour>.ResetAllSystem();
            }
        }
        private void CaculateSimNext(CommandBuffer cmd, InteractiveSim interactiveSim, InteractiveSim levelUpSim)
        {

            CaculateWaveParams(cmd, interactiveSim.simSize);
            try
            {
                RenderTexture outputTex = RTManager.Instance.GetRT("temRT", texSize, RenderTextureFormat.RFloat);
                RenderTexture outputNormalTex = RTManager.Instance.GetRT("temNormalRT", texSize, RenderTextureFormat.ARGBFloat);
                cmd.SetComputeFloatParam(interactiveCS, sp_FoamAttenuate, foamAttenuate);
                cmd.SetComputeFloatParam(interactiveCS, sp_FoamSpawnIntensity, foamSpawnIntensity);

            cmd.SetComputeTextureParam(interactiveCS, ke_CaculateNextWave, sp_Tex_Now, interactiveSim.simWaterTex);
            cmd.SetComputeTextureParam(interactiveCS, ke_CaculateNextWave, sp_Tex_Last, interactiveSim.simWaterTexLast);
            cmd.SetComputeTextureParam(interactiveCS, ke_CaculateNextWave, sp_Tex_LevelUP, levelUpSim.simWaterTex);
            cmd.SetComputeTextureParam(interactiveCS, ke_CaculateNextWave, sp_NormalFoamTex, interactiveSim.simWaterTexNormalFoam);
            cmd.SetComputeTextureParam(interactiveCS, ke_CaculateNextWave, sp_NormalFoamTex_LevelUP, levelUpSim.simWaterTexNormalFoam);
            cmd.SetComputeTextureParam(interactiveCS, ke_CaculateNextWave, sp_OutputNormal, outputNormalTex);
            cmd.SetComputeTextureParam(interactiveCS, ke_CaculateNextWave, sp_OutputTex, outputTex);
            cmd.SetComputeFloatParam(interactiveCS, "_TexSize", texSize);
            ExecuteCS(cmd, ke_CaculateNextWave);
            cmd.SetComputeTextureParam(interactiveCS, ke_EnterValue, sp_Tex_Now, interactiveSim.simWaterTex);
            cmd.SetComputeTextureParam(interactiveCS, ke_EnterValue, sp_Tex_Last, interactiveSim.simWaterTexLast);
            cmd.SetComputeTextureParam(interactiveCS, ke_EnterValue, sp_NormalFoamTex, interactiveSim.simWaterTexNormalFoam);
            cmd.SetComputeTextureParam(interactiveCS, ke_EnterValue, sp_OutputTex, outputTex);
            cmd.SetComputeTextureParam(interactiveCS, ke_EnterValue, sp_OutputNormal, outputNormalTex);
            cmd.SetComputeFloatParam(interactiveCS, sp_CellSize, interactiveSim.simSize / (float)texSize);
            cmd.SetComputeFloatParam(interactiveCS, sp_WaveLimitHeight, WaveLimitHeight);

            ExecuteCS(cmd, ke_EnterValue);
            }
            catch
            {
                HigSingleInstance<MonoBehaviour>.ResetAllSystem();
            }


        }
        public void Start()
        {
            waterSimDraws.Clear();
        }
        public override void ResetSystem()
        {
            nowLodLevel = startLodIndex;
            GetKernel();
            ReleaseTex();
            waterSimDraws.Clear();
            InitSims();
            initLevel = true;
        }
        private void GetKernel()
        {
            ke_HandleVert = interactiveCS.FindKernel("HandleVert");
            ke_EnterValue = interactiveCS.FindKernel("EnterValue");
            ke_EnterValue2 = interactiveCS.FindKernel("EnterValue2");
            ke_CaculateNextWave = interactiveCS.FindKernel("CaculateNextWave");
            ke_PosMove = interactiveCS.FindKernel("PosMove");
            ke_ChangeLevel = interactiveCS.FindKernel("ChangeLevel");
            ke_DrawSquareCircle = interactiveDrawCS.FindKernel("DrawSquareCircle");
            ke_DrawBoatWake = interactiveDrawCS.FindKernel("DrawKelvinWake");
        }
        private void ReleaseTex()
        {
            foreach (InteractiveSim sim in interactiveSims)
            {
                sim.DestoryTex();
            }
            interactiveSims.Clear();
            foreach (InteractiveSim sim in interactiveSimsPingPong)
            {
                sim.DestoryTex();
            }
            interactiveSimsPingPong.Clear();

            emptySim?.DestoryTex();
        }
        private void InitSims()
        {

            for (int i = 0; i < lodCount; i++)
            {
                RenderTexture RT = RTManager.Instance.GetRT("InteractiveWaterSimTex" + i.ToString(), texSize, RenderTextureFormat.RFloat);
                RenderTexture RTL = RTManager.Instance.GetRT("InteractiveWaterSimTexLast" + i.ToString(), texSize, RenderTextureFormat.RFloat);
                RenderTexture NormalRT = RTManager.Instance.GetRT("InteractiveWaterNormalFoam" + i.ToString(), texSize, RenderTextureFormat.ARGBFloat);
                InteractiveSim interactiveSim = new InteractiveSim { simWaterTex = RT, simWaterTexLast = RTL, simWaterTexNormalFoam = NormalRT };
                interactiveSim.InitValue();
                interactiveSims.Add(interactiveSim);
                RenderTexture RTP = RTManager.Instance.GetRT("InteractiveWaterSimTex2" + i.ToString(), texSize, RenderTextureFormat.RFloat);
                RenderTexture RTLP = RTManager.Instance.GetRT("InteractiveWaterSimTexLast2" + i.ToString(), texSize, RenderTextureFormat.RFloat);
                RenderTexture NormalRTP = RTManager.Instance.GetRT("InteractiveWaterNormalFoam2" + i.ToString(), texSize, RenderTextureFormat.ARGBFloat);
                interactiveSim = new InteractiveSim { simWaterTex = RTP, simWaterTexLast = RTLP, simWaterTexNormalFoam = NormalRTP };
                interactiveSim.InitValue();
                interactiveSimsPingPong.Add(interactiveSim);
            }
            RenderTexture RTE = RTManager.Instance.GetRT("InteractiveWaterSimTexEmpty", texSize, RenderTextureFormat.RFloat);
            RenderTexture RTLE = RTManager.Instance.GetRT("InteractiveWaterSimTexLastEmpty", texSize, RenderTextureFormat.RFloat);
            RenderTexture NormalRTE = RTManager.Instance.GetRT("InteractiveWaterNormalFoamEmpty", texSize, RenderTextureFormat.ARGBFloat);

            emptySim = new InteractiveSim { simWaterTex = RTE, simWaterTexLast = RTLE, simWaterTexNormalFoam = NormalRTE };
            emptySim.InitValue();
        }
    }
    public class InteractiveSim
    {
        public RenderTexture simWaterTex;
        public RenderTexture simWaterTexLast;
        public RenderTexture simWaterTexNormalFoam;
        public bool enableToUpdate;
        public float simSize;
        public Vector3 simPos;
        public void InitValue()
        {
            RTFunction.Instance.ResetValue(simWaterTex, new Vector4(0, 0, 0, 0));
            RTFunction.Instance.ResetValue(simWaterTexLast, new Vector4(0, 0, 0, 0));
            RTFunction.Instance.ResetValue(simWaterTexNormalFoam, new Vector4(0, 1, 0, 0));

        }
        public void DestoryTex()
        {
            GameObject.DestroyImmediate(simWaterTex);
            GameObject.DestroyImmediate(simWaterTexLast);
            GameObject.DestroyImmediate(simWaterTexNormalFoam);
        }
    }
    public interface IWaterSimDraw
    {
        public bool CheckInPlane(Vector3 lodPos, float simSize);

        public void Draw(CommandBuffer cmd, InteractiveSim sim);

    }
}