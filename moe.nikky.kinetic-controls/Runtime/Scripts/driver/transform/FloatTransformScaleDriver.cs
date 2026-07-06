using moe.nikky.common;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.driver.transform
{
    public class FloatTransformScaleDriver : FloatDriver
    {
        public Transform targetTransform;
        public float tweenDuration = 0.5f;
        public VRCTweenEase easeType = VRCTweenEase.InOutSine;
        
        void Start()
        {
            _EnsureInit();
        }

        private VRCTweenHandle tweenHandle;

        protected override string LogPrefix => nameof(FloatTransformScaleDriver);
        protected override void OnUpdateFloat(float value)
        {
            if (Utilities.IsValid(tweenHandle))
            {
                tweenHandle.Kill();
            }

            tweenHandle = targetTransform.TweenScale(new Vector3(value, value, value), tweenDuration, easeType)
                .SetDelay(0.1f);
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected override bool UpdateInEditor => true;

        protected override void PostEditorUpdate(float value)
        {
            // targetTransform.SetDirty();
        }
#endif
    }
}
