using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace HigWaterSystem2
{
    public class MinMaxRangeAttribute : PropertyAttribute
    {
        public float Min;
        public float Max;

        public MinMaxRangeAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

    [System.Serializable]
    public class RangedFloat
    {
        public float MinValue;
        public float MaxValue;
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
    public class MinMaxRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            MinMaxRangeAttribute range = (MinMaxRangeAttribute)attribute;

            if (property.type == "RangedFloat")
            {
                SerializedProperty minValue = property.FindPropertyRelative("MinValue");
                SerializedProperty maxValue = property.FindPropertyRelative("MaxValue");

                float min = minValue.floatValue;
                float max = maxValue.floatValue;

                EditorGUI.BeginProperty(position, label, property);

                Rect controlRect = EditorGUI.PrefixLabel(position, label);
                float fieldWidth = 45f;
                float sliderWidth = controlRect.width - (fieldWidth * 2) - 4f;

                Rect minFieldRect = new Rect(controlRect.x, controlRect.y, fieldWidth, controlRect.height);
                Rect sliderRect = new Rect(controlRect.x + fieldWidth + 2f, controlRect.y, sliderWidth, controlRect.height);
                Rect maxFieldRect = new Rect(controlRect.x + fieldWidth + sliderWidth + 4f, controlRect.y, fieldWidth, controlRect.height);

                min = EditorGUI.FloatField(minFieldRect, min);
                max = EditorGUI.FloatField(maxFieldRect, max);

                EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, range.Min, range.Max);

                minValue.floatValue = min;
                maxValue.floatValue = max;

                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use MinMaxRange with RangedFloat only.");
            }
        }
    }
#endif
}