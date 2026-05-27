using System;
using moe.nikky.common;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.control.interact
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TriggerButton : TexelAccessControl
    {
        [Header("Trigger - MIDI - Requires VRC_MidiListener Component with NoteOn")] //
        [Tooltip("Requires a VRC MIDI Listened with NoteOn enabled")]
        [SerializeField]
        protected bool midiEnabled = true;
        [SerializeField, Range(0,15)]
        protected int midiChannel = 0;
        [SerializeField, Range(0,127)]
        protected int midiNumber = 0;
        [SerializeField, Range(0,127)]
        protected int midiMinVelocity = 127;

        [Header("Drivers")] // header
        [FormerlySerializedAs("triggerDrivers")]
        [Tooltip("default: self")]
        [SerializeField] private GameObject triggerDriverSource;

        protected override string LogPrefix => nameof(TriggerButton);
    
        [FormerlySerializedAs("triggerDriversReadonly")]
        [SerializeField] 
        [ReadOnly]
        [NonReorderable]
        private TriggerDriver[] _triggerDrivers = { };

        void Start()
        {
            _EnsureInit();   
        }

        protected override void _Init()
        {
            base._Init();
            // if (triggerDrivers == null)
            // {
            //     triggerDrivers = this.gameObject;
            // }

        }


        protected override void AccessChanged()
        {
            // Log($"AccessChanged: {IsAuthorized}");
            DisableInteractive = !IsAuthorized;
        }

        public override void Interact()
        {
            if (!IsAuthorized) return;
            Log("Trigger Interact");
            for (var i = 0; i < _triggerDrivers.Length; i++)
            {
                _triggerDrivers[i].OnTrigger();
            }
        }
        //TODO: call network event on trigger ?
        
        public override void MidiNoteOn(int channel, int number, int velocity)
        {
            if (!IsAuthorized) return;
            base.MidiNoteOn(channel, number, velocity);
            if (!midiEnabled) return;
            
            LogDebug($"MidiNoteOn({channel}, {number}, {velocity})");
            if (channel == midiChannel && number == midiNumber && velocity >= midiMinVelocity)
            {
                LogDebug("midi triggered");
                for (var i = 0; i < _triggerDrivers.Length; i++)
                {
                    _triggerDrivers[i].OnTrigger();
                }
            }
        }

        // public override void MidiNoteOff(int channel, int number, int velocity)
        // {
        //     if (!IsAuthorized) return;
        //     base.MidiNoteOff(channel, number, velocity);
        //     if (!midiEnabled) return;
        //     
        //     Log($"MidiNoteOff({channel}, {number}, {velocity})");
        // }
        
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        
        // protected override void OnValidate()
        // {
        //     base.OnValidate();
        //     if (triggerDrivers == null && Utilities.IsValid(triggerDrivers))
        //     {
        //         triggerDrivers = triggerDrivers.gameObject;
        //         this.MarkDirty();
        //     }
        // }
        
        [ContextMenu("Assign Defaults")]
        public void AssignDefaults()
        {
            if (Application.isPlaying) return;
            // UnityEditor.EditorUtility.SetDirty(this);

            var candidates = gameObject.GetComponentsInChildren<GameObject>();
            if (triggerDriverSource == null)
            {
                foreach (var candidate in candidates)
                {
                    if (candidate.name == "Trigger Drivers")
                    {
                        triggerDriverSource = candidate;
                        Log("Found and assigned Trigger Drivers");
                        UnityEditor.EditorUtility.SetDirty(this);
                        break;
                    }
                }
            }

            if (!Application.isPlaying)
            {
                this.MarkDirty();
            }
        }

        private void FindTriggerDrivers()
        {
            
            _triggerDrivers = Array.Empty<TriggerDriver>();
            if (Utilities.IsValid(triggerDriverSource))
            {
                Log($"loading tigger drivers from {triggerDriverSource}");
                _triggerDrivers = triggerDriverSource.GetComponentsInChildren<TriggerDriver>();
            }
            else
            {
                Log($"loading tigger drivers from {gameObject}");
                _triggerDrivers = gameObject.GetComponents<TriggerDriver>();
            }
            Log($"Found {_triggerDrivers.Length} trigger drivers");
        }

        public override bool OnPreprocess()
        {
            if (!base.OnPreprocess())
            {
                return false;
            }
            FindTriggerDrivers();

            return true;
        }
#endif
    }
}
