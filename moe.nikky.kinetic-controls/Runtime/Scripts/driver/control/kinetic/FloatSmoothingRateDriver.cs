using moe.nikky.common;
using moe.nikky.kinetic_controls.control;
using UnityEngine;
using UnityEngine.Serialization;
using VRC;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.driver.control.kinetic
{
    public class FloatSmoothingRateDriver : FloatDriver
    {
        [FormerlySerializedAs("smoothedBehaviours")]
        [Header("Deprecated, use FloatSmoothingTimeDriver and FloatSmoothingMaxSpeedDriver instead")]
        [Header("External Behaviours")] // header
        [FormerlySerializedAs("faders")]
        [SerializeField]
        private SmoothedControl[] smoothedControls = {};

        protected override string LogPrefix => nameof(FloatSmoothingRateDriver);

        void Start()
        {
            _EnsureInit();
        }

        protected override void OnUpdateFloat(float value)
        {
            if (!enabled) return;
            if (value <= 0f)
            {
                LogError("value must be greater than 0");
                return;
            }

            foreach (var behaviour in smoothedControls)
            {
                if (Utilities.IsValid(behaviour))
                {
                    // commented to prevent breakage
                    // behaviour.SmoothingRate = value;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
                    behaviour.MarkDirty();
#endif
                }

            }
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected override bool UpdateInEditor => true;
#endif
    }
}