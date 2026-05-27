using moe.nikky.common;
using moe.nikky.kinetic_controls.control.headless;
using UnityEngine;

namespace moe.nikky.kinetic_controls.driver.control.headless
{
    public class FloatCyclingRate : FloatDriver
    {
        [SerializeField] private CyclingFloat cyclingFloat;
        protected override string LogPrefix => nameof(FloatCyclingRate);
        void Start()
        {
            _EnsureInit();
        }


        protected override void OnUpdateFloat(float value)
        {
            if (!enabled) return;
        
            cyclingFloat.Speed = value;
        }
    }
}
