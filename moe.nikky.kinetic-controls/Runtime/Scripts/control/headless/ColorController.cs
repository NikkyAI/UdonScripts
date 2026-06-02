using JetBrains.Annotations;
using moe.nikky.common;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.control.headless
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ColorController : CommonLogger
    {
        [Header("Color Controller")] // header
        
        [FormerlySerializedAs("colorDrivers")]
        [SerializeField] private GameObject colorDriverSource;
        [SerializeField]
        [ReadOnly]
        [NonReorderable]
        private ColorDriver[] _colorDrivers = {};
        
        // private int[] _propertyIds = { };
        protected override string LogPrefix => nameof(ColorController);
        void Start()
        {
            _EnsureInit();
        }
        
        // protected override void _Init()
        // {
        //     base._Init();
        //     FindDrivers();
        // }

        // [FieldChangeCallback(nameof(Hue))]
        public float hue = 0.5f;
        // private float Hue
        // {
        //     get => hue;
        //     set
        //     {
        //         hue = value;
        //         UpdateColor();
        //     }
        // }
        //
        // [FieldChangeCallback(nameof(Saturation))]
        public float saturation = 0.5f;
        // private float Saturation
        // {
        //     get => saturation;
        //     set
        //     {
        //         saturation = value;
        //         UpdateColor();
        //     }
        // }
        // [FieldChangeCallback(nameof(Brightness))]
        public float brightness = 0.5f;
        // private float Brightness
        // {
        //     get => brightness;
        //     set
        //     {
        //         brightness = value;
        //         UpdateColor();
        //     }
        // }
        private Color _lastColor = Color.black;
        
        [UsedImplicitly]
        public void UpdateColor()
        {
            LogDebug($"update color ({hue}, {saturation}, {brightness})");
            Color value = Color.HSVToRGB(hue, saturation, brightness);
            if (value != _lastColor)
            {
                // Log($"applying color {value} to {_colorDrivers.Length} drivers");
                for (var i = 0; i < _colorDrivers.Length; i++)
                {
                    _colorDrivers[i].OnUpdateColor(value);
                }
                _lastColor = value;
            }
        }
    
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void FindDrivers()
        {
            
            Log($"Searching for float value drivers in {colorDriverSource}");
            if (Utilities.IsValid(colorDriverSource))
            {
                _colorDrivers = colorDriverSource.GetComponents<ColorDriver>();
                Log($"found {_colorDrivers.Length} drivers for value");
            }
            else
            {
                LogError("missing object for color drivers");
            }
        }

        public override void OnPreprocess()
        {
            FindDrivers();
        }
        
        
        [ContextMenu("Apply Color")]
        public void EditorUpdateColor()
        {
            FindDrivers();
            Color value = Color.HSVToRGB(hue, saturation, brightness);
            foreach (var colorDriver in _colorDrivers)
            {
                LogDebug($"Applying color {value} to driver {colorDriver}");
                colorDriver.ApplyColorValue(value);
            }
        }
#endif
    }
    
}
