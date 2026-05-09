using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;

namespace moe.nikky.common
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class IntDriver: LoggingSimple
    {
        
        [Header("Value Remapping")]
        [FormerlySerializedAs("useRemap")]
        [SerializeField]
        protected bool enableValueRemapping = false;
        [SerializeField] //
        private Vector2Int[] remapValues = { };
        
        private int RemapIndex(int index)
        {
            if (enableValueRemapping)
            {
                foreach (var remapValue in remapValues)
                {
                    if (remapValue.x == index)
                    {
                        return remapValue.y;
                    }
                }
                return index;
            }
            else
            {
                return index;
            }
        }
        
        protected abstract void OnUpdateInt(int value);
        
        // defaults for Modern UI selector
        // ReSharper disable once InconsistentNaming
        [HideInInspector, UsedImplicitly] public int selectionId;
        [UsedImplicitly]
        public void _SelectionChanged()
        {
            OnUpdateInt(RemapIndex(selectionId));
        }

        public void UpdateIntRemap(int value)
        {
            OnUpdateInt(RemapIndex(value));
        }

        // protected int cachedValue = int.MinValue;
#if UNITY_EDITOR && !COMPILER_UDONSHARP

        // protected override int ValidationHash => HashCode.Combine(base.GetHashCode(), cachedValue);
        protected virtual bool UpdateInEditor => false;

        protected virtual void EditorUpdateIntValue(int value)
        {
            if (!UpdateInEditor) return;
            _EnsureInit();
            OnUpdateInt(value);
            PostEditorUpdate(value);
        }
        
        
        protected virtual void PostEditorUpdate(int value)
        {
            
        }
        
        
        public void EditorUpdateIntRescale(int inputValue)
        {
            if (!enabled) return;
            if (enableValueRemapping)
            {
                inputValue = RemapIndex(inputValue);
            }
            EditorUpdateIntValue(inputValue);
        }
#endif
    }
}