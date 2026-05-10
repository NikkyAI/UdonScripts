using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;

namespace moe.nikky.common
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class BoolDriver : CommonLogger
    {
        public abstract void OnUpdateBool(bool value);
        
        #region ModernUI
        // defaults for Modern UI selector
        // ReSharper disable once InconsistentNaming
        [HideInInspector] [UsedImplicitly] public int selectionId;
        
        [UsedImplicitly]
        public void _SelectionChanged()
        {
            if (selectionId == 0)
                OnUpdateBool(false);
            else if (selectionId == 1) OnUpdateBool(true);
        }
        
        #endregion
#if UNITY_EDITOR && !COMPILER_UDONSHARP

        public virtual void ApplyBoolValue(bool value)
        {
            _EnsureInit();
        }
#endif
    }
}