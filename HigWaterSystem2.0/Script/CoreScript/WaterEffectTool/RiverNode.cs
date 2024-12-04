using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HigWaterSystem2
{
    [ExecuteInEditMode]
    public class RiverNode : MonoBehaviour
    {
        [HideInInspector]
        public RiverPoint riverPoint = new RiverPoint { width = 5f };
        private float lastWidth;
        public float width = 5f;
        private Vector3 lastPosition;
        public delegate void OnPositionChanged();
        public event OnPositionChanged PositionChanged;
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.position, width / 40);
        }
        public void CleanAction()
        {
            PositionChanged = null;
        }
        void Start()
        {
            lastPosition = transform.localPosition;
        }

        void Update()
        {
            if (transform.localPosition != lastPosition || lastWidth != width)
            {
                PositionChanged?.Invoke();
                lastPosition = transform.localPosition;
                lastWidth = width;
                riverPoint.worldPos = transform.position;
                riverPoint.width = width;
            }
        }

    }
}