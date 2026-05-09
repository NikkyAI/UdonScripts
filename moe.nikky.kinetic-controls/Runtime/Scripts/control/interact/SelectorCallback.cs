using moe.nikky.common;
using moe.nikky.common.Editor;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.control.interact
{
#if UNITY_EDITOR && !COMPILER_UDONSHARP
    [RequireComponent(typeof(PreProcessEditorHelper))]
#endif
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SelectorCallback : LoggingSimple
    {
        [Header("Selector Callback")] //header
        [FormerlySerializedAs("boolToggleDriver")]  //
        [SerializeField]
        public GameObject boolDriverSource;

        [Header("Internals")] 
        [SerializeField] [ReadOnly] public Selector selector;
        [SerializeField] [ReadOnly] public int index = -1;

        [SerializeField, ReadOnly] internal BoolDriver[] boolDrivers = { };

        protected override string LogPrefix => nameof(SelectorCallback);

        // public const int EVENT_INTERACT = 0;
        // public const int EVENT_RELEASE = 1;
        // const int EVENT_COUNT = 2;

        // protected override int EventCount => EVENT_COUNT;

        void Start()
        {
            _EnsureInit();
        }

        private bool IsAuthorized { get; set; } = false;

        internal void OnAccessChanged(bool isAuthorized)
        {
            IsAuthorized = isAuthorized;
            DisableInteractive = !isAuthorized;
        }

        // private bool _isInteracting = false;
        public override void Interact()
        {
            // if (_isInteracting) return;
            if (!IsAuthorized) return;
            // _isInteracting = true;
            Log($"interact on {index}");
            selector._OnInteract(index);
            // _UpdateHandlers(EVENT_INTERACT, index);
        }

        // public override void InputUse(bool value, VRC.Udon.Common.UdonInputEventArgs args)
        // {
        //     if (!_isInteracting) return;
        //     if (!isAuthorized) return;
        //     if (!value)
        //     {
        //         _isInteracting = false;
        //         // _UpdateHandlers(EVENT_RELEASE, index);
        //     }
        // }
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public override bool OnPreprocess()
        {
            if (!base.OnPreprocess())
            {
                return false;
            }

            if (Utilities.IsValid(boolDriverSource))
            {
                boolDrivers = boolDriverSource.GetComponentsInChildren<BoolDriver>();
            }
            Log($"found {boolDrivers.Length} bool drivers");

            return true;
        }
#endif
    }
}