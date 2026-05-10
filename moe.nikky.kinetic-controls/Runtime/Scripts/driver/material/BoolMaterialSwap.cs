using moe.nikky.common;
using UnityEngine;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.driver.material
{
    public class BoolMaterialSwap : BoolDriver
    {
        [SerializeField] private Material disabledMat;
        [SerializeField] private Material enabledMat;
        [SerializeField] private Renderer meshRenderer;
        [SerializeField] private Renderer[] meshRenderers = {};
        [SerializeField] private int materialSlot = 0;

        protected override string LogPrefix => nameof(BoolMaterialSwap);

        void Start()
        {
            _EnsureInit();
        }

        public override void OnUpdateBool(bool value)
        {
            if (!enabled) return;
            if (!Utilities.IsValid(meshRenderer))
            {
                LogWarning("meshRenderer is not valid");
                return;
            }

            //Material[] newMats = new Material[meshRenderer.sharedMaterials.Length];

            Material[] newMats = meshRenderer.sharedMaterials;
            
            if (value)
            {
                LogDebug($"setting material to enabled: {enabledMat.name}");
                newMats[materialSlot] = enabledMat;
                // meshRenderer.materials[materialSlot] = enabledMat;
            }
            else
            {
                LogDebug($"setting material to disabled: {disabledMat.name}");
                newMats[materialSlot] = disabledMat;
                // meshRenderer.materials[materialSlot] = disabledMat;
            }
            meshRenderer.sharedMaterials = newMats;
            if (Utilities.IsValid(meshRenderers))
            {
                foreach (var meshRenderer1 in meshRenderers)
                {
                    meshRenderer1.sharedMaterials = newMats;
                }
            }
        }
        
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public override void ApplyBoolValue(bool value)
        {
            base.ApplyBoolValue(value);
            OnUpdateBool(value);
        }
#endif
    }
}