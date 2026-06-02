#define READONLY

using System;
using moe.nikky.common;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.control.headless
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FloatProxy : CommonLogger
    {
        [SerializeField]
        internal GameObject floatDriverSource;

        [SerializeField][ReadOnly][NonReorderable]
        public FloatDriver[] floatDrivers = Array.Empty<FloatDriver>();


        protected override string LogPrefix => nameof(FloatProxy);

        private void Start()
        {
            _EnsureInit();
        }

        //TODO: call from a proxy driver
        public void UpdateFloat(float value)
        {
            foreach (var floatDriver in floatDrivers)
            {
                floatDriver.UpdateFloatRescale(value);
            }
        }
        
        // protected override void OnUpdateFloat(float value)
        // {
        //     foreach (var floatDriver in floatDrivers)
        //     {
        //         floatDriver.UpdateFloatRescale(value);
        //     }
        // }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void FindFloatDrivers()
        {
            if (Utilities.IsValid(floatDriverSource))
            {
                floatDrivers = floatDriverSource.GetComponentsInChildren<FloatDriver>();
            }
            else
            {
                LogError("missing object for float value drivers");
            }
        }

        public override void OnPreprocess()
        {
            base.OnPreprocess();

            FindFloatDrivers();
        }

        public void EditorUpdateFloatRescale(float value)
        {
            foreach (var floatDriver in floatDrivers)
            {
                floatDriver.EditorUpdateFloatRescale(value);
            }
        }
#endif
    }
}