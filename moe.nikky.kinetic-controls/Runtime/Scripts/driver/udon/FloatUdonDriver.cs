using System.Linq;
using moe.nikky.common;
using moe.nikky.common.utils;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace moe.nikky.kinetic_controls.driver.udon
{
    public class FloatUdonDriver : FloatDriver
    {
        [Header("External Behaviours")] // header
        [SerializeField]
        private UdonBehaviour[] externalBehaviours;

        [SerializeField] private string floatField;
        
        [FormerlySerializedAs("eventName")]
        [Tooltip("this PUBLIC methid is called after the field has been updated")]
        [SerializeField] private string eventNameUdon;
        [Tooltip("this method is getting called instead of the event name when updating in the unity editor")]
        [SerializeField] private string eventNameEditor;

        protected override string LogPrefix => nameof(FloatUdonDriver);

        void Start()
        {
            _EnsureInit();
        }

        protected override void OnUpdateFloat(float value)
        {
            for (var i = 0; i < externalBehaviours.Length; i++)
            {
                var ext = externalBehaviours[i];
                if (Utilities.IsValid(ext))
                {
                    ext.SetProgramVariable(floatField, value);
                }
            }

            if (eventNameUdon.Length > 0)
            {
                for (var i = 0; i < externalBehaviours.Length; i++)
                {
                    var ext = externalBehaviours[i];
                    if (Utilities.IsValid(ext))
                    {
                        // Log("sending event " + eventName + " to " + ext);
                        ext.SendCustomEvent(eventNameUdon);
#if UNITY_EDITOR && !COMPILER_UDONSHARP
//                         var componentIndex = ext.gameObject.GetComponentIndex(ext);
//                         if (componentIndex >= 0)
//                         {
//                             var baseBehaviour = ext.gameObject.GetComponentAtIndex<BaseBehaviour>(componentIndex);
//                             if (baseBehaviour != null)
//                             {
//                                 Log("found base behaviour " + baseBehaviour);
//                             }
//                         }
#endif
                    }
                }
            }
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected override bool UpdateInEditor => true;

        protected override void PostEditorUpdate(float value)
        {
            foreach (var ext in externalBehaviours)
            {
                if (Utilities.IsValid(ext))
                {
                    if (UdonsharpFinder.Find(ext, out var udonSharpBehaviour))
                    {
                        LogDebug($"found {udonSharpBehaviour}");
                        var serializedObj = new SerializedObject(udonSharpBehaviour);
                        var property = serializedObj.FindProperty(floatField);
                        if (property != null)
                        {
                            LogDebug($"found {floatField}");
                            if (!Mathf.Approximately(property.floatValue, value))
                            {
                                Log($"setting value of {floatField} to {value}");
                                property.floatValue = value;
                                serializedObj.ApplyModifiedProperties();
                            }
                        }

                        if (eventNameEditor.Length > 0)
                        {
                            Log($"invoking {eventNameEditor}");
                            udonSharpBehaviour.Invoke(eventNameEditor, 0f);
                        }
                    }
                }
            }
        }
#endif
    }
}