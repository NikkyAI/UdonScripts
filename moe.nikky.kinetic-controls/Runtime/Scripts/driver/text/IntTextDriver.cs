using System;
using moe.nikky.common;
using moe.nikky.common.utils;
using TMPro;
using UnityEngine;
using VRC;

namespace moe.nikky.kinetic_controls.driver.text
{
    public class IntTextDriver : IntDriver
    {
        [Header("TextMeshPro")] // header
        [SerializeField]
        private TextMeshPro textMeshPro;

        [Tooltip(
            "What the slider value will be formated as.\n" +
            "- 0.0 means it will always at least show one digit with one decimal point\n" +
            "- 00 means it will fill always be formated as two digits with no decimal point\n" +
            "- P0 will format it as a percentage, number is the amount of decimals to show")]
        [SerializeField]
        private String valueDisplayFormat = "00";

        protected override string LogPrefix => nameof(IntTextDriver);

        void Start()
        {
            _EnsureInit();
        }

        protected override void _Init()
        {
            base._Init();
            
            //TODO: check if all fields are valid
            // or find the TMP component
        }

        protected override void OnUpdateInt(int value)
        {
            if(textMeshPro) {
                textMeshPro.text = value.ToString(valueDisplayFormat);
            }
        }

        private int _cachedValue = int.MinValue;
#if UNITY_EDITOR && !COMPILER_UDONSHARP

        protected override void OnValidate()
        {
            if(!Application.isPlaying) return;
            base.OnValidate();

            if (_cachedValue == int.MinValue) return;
            
            if (
                ValidationCache.ShouldRunValidation(
                    this,
                    HashCode.Combine(
                        valueDisplayFormat,
                        _cachedValue
                    )
                )
            )
            {
                OnUpdateInt(_cachedValue);
            }
        }
        protected override bool UpdateInEditor => true;

        protected override void EditorUpdateIntValue(int value)
        {
            OnUpdateInt(value);
            if (textMeshPro)
            {
                textMeshPro.MarkDirty();
            }
            _cachedValue = value;
        }
#endif
    }
}