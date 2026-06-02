using System;
using moe.nikky.common;
using moe.nikky.kinetic_controls.control;
using UnityEngine;
using VRC;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.driver.control.kinetic
{

    public class IntSmoothingFramesDriver : IntDriver
    {
        [Header("External Behaviours")] // header
        [SerializeField]
        private GameObject smoothedControlSource;

        [SerializeField] [ReadOnly] [NonReorderable] private SmoothedControl[] smoothedControls = { };

        protected override string LogPrefix => nameof(IntSmoothingFramesDriver);

        void Start()
        {
            _EnsureInit();
        }

        protected override void OnUpdateInt(int value)
        {
            if (!enabled) return;
            if (value <= 0)
            {
                LogError("value must be greater than 0");
                return;
            }

            foreach (var behaviour in smoothedControls)
            {
                if (Utilities.IsValid(behaviour))
                {
                    behaviour.SmoothingFrames = value;

#if UNITY_EDITOR && !COMPILER_UDONSHARP
                    if (!Application.isPlaying)
                    {
                        behaviour.MarkDirty();
                    }
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

        public override void OnPreprocess()
        {
            base.OnPreprocess();

            FindSmoothedBehaviours();
        }
#endif
    }
}