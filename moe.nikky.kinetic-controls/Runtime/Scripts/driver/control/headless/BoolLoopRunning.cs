using moe.nikky.common;
using moe.nikky.kinetic_controls.control.headless;
using UnityEngine;

namespace moe.nikky.kinetic_controls.driver.control.headless
{
    public class BoolLoopRunning : BoolDriver
    {
        [SerializeField] private LoopTrigger loopTrigger;
        protected override string LogPrefix => nameof(BoolLoopRunning);
    
        void Start()
        {
            _EnsureInit();
        }

        public override void OnUpdateBool(bool value)
        {
            if (!enabled) return;
        
            // Log($"time running: {value}");
            loopTrigger.TimerRunning = value;
        }
    }
}
