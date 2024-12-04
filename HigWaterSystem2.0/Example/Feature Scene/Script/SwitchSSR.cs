using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class SwitchSSR : MonoBehaviour
    {
        public void EnableSSR_Refraction(bool enable)
        {
            ReflectionManager.Instance.refractionType = enable ? ReflectionManager.LightType.SSR : ReflectionManager.LightType.fake;

        }
        public void EnableSSR_Reflection(bool enable)
        {
            ReflectionManager.Instance.reflectionType = enable ? ReflectionManager.LightType.SSR : ReflectionManager.LightType.fake;

        }
        void Start()
        {

        }

        void Update()
        {

        }
    }
}