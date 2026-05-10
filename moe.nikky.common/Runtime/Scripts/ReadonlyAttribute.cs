using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace moe.nikky.common
{
    public class ReadOnlyAttribute : PropertyAttribute
    {
        internal string _fieldToCheck;
        internal bool _invert;

        public ReadOnlyAttribute()
        {
            _fieldToCheck = null;
        }

        public ReadOnlyAttribute(string fieldToCheck)
        {
            _fieldToCheck = fieldToCheck;
        }

        public ReadOnlyAttribute(string fieldToCheck, bool invert)
        {
            _fieldToCheck = fieldToCheck;
            _invert = invert;
        }
    }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
            GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            if (!(attribute is ReadOnlyAttribute readonlyAttr)) return;

            bool isReadOnly;
            if (readonlyAttr._fieldToCheck != null)
            {
                isReadOnly = false;
                var targetObject = property.serializedObject.targetObject;
                var contextType = targetObject.GetType();
                // Debug.Log($"found type {contextType} of {targetObject}", targetObject);
                var checkFieldInfo = contextType.GetField(readonlyAttr._fieldToCheck,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var checkPropInfo = contextType.GetProperty(readonlyAttr._fieldToCheck,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (checkFieldInfo != null)
                {
                    isReadOnly = (bool)checkFieldInfo.GetValue(targetObject);
                }
                else if (checkPropInfo != null)
                {
                    isReadOnly = (bool)checkPropInfo.GetValue(targetObject, null);
                }
                // Debug.Log($"found {readonlyAttr._fieldToCheck} {isReadOnly} in {targetObject}", targetObject);


                // var checkProperty = property.serializedObject.FindProperty(readonlyAttr._fieldToCheck);
                // if (checkProperty != null && checkProperty.type == "bool")
                // {
                //     isReadOnly = checkProperty.boolValue;
                //     if (readonlyAttr._invert)
                //     {
                //         isReadOnly = !isReadOnly;
                //     }
                // }
                if (readonlyAttr._invert) isReadOnly = !isReadOnly;
            }
            else
            {
                isReadOnly = true;
            }


            GUI.enabled = !isReadOnly;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
#endif
}