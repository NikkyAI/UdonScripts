using UdonSharp;
using UnityEngine;

namespace moe.nikky.common
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class ColorDriver : CommonLogger
    {
        protected Color cachedValue = Color.clear;
        public abstract void OnUpdateColor(Color value);
#if UNITY_EDITOR && !COMPILER_UDONSHARP

        public virtual void ApplyColorValue(Color value)
        {
            _EnsureInit();
            cachedValue = value;
        }
#endif
    }
}