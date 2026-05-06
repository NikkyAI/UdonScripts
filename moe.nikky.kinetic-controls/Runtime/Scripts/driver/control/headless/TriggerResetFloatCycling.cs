using moe.nikky.kinetic_controls.common;
using moe.nikky.kinetic_controls.control.headless;
using UnityEngine;

namespace moe.nikky.kinetic_controls.driver.control.headless
{
    public class TriggerResetFloatCycling : TriggerDriver
    {
        [SerializeField] private CyclingFloat cyclingFloat;
        void Start()
        {
            _EnsureInit();
        }

        protected override string LogPrefix => nameof(TriggerResetFloatCycling);
        public override void OnTrigger()
        {
            cyclingFloat.Reset();
        }
    }
}
