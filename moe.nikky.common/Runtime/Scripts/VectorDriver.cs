using UdonSharp;
using UnityEngine;

namespace moe.nikky.common
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class VectorDriver : CommonLogger
    {
        protected Vector4 cachedValue = Vector4.negativeInfinity;
        // [FormerlySerializedAs("range")] // 
        // [SerializeField, InspectorName("remap range")]
        // protected Vector2 remapRange = new Vector2(0, 1);

        protected abstract void OnUpdateVector(Vector4 value);

        public void UpdateVector(Vector4 value)
        {
            // var floatValue = Mathf.LerpUnclamped(remapRange.x, remapRange.y, normalizedValue);
            OnUpdateVector(value);
        }
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public virtual void ApplyVectorValue(Vector4 value)
        {
            _EnsureInit();
            cachedValue = value;
        }
        
        protected virtual bool UpdateInEditor => false;

        protected virtual void EditorUpdateVectorValue(Vector4 value)
        {
            if (!UpdateInEditor || Application.isPlaying) return;
            _EnsureInit();
            OnUpdateVector(value);
            PostEditorUpdate(value);
        }

        protected virtual void PostEditorUpdate(Vector4 value)
        {
        }

        // public void EditorUpdateFloatRescale(float inputValue)
        // {
        //     if (!enabled) return;
        //     if (enableValueRemapping)
        //     {
        //         inputValue = Mathf.InverseLerp(remapFrom.x, remapFrom.y, inputValue);
        //         inputValue = Mathf.LerpUnclamped(remapTo.x, remapTo.y, inputValue);
        //     }
        //
        //     EditorUpdateFloatValue(inputValue);
        //     PostEditorUpdate(inputValue);
        // }
#endif
    }
}