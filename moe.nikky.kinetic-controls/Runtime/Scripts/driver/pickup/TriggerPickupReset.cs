using moe.nikky.common;
using UnityEngine;
using VRC.SDK3.Components;

namespace moe.nikky.kinetic_controls.driver.pickup
{
    public class TriggerPickupReset : TriggerDriver
    {
        [Header("Pickup Reset")]
        public VRCObjectSync objectSync;
        void Start()
        {
            _EnsureInit();
        }

        protected override string LogPrefix => nameof(TriggerPickupReset);
        public override void OnTrigger()
        {
            objectSync.Respawn();
        }
    }
}
