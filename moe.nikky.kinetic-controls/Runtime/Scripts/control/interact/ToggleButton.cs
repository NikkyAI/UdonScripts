using JetBrains.Annotations;
using moe.nikky.common;
using moe.nikky.common.Editor;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.control.interact
{
#if UNITY_EDITOR && !COMPILER_UDONSHARP
    [RequireComponent(typeof(PreProcessEditorHelper))]
#endif
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ToggleButton : TexelAccessControl
    {
        [Header("Toggle")] // header
        [Tooltip(
             "The button will initialize into this value, toggle this for elements that should be enabled by default"),
         SerializeField]
        private bool defaultValue = false;
        [SerializeField, UdonSynced]
        private bool synced = true;

        [Header("Toggle - MIDI - Requires VRC_MidiListener Component")] //
        [SerializeField, Tooltip("Requires a VRC MIDI Listened with NoteOn enabled")]
        protected bool midiEnabled = true;
        [SerializeField, Range(0,15)]
        protected int midiChannel = 0;
        [SerializeField, Range(0,127)]
        protected int midiNumber = 0;
        [SerializeField, Range(0,127)]
        protected int midiMinVelocity = 127;

        [FormerlySerializedAs("boolStateDrivers")]
        [Header("Drivers")] // header
        [SerializeField]
        private GameObject boolStateDriverSource;

        protected override string LogPrefix => nameof(ToggleButton);

        public override bool NetworkSynced
        {
            get => synced;
            set
            {
                if (!IsAuthorized) return;

                var prevValue = _syncedState;
                TakeOwnership();
                Log($"set synced to {value}");
                synced = value;
                Log($"set value to {_syncedState} => {prevValue}");
                _syncedState = prevValue;

                RequestSerialization();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SyncedState))]
        private bool _syncedState = false;

        public bool SyncedState
        {
            get => _syncedState;
            private set
            {
                _syncedState = value;

                Log($"SyncedState set to {_syncedState}");

                for (var i = 0; i < valueBoolDrivers.Length; i++)
                {
                    valueBoolDrivers[i].OnUpdateBool(_syncedState);
                }
            }
        }

        [SerializeField]
        [ReadOnly]
        [NonReorderable]
        private BoolDriver[] valueBoolDrivers = { };

        void Start()
        {
            _EnsureInit();
        }

        protected override void _Init()
        {
            base._Init();
            LogDebug("Initializing");
            DisableInteractive = true;

            LogDebug($"setting default value {defaultValue}");
            SyncedState = defaultValue;

            OnDeserialization();
        }
        protected override void AccessChanged()
        {
            Log($"AccessChanged: {IsAuthorized}");
            DisableInteractive = !IsAuthorized;
        }

        [UsedImplicitly]
        public void SetState(bool newValue)
        {
            if (!IsAuthorized) return;

            SyncedState = newValue;

            if (synced)
            {
                TakeOwnership();
                RequestSerialization();
            }

            OnDeserialization();
        }

        public void Reset()
        {
            if (!IsAuthorized) return;
            SyncedState = defaultValue;
            if (synced)
            {
                TakeOwnership();
                RequestSerialization();
            }
            // OnDeserialization();
        }

        public override void Interact()
        {
            LogDebug("OnInteract");
            _Interact();
        }

        public void _Interact()
        {
            if (!IsAuthorized) return;

            SyncedState = !SyncedState;
            // _UpdateState();
            if (synced)
            {
                TakeOwnership();
                RequestSerialization();
            }
            // OnDeserialization();
        }


        public override void OnDeserialization()
        {
        }
        
        public override void MidiNoteOn(int channel, int number, int velocity)
        {
            if (!IsAuthorized) return;
            base.MidiNoteOn(channel, number, velocity);
            if (!midiEnabled) return;
            
            LogDebug($"MidiNoteOn({channel}, {number}, {velocity})");
            if (channel == midiChannel && number == midiNumber && velocity >= midiMinVelocity)
            {
                LogDebug("midi triggered");
                _Interact();
            }
        }
        
#if UNITY_EDITOR && !COMPILER_UDONSHARP

        // protected override void OnValidate()
        // {
        //     base.OnValidate();
        //     if (boolStateDrivers == null && Utilities.IsValid(boolStateDriversTransform))
        //     {
        //         boolStateDrivers = boolStateDriversTransform.gameObject;
        //         this.MarkDirty();
        //     }
        // }

        [ContextMenu("Assign Defaults")]
        public void AssignDefaults()
        {
            if (Application.isPlaying) return;
            // UnityEditor.EditorUtility.SetDirty(this);

            var candidates = gameObject.GetComponentsInChildren<Transform>();
            if (boolStateDriverSource == null)
            {
                foreach (var candidate in candidates)
                {
                    if (candidate.name == "Bool State Drivers")
                    {
                        boolStateDriverSource = candidate.gameObject;
                        Log("Found and assigned Bool State Drivers");
                        UnityEditor.EditorUtility.SetDirty(this);
                        break;
                    }
                }
            }

            UnityEditor.EditorUtility.SetDirty(this);
            // if (button == null)
            // {
            //     button = gameObject;
            // }
            //
            // if (buttonCollider == null)
            // {
            //     buttonCollider = button.GetComponent<Collider>();
            // }


            if (!Application.isPlaying)
            {
                this.MarkDirty();
            }
        }

        private void FindBoolDrivers()
        {
            // _valueBoolDrivers = _valueBoolDrivers.AddRange(gameObject.GetComponents<BoolDriver>());
            if (Utilities.IsValid(boolStateDriverSource))
            {
                Log("loading bool drivers");
                valueBoolDrivers = boolStateDriverSource.GetComponentsInChildren<BoolDriver>();
            }
            Log($"found {valueBoolDrivers.Length} bool drivers");
        }

        public override bool OnPreprocess()
        {
            if (!base.OnPreprocess())
            {
                return false;
            }
            FindBoolDrivers();

            return true;
        }
#endif
    }
}