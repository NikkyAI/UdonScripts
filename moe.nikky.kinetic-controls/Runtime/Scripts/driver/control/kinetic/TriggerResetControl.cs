using moe.nikky.common;
using moe.nikky.kinetic_controls.control;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.driver.control.kinetic
{
    public class TriggerResetControl : TriggerDriver
    {
        [FormerlySerializedAs("smoothedBehaviours")]//
        [SerializeField] private SmoothedControl[] smoothedControls = { };

        void Start()
        {
            _EnsureInit();
        }

        protected override string LogPrefix => nameof(TriggerResetControl);

        public override void OnTrigger()
        {
            if (!enabled) return;
            Log("triggered reset");
            for (var i = 0; i < smoothedControls.Length; i++)
            {
                var behaviour = smoothedControls[i];
                if (Utilities.IsValid(behaviour))
                { 
                    Log($"resetting {behaviour.name}");
                    behaviour.Reset();
                }
                else
                {
                    LogError($"behaviour {i} was not valid");
                }
            }
        }
    }
}
