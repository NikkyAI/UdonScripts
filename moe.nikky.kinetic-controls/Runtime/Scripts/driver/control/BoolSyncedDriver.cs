using System;
using moe.nikky.common;
using UnityEngine;
using UnityEngine.Serialization;
using VRC;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.driver.control
{

    public class BoolSyncedDriver : BoolDriver
    {
        [FormerlySerializedAs("syncedBehaviours")]
        [Header("External Behaviours")] // header
        [SerializeField]
        private GameObject syncedBehaviourSource;

        [SerializeField] [ReadOnly] [NonReorderable] private CommonBehaviour[] baseBehaviours;

        protected override string LogPrefix => nameof(BoolSyncedDriver);

        public override void OnUpdateBool(bool value)
        {
            if (!enabled) return;

            foreach (var behaviour in baseBehaviours)
            {
                if (Utilities.IsValid(behaviour))
                {
                    behaviour.NetworkSynced = value;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
                    if (!Application.isPlaying)
                    {
                        behaviour.MarkDirty();
                    }
#endif
                }
            }
        }


#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public override void ApplyBoolValue(bool value)
        {
            base.ApplyBoolValue(value);
            OnUpdateBool(value);
        }


        private void FindBaseBehaviours()
        {
            // _valueBoolDrivers = _valueBoolDrivers.AddRange(gameObject.GetComponents<BoolDriver>());
            baseBehaviours = Array.Empty<CommonBehaviour>();
            if (Utilities.IsValid(syncedBehaviourSource))
            {
                baseBehaviours = syncedBehaviourSource.GetComponentsInChildren<CommonBehaviour>();
            }
            
            Log($"found {baseBehaviours.Length} base behaviours");
        }

        public override bool OnPreprocess()
        {
            if (!base.OnPreprocess())
            {
                return false;
            }

            FindBaseBehaviours();

            return true;
        }
#endif
    }
}