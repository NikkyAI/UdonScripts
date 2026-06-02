using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;

namespace moe.nikky.common
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class FloatDriver : CommonLogger
    {
        [Header("Value Remapping")] // header
        [FormerlySerializedAs("useRemapRange")]
        [SerializeField]
        protected bool enableValueRemapping = false;

        [SerializeField]
        [Tooltip("clamps value to input defined input/output range")]
        protected bool clampValue = false;
        
        [SerializeField]
        [ReadOnly(nameof(enableValueRemapping), true)]
        protected Vector2 remapFrom = new Vector2(0, 1);

        [SerializeField]
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        [ReadOnly(nameof(enableValueRemapping), true)]
#endif
        protected Vector2 remapTo = new Vector2(0, 1);

        protected abstract void OnUpdateFloat(float value);

        public void UpdateFloatRescale(float inputValue)
        {
            // var inputValue = inputValue;
            if (enableValueRemapping)
            {
                var normalized = Mathf.InverseLerp(remapFrom.x, remapFrom.y, inputValue);
                if(clampValue)
                {
                    normalized = Mathf.Clamp01(normalized);
                }
                inputValue = Mathf.LerpUnclamped(remapTo.x, remapTo.y, normalized);
                
            }

            OnUpdateFloat(inputValue);
        }

        // defaults for Modern UI slider
        // ReSharper disable once InconsistentNaming
        [HideInInspector, UsedImplicitly] public float sliderValue;

        [UsedImplicitly]
        public void _SliderUpdated()
        {
            var floatValue = sliderValue;
            UpdateFloatRescale(floatValue);
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected virtual bool UpdateInEditor => false;

        protected virtual void EditorUpdateFloatValue(float value)
        {
            if (!UpdateInEditor || Application.isPlaying) return;
            _EnsureInit();
            OnUpdateFloat(value);
            PostEditorUpdate(value);
        }

        protected virtual void PostEditorUpdate(float value)
        {
        }

        public void EditorUpdateFloatRescale(float inputValue)
        {
            if (!enabled) return;
            if (enableValueRemapping)
            {
                var normalized = Mathf.InverseLerp(remapFrom.x, remapFrom.y, inputValue);
                if(clampValue)
                {
                    normalized = Mathf.Clamp01(normalized);
                }
                inputValue = Mathf.LerpUnclamped(remapTo.x, remapTo.y, normalized);

            }

            EditorUpdateFloatValue(inputValue);
            PostEditorUpdate(inputValue);
        }
#endif
    }
}