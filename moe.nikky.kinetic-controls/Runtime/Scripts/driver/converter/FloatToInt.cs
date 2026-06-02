using moe.nikky.common;
using UnityEngine;

namespace moe.nikky.kinetic_controls.driver.converter
{
    public class FloatToInt : FloatDriver
    {
        void Start()
        {
            _EnsureInit();
        }
        [SerializeField] private GameObject intDrivers;
        private IntDriver[] _intDrivers = {};
    
        protected override string LogPrefix => nameof(FloatToInt);
    
        protected override void _Init()
        {
            base._Init();
            _intDrivers = intDrivers.GetComponentsInChildren<IntDriver>();
        }

        private int _value = int.MinValue;
        protected override void OnUpdateFloat(float value)
        {
            var oldValue = _value;
            _value = Mathf.RoundToInt(value);
            if (oldValue != _value)
            {
                for (var i = 0; i < _intDrivers.Length; i++)
                {
                    _intDrivers[i].UpdateIntRemap(_value);
                }
            }
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected override bool UpdateInEditor => true;
        // protected override void EditorUpdateFloatValue(float value)
        // {
        //     _intDrivers = intDrivers.GetComponentsInChildren<IntDriver>();
        //     base.EditorUpdateFloatValue(value);
        // }
#endif
    }
}
