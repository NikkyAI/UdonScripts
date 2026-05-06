using moe.nikky.common;
using UnityEngine;

namespace moe.nikky.kinetic_controls.driver.animator
{
    public class AnimatorBoolDriver : BoolDriver
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string boolParameterName;
        protected override string LogPrefix => nameof(AnimatorBoolDriver);

        public override void OnUpdateBool(bool value)
        {
            if (!enabled) return;
            animator.SetBool(boolParameterName, value);
        }
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public override void ApplyBoolValue(bool value)
        {
            base.ApplyBoolValue(value);
        }
#endif
    }
}
