using System;
using moe.nikky.common;
using UnityEngine;
using VRC;

namespace moe.nikky.kinetic_controls.driver
{
    public class BoolTransformScaleDriver : BoolDriver
    {
        [SerializeField] private Transform[] targetsOn = { };
        [SerializeField] private Transform[] targetsOff = { };
        protected override string LogPrefix => nameof(BoolTransformScaleDriver);

        private void Start()
        {
            _EnsureInit();
        }

        public override void OnUpdateBool(bool value)
        {
            if (!enabled) return;
            foreach (var obj in targetsOn)
            {
                if (obj)
                {
                    obj.localScale = value ?  Vector3.one : Vector3.zero;
                }
            }

            foreach (var obj in targetsOff)
            {
                if (obj)
                {
                    obj.localScale = !value ?  Vector3.one : Vector3.zero;
                }
            }
        }
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public override void ApplyBoolValue(bool value)
        {
            base.ApplyBoolValue(value);
            if (!Application.isPlaying)
            {
                OnUpdateBool(value);
                this.MarkDirty();
            }
        }
#endif
    }
}