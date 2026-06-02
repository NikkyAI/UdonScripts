using System;
using System.Linq;
using moe.nikky.common;
using moe.nikky.common.utils;
using Texel;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.control.interact
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Selector : TexelAccessControl
    {
        [Header("Selector")] // header
        [SerializeField]
        [Min(0)]
        private int defaultIndex = 0;

        [SerializeField] private bool clickOnActiveDisables = false;

        [SerializeField]
        [Min(0)]
        [Tooltip("index to select when deactivated by clickign on current active, requires clickOnActiveDisables")]
        private int disabledIndex = 0;

        [SerializeField] protected bool useRemap = false;

        [SerializeField] //
        private Vector2Int[] remapValues = { };

        private int RemapIndex(int index)
        {
            if (useRemap)
            {
                foreach (var remapValue in remapValues)
                {
                    if (remapValue.x == index)
                    {
                        return remapValue.y;
                    }
                }

                return index;
            }
            else
            {
                return index;
            }
        }

        [Header("Drivers")] // header
        [FormerlySerializedAs("intDrivers")]
        [SerializeField]
        private GameObject intDriverSource;

        [SerializeField] [ReadOnly] [NonReorderable] private IntDriver[] intDrivers = { };

        protected override string LogPrefix => nameof(Selector);

        //TODO: replace with Texel.InteractTrigger and handle ACL centrally ???

        [SerializeField] [ReadOnly] [NonReorderable] private SelectorCallback[] interactCallbacks = { };

        private BoolDriver[][] _boolDrivers = { };

        [Header("Network Sync")] // header
        [FormerlySerializedAs("synced")]
        [SerializeField]
        [UdonSynced]
        private bool networkSynced = true;

        private int cachedPrevValue = int.MinValue;
        public override bool NetworkSynced
        {
            get => networkSynced;
            set
            {
                if (!IsAuthorized) return;

                cachedPrevValue = _syncedIndex;
                TakeOwnership();
                Log($"set synced to {value}");
                networkSynced = value;
                if (Networking.IsOwner(gameObject))
                {
                    Log($"set index to {_syncedIndex} => {cachedPrevValue}");
                    _syncedIndex = cachedPrevValue;
                }
                //TODO: await the ownership transfer properly ?

                RequestSerialization();
            }
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            base.OnOwnershipTransferred(player);

            if (player == LocalPlayer && cachedPrevValue != int.MinValue)
            {
                LogDebug($"set index to {_syncedIndex} => {cachedPrevValue}");
                _syncedIndex = cachedPrevValue;
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(SyncedIndex))]
        private int _syncedIndex;

        public int SyncedIndex
        {
            get => _syncedIndex;
            set
            {
                var oldIndex = _syncedIndex;

                if (oldIndex != value)
                {
                    _syncedIndex = value;
                    var remappedValue = RemapIndex(_syncedIndex);
                    Log($"index changed {oldIndex} => {_syncedIndex} (remapped: {remappedValue}");
                    // if (remapValues.Length - 1 >= _syncedIndex)
                    // {
                    //     remappedValue = remapValues[_syncedIndex];
                    // }

                    for (var i = 0; i < intDrivers.Length; i++)
                    {
                        intDrivers[i].UpdateIntRemap(remappedValue);
                    }

                    if (_syncedIndex >= 0 && _syncedIndex < interactCallbacks.Length)
                    {
                        var newDrivers = interactCallbacks[_syncedIndex].boolDrivers;
                        if (newDrivers != null)
                        {
                            for (var i = 0; i < newDrivers.Length; i++)
                            {
                                newDrivers[i].OnUpdateBool(true);
                            }
                        }
                    }

                    if (oldIndex >= 0 && oldIndex < interactCallbacks.Length)
                    {
                        var oldDrivers = interactCallbacks[oldIndex].boolDrivers;
                        if (oldDrivers != null)
                        {
                            for (var i = 0; i < oldDrivers.Length; i++)
                            {
                                oldDrivers[i].OnUpdateBool(false);
                            }
                        }
                    }
                }
                // if (synced)
                // {
                //     Log("taking ownership and serializing");
                //     if (!Networking.IsOwner(gameObject))
                //     {
                //         Networking.SetOwner(Networking.LocalPlayer, gameObject);
                //     }
                //     RequestSerialization();
                // }
            }
        }

        public void UpdateSyncedIndex()
        {
            if (networkSynced)
            {
                LogDebug("taking ownership and serializing");
                TakeOwnership();
                RequestSerialization();
            }
        }

        private void Start()
        {
            _EnsureInit();
        }

        protected override void _Init()
        {
            base._Init();
            SetupComponents();
        }

        private void SetupComponents()
        {
            _boolDrivers = new BoolDriver[interactCallbacks.Length][];

            for (var i = 0; i < interactCallbacks.Length; i++)
            {
                var callback = interactCallbacks[i];
                // callback.selector = this;
                // callback.index = i;
                // var boolToggleDriver = callback.boolToggleDriver;
                // if (boolToggleDriver == null)
                // {
                //     boolToggleDriver = callback.gameObject;
                // }

                var boolDrivers = callback.boolDrivers;
                _boolDrivers[i] = callback.boolDrivers;
                // _boolDrivers[i] = boolToggleDriver.GetComponentsInChildren<BoolDriver>();
                // Log($"Found {boolDrivers.Length} bool drivers for selector button {i}");
                foreach (var boolDriver in boolDrivers)
                {
                    boolDriver._EnsureInit();
                    boolDriver.OnUpdateBool(i == defaultIndex);
                }
            }

            SyncedIndex = defaultIndex;
        }


        protected override void AccessChanged()
        {
            // for (var i = 0; i < interactCallbacks.Length; i++)
            // {
            //     interactCallbacks[i].OnAccessChanged(IsAuthorized);
            // }
        }

        // [NonSerialized] private int _interactIndex;
        public void _OnInteract(int index)
        {
            if (!IsAuthorized) return;

            TakeOwnership();
            Log($"interact from {index}");
            if (clickOnActiveDisables && SyncedIndex == index)
            {
                SyncedIndex = disabledIndex;
            }
            else
            {
                SyncedIndex = index;
            }

            UpdateSyncedIndex();
        }

        public override void OnDeserialization()
        {
        }

        public void Reset()
        {
            SyncedIndex = defaultIndex;
            UpdateSyncedIndex();
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        
        // private void UpdateCallbacks()
        // {
        //     foreach (var interactCallback in interactCallbacks)
        //     {
        //         interactCallback.EditorACL = EditorACL;
        //         interactCallback.AuthStateInEditor = AuthStateInEditor;
        //         interactCallback.EditorEnforceACL = EditorEnforceACL;
        //         interactCallback.EditorDebugLog = EditorDebugLog;
        //     }
        // }

        public override void OnPreprocess()
        {
            base.OnPreprocess();
            FindDrivers();
            FindCallbacks();
        }

#endif

#if UNITY_EDITOR && !COMPILER_UDONSHARP

        protected override void OnValidate()
        {
            if (Application.isPlaying) return;
            base.OnValidate();
            UnityEditor.EditorUtility.SetDirty(this);

            if(ValidationCache.ShouldRunValidation(this, HashCode.Combine(
                   AuthStateInEditor,AccessControl,EnforceACL,DebugLog,defaultIndex,remapValues)))
            {
                FindDrivers();
                FindCallbacks();
                ApplyValues();
            }
        }


        [ContextMenu("Apply Values")]
        public void ApplyValues()
        {
            FindDrivers();
            FindCallbacks();
            SetupComponents();
            foreach (var intDriver in intDrivers)
            {
                // var remappedValue = defaultIndex;
                // if (remapValues.Length - 1 >= defaultIndex)
                // {
                //     remappedValue = remapValues[defaultIndex];
                // }

                intDriver.EditorUpdateIntRescale(RemapIndex(defaultIndex));
                // intDriver.gameObject.MarkDirty();
            }

            for (var i = 0; i < _boolDrivers.Length; i++)
            {
                for (var j = 0; j < _boolDrivers[i].Length; j++)
                {
                    _boolDrivers[i][j].ApplyBoolValue(defaultIndex == i);
                    // _boolDrivers[i][j].gameObject.MarkDirty();
                }
            }
        }

        private void FindDrivers()
        {
            if (Utilities.IsValid(intDriverSource))
            {
                // Log("getting int drivers");
                intDrivers = intDriverSource.GetComponentsInChildren<IntDriver>();
            }
        }

        [ContextMenu("Update Callbacks Components")]
        private void FindCallbacks()
        {
            if (Utilities.IsValid(gameObject))
            {
                // Log("getting interact callbacks");
                interactCallbacks = gameObject.GetComponentsInChildren<SelectorCallback>();
            }

            if (Utilities.IsValid(interactCallbacks))
            {
                Log($"Found {interactCallbacks.Length} selector callbacks");
            }
            else
            {
                LogWarning("found no selector callbacks");
            }

            for (var i = 0; i < interactCallbacks.Length; i++)
            {
                var interactCallback = interactCallbacks[i];
                interactCallback.selector = this;
                interactCallback.index = i;
                interactCallback.AuthStateInEditor = AuthStateInEditor;
                interactCallback.EditorACL = AccessControl;
                interactCallback.EditorDebugLog = DebugLog;
                interactCallback.EditorEnforceACL = EnforceACL;
                if (!Application.isPlaying)
                {
                    interactCallback.MarkDirty();
                }
            }
        }
#endif
    }
}