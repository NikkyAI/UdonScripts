using System.Linq;
using moe.nikky.common;
using UnityEngine;
using UnityEngine.UI;

namespace moe.nikky.kinetic_controls.driver.ui
{
    public class BoolSetInteractable : BoolDriver
    {
        [SerializeField] private GameObject[] selectablesSources = { };

        [SerializeField] [ReadOnly] [NonReorderable]
        private Selectable[] selectables = { };

        protected override string LogPrefix => nameof(BoolSetInteractable);

        void Start()
        {
            _EnsureInit();
        }

        public override void OnUpdateBool(bool value)
        {
            foreach (var selectable in selectables)
            {
                selectable.interactable = value;
            }
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public override void OnPreprocess()
        {
            base.OnPreprocess();

            selectables = selectablesSources
                .SelectMany(s => s.GetComponentsInChildren<Selectable>())
                .ToArray();
        }
#endif
    }
}