using HigWaterSystem2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class OceanSound : MonoBehaviour
    {
        public List<OceanSoundEntry> soundEntries = new List<OceanSoundEntry>();

        [Range(0, 1)]
        public float seaState = 0f;

        void Start()
        {

            foreach (var entry in soundEntries)
            {
                if (entry.audioClip != null)
                {

                    AudioSource source = gameObject.AddComponent<AudioSource>();
                    source.clip = entry.audioClip;
                    source.loop = true;
                    source.playOnAwake = false;
                    source.volume = 0f; 


                    entry.audioSource = source;

                    source.Play();
                }
                else
                {

                }
            }
        }

        void Update()
        {
            float cameraHeight = WaterPlaneControl.Instance.CamH;

            foreach (var entry in soundEntries)
            {
                if (entry.audioSource == null)
                    continue;

                bool withinSeaState = seaState >= entry.seaStateRange.MinValue && seaState <= entry.seaStateRange.MaxValue;

                float minHeight = entry.cameraHeightRange.MinValue;
                float maxHeight = entry.cameraHeightRange.MaxValue;

                if (cameraHeight <= -1000f)
                    cameraHeight = -1000f;
                if (cameraHeight >= 1000f)
                    cameraHeight = 1000f;

                bool withinCameraHeight = cameraHeight >= minHeight && cameraHeight <= maxHeight;

                if (withinSeaState && withinCameraHeight)
                {
                    float seaStateRange = entry.seaStateRange.MaxValue - entry.seaStateRange.MinValue;
                    float seaStateRatio = seaStateRange > 0f ? (seaState - entry.seaStateRange.MinValue) / seaStateRange : 1f;

                    float cameraHeightRange = entry.cameraHeightRange.MaxValue - entry.cameraHeightRange.MinValue;
                    float cameraHeightRatio = cameraHeightRange > 0f ? (cameraHeight - entry.cameraHeightRange.MinValue) / cameraHeightRange : 1f;

                    seaStateRatio = Mathf.Clamp01(seaStateRatio);
                    cameraHeightRatio = Mathf.Clamp01(cameraHeightRatio);

                    float volume = Mathf.Pow(seaStateRatio * cameraHeightRatio, entry.powCorrection);

                    entry.targetVolume = volume;
                }
                else
                {
                    entry.targetVolume = 0f;
                }

                entry.audioSource.volume = Mathf.Lerp(entry.audioSource.volume, entry.targetVolume, Time.deltaTime * 5f);
            }
        }
    }
    [System.Serializable]
    public class OceanSoundEntry
    {
        public AudioClip audioClip;

        [MinMaxRange(0f, 1f)]
        public RangedFloat seaStateRange;

        [MinMaxRange(-1000f, 1000f)]
        public RangedFloat cameraHeightRange;

        public float powCorrection = 1.0f;

        [HideInInspector]
        public float targetVolume = 0f;

        [HideInInspector]
        public AudioSource audioSource;
    }
}