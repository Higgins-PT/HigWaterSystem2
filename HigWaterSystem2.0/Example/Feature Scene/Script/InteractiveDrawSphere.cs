using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    public class InteractiveDrawSphere : MonoBehaviour
    {
        public float radius = 5f;
 

        [HideInInspector]
        public Vector3 lastPos; 
        public float GetWaterHeight()
        {
            return OceanPhysics.Instance.GetOceanHeight(transform.position);
        }
        private void Update()
        {
            float speed = (transform.position - lastPos).magnitude;
            lastPos = transform.position;
            float deltaHeight = Mathf.Abs(transform.position.y - GetWaterHeight());
            if (deltaHeight < radius)
            {
                InteractiveWaterManager.Instance.AddDrawRequest(new DrawCircleRequest(this.transform.position, (radius - deltaHeight), 1 * Mathf.Clamp(speed, 0, 10), 1));
            }
            
        }
    }
}