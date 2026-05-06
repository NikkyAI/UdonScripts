using moe.nikky.common;
using UnityEngine;

namespace moe.nikky.kinetic_controls.driver.animator
{
    public class AnimatorIntDriver : IntDriver
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string intParameterName;
        protected override string LogPrefix => nameof(AnimatorIntDriver);

        protected override void OnUpdateInt(int value)
        {
            if (!enabled) return;
            animator.SetInteger(intParameterName, value);
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public override void ApplyIntValue(int value)
        {
            base.ApplyIntValue(value);
            OnUpdateInt(value);
        }
#endif
    }
}
