using moe.nikky.common;
using moe.nikky.kinetic_controls.control.kinetic;
using UdonSharp;
using UnityEngine;
using VRC;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.driver
{
    public class FloatSetRotation : FloatDriver
    {
        [SerializeField]
        private Axis axis = Axis.X;

        [SerializeField] private Transform target;

        private Vector3 vectorAxis = Vector3.zero;
        
        void Start()
        {
            _EnsureInit();
        }

        protected override void _Init()
        {
            base._Init();
            
            vectorAxis[(int)axis] = 1;
        }

        protected override string LogPrefix => nameof(FloatSetRotation);

        protected override void OnUpdateFloat(float value)
        {
            if (!enabled) return;

            if (Utilities.IsValid(target))
            {
                var rotationVector = target.localRotation.eulerAngles;
                var clamped = Mathf.Repeat(value+180f, 360f) - 180f;
                rotationVector[(int)axis] = clamped;
                target.localRotation = Quaternion.Euler(rotationVector);
            }
            else
            {
                LogWarning("reference target is invalid");
            }
        }
        
        
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected override bool UpdateInEditor => true;
        
        protected override void PostEditorUpdate(float value)
        {
            if (!Application.isPlaying)
            {
                target.MarkDirty();
            }
        }
#endif
    }
}
