using moe.nikky.common;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace moe.nikky.kinetic_controls.driver.udon
{
    public class BoolUdonDriver : BoolDriver
    {
        [Header("External Behaviours")] // header
        [SerializeField]
        private UdonBehaviour[] externalBehaviours;

        [SerializeField]
        private string boolField;
        [FormerlySerializedAs("eventName")] [SerializeField]
        private string eventNameChanged;
        [SerializeField]
        private string eventNameTrue;
        [SerializeField]
        private string eventNameFalse;

        protected override string LogPrefix => nameof(BoolUdonDriver);

        void Start()
        {
            _EnsureInit();
        }
    
    
        public override void OnUpdateBool(bool value)
        {
            if (boolField.Length > 0)
            {
                for (var i = 0; i < externalBehaviours.Length; i++)
                {
                    var ext = externalBehaviours[i];
                    if (Utilities.IsValid(ext))
                    {
                        ext.SetProgramVariable(boolField, value);
                    }
                }
            }

            if (eventNameChanged.Length > 0)
            {
                for (var i = 0; i < externalBehaviours.Length; i++)
                {
                    var ext = externalBehaviours[i];
                    if (Utilities.IsValid(ext))
                    {
                        ext.SendCustomEvent(eventNameChanged);
                    }
                }
            }
            if (value)
            {
                if (eventNameTrue.Length > 0)
                {
                    for (var i = 0; i < externalBehaviours.Length; i++)
                    {
                        var ext = externalBehaviours[i];
                        if (Utilities.IsValid(ext))
                        {
                            ext.SendCustomEvent(eventNameTrue);
                        }
                    }
                }
            }
            else
            {
                if (eventNameFalse.Length > 0)
                {
                    for (var i = 0; i < externalBehaviours.Length; i++)
                    {
                        var ext = externalBehaviours[i];
                        if (Utilities.IsValid(ext))
                        {
                            ext.SendCustomEvent(eventNameFalse);
                        }
                    }
                }
            }
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public override void ApplyBoolValue(bool value)
        {
            base.ApplyBoolValue(value);
            OnUpdateBool(value);
            
        }
#endif
    }
}
