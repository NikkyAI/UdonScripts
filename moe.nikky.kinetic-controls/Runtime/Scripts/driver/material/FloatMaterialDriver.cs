using moe.nikky.common;
using UnityEngine;
using VRC;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.driver.material
{
    public class FloatMaterialDriver : FloatDriver
    {
        [SerializeField] private Material[] materials = { };
        [SerializeField] private MeshRenderer[] renderers = { };
        [SerializeField] private string[] propertyNames = { };
        private Material[] _materials = { };
        private int[] _propertyIds = { };
        // private float _lastValue = 0.0f;

        protected override string LogPrefix => nameof(FloatMaterialDriver);

        private void Start()
        {
            _EnsureInit();
        }

        protected override void _Init()
        {
            base._Init();
            
            if (Utilities.IsValid(materials))
            {
                _materials = _materials.AddRange(materials);
            }

            for (var index = 0; index < renderers.Length; index++)
            {
                var meshRenderer =  renderers[index];
                if(Utilities.IsValid(meshRenderer))
                {
                    _materials = _materials.AddRange(meshRenderer.materials);
                }
            }
            
            InitProperties();
        }

        private void InitProperties()
        {
            _propertyIds = new int[propertyNames.Length];
            for (var i = 0; i < propertyNames.Length; i++)
            {
                _propertyIds[i] = VRCShader.PropertyToID(propertyNames[i]);
                LogDebug($"property {propertyNames[i]} => {_propertyIds[i]}");
            }
        }

        protected override void OnUpdateFloat(float value)
        {
            if (!enabled) return;
            // if (_lastValue == value) return;
            // Log($"UpdateFloat {value} on {materials.Length} materials {_propertyIds.Length} properties");
            for (var i = 0; i < materials.Length; i++)
            {
                var mat = materials[i];
                for (var j = 0; j < _propertyIds.Length; j++)
                {
                    LogDebug($"Set {propertyNames[j]} to {value}");
                    mat.SetFloat(_propertyIds[j], value);
                }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
                if (!Application.isPlaying)
                {
                    mat.MarkDirty();
                }
#endif
            }

            // _lastValue = value;
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected override bool UpdateInEditor => true;
#endif
    }
}