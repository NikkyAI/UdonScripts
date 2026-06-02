using moe.nikky.common;
using UnityEngine;

namespace moe.nikky.kinetic_controls.driver.transform
{
    public class FloatTransformScaleDriver : FloatDriver
    {
        public Transform targetTransform;
        void Start()
        {
            _EnsureInit();
        }

        protected override string LogPrefix => nameof(FloatTransformScaleDriver);
        protected override void OnUpdateFloat(float value)
        {
            targetTransform.localScale = new Vector3(value,value,value);
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
