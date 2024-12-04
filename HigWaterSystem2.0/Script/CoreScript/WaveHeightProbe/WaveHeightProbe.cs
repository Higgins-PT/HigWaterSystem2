
using UnityEngine;
namespace HigWaterSystem2
{
    [ExecuteInEditMode]
    public class WaveHeightProbe : MonoBehaviour
    {
        public bool alwaysUpdate = true;
        [HideInInspector]
        private bool refreshRequest;
        private bool isQuitting;
        public int maxTexSize = 512;
        public Vector3 probeSize = new Vector3(20f, 20f, 20f);
        public WaveHeightProbeRenderRequest waveHeightProbeRenderRequest;
        public void FixedUpdate()
        {
            waveHeightProbeRenderRequest = new WaveHeightProbeRenderRequest(probeSize, maxTexSize, transform.position, transform.rotation.eulerAngles.y);

        }
        public void RequestRefresh() { refreshRequest = true; }
        public bool GetRefreshBool()
        {
            if (alwaysUpdate || refreshRequest)
            {
                refreshRequest = false;
                return true;
            }
            return false;
        }
        private void OnDrawGizmosSelected()
        {

            WaveHeightProbeRenderRequest request = waveHeightProbeRenderRequest;

            Gizmos.color = Color.cyan;
            waveHeightProbeRenderRequest = new WaveHeightProbeRenderRequest(probeSize, maxTexSize, transform.position, transform.rotation.eulerAngles.y);
            Gizmos.matrix = Matrix4x4.TRS(request.centerPos, Quaternion.Euler(0, request.rot, 0), Vector3.one);


            Gizmos.DrawWireCube(Vector3.zero, request.size);
            Gizmos.matrix = Matrix4x4.identity;
        }

        void OnApplicationQuit()
        {
            if (Application.isPlaying)
            {
                isQuitting = true;
            }
        }
        public void Start()
        {

            if (Application.isPlaying)
            {
                isQuitting = false;
            }
        }
        public void OnEnable()
        {

            if (Application.isPlaying)
            {
                try
                {
                    RequestRefresh();
                    waveHeightProbeRenderRequest = new WaveHeightProbeRenderRequest(probeSize, maxTexSize, transform.position, transform.rotation.eulerAngles.y);
                    WaveHeightProbeManager.Instance.AddProbe(this);
                }
                catch
                {

                }
            }
        }
        public void OnDisable()
        {
            if (Application.isPlaying)
            {
                if (isQuitting == false)
                {
                    try
                    {
                        WaveHeightProbeManager.Instance.RemoveProbe(this);
                    }
                    catch
                    {

                    }
                }
            }
        }
    }
}