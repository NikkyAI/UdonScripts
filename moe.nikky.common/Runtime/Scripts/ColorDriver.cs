using UdonSharp;
using UnityEngine;

namespace moe.nikky.common
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class ColorDriver: LoggingSimple
    {
        public abstract void OnUpdateColor(Color value);

        protected Color cachedValue = Color.clear;
#if UNITY_EDITOR && !COMPILER_UDONSHARP

        public virtual void ApplyColorValue(Color value)
        {
            _EnsureInit();
            cachedValue = value;
        }
#endif
    }
}