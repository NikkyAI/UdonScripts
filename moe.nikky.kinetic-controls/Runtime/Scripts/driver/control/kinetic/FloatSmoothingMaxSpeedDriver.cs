using System;
using moe.nikky.common;
using moe.nikky.common.Editor;
using moe.nikky.kinetic_controls.control;
using UnityEngine;
using VRC;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.driver.control.kinetic
{
#if UNITY_EDITOR && !COMPILER_UDONSHARP
    [RequireComponent(typeof(PreProcessEditorHelper))]
#endif
    public class FloatSmoothingMaxSpeedDriver : FloatDriver
    {
        [Header("External Behaviours")] // header
        [SerializeField]
        private GameObject smoothedControlSource;

        [SerializeField] [ReadOnly] private SmoothedControl[] smoothedControls = { };

        protected override string LogPrefix => nameof(FloatSmoothingMaxSpeedDriver);

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
                    behaviour.smoothingMaxSpeed = value;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
                    behaviour.MarkDirty();
#endif
                }
            }
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected override bool UpdateInEditor => true;


        private void FindSmoothedBehaviours()
        {
            // _valueBoolDrivers = _valueBoolDrivers.AddRange(gameObject.GetComponents<BoolDriver>());
            smoothedControls = Array.Empty<SmoothedControl>();
            if (Utilities.IsValid(smoothedControlSource))
            {
                smoothedControls = smoothedControlSource.GetComponentsInChildren<SmoothedControl>();
            }

            Log($"found {smoothedControls.Length} smoothed controls");
        }

        public override bool OnPreprocess()
        {
            if (!base.OnPreprocess())
            {
                return false;
            }

            FindSmoothedBehaviours();

            return true;
        }
#endif
    }
}