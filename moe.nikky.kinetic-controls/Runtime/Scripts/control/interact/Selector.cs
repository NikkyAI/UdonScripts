using System.Linq;
using moe.nikky.common;
using moe.nikky.common.Editor;
using Texel;
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
    public class Selector : ACLBaseSimple
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

        [SerializeField] [ReadOnly] private IntDriver[] intDrivers = { };

        protected override string LogPrefix => nameof(Selector);

        //TODO: replace with Texel.InteractTrigger and handle ACL centrally ???

        [SerializeField] [ReadOnly] private SelectorCallback[] interactCallbacks = { };

        private BoolDriver[][] _boolDrivers = { };

        [Header("Network Sync")] // header
        [FormerlySerializedAs("synced")]
        [SerializeField]
        [UdonSynced]
        private bool networkSynced = true;

        public override bool NetworkSynced
        {
            get => networkSynced;
            set
            {
                if (!IsAuthorized) return;

                var prevValue = _syncedIndex;
                TakeOwnership();
                Log($"set synced to {value}");
                networkSynced = value;
                Log($"set index to {_syncedIndex} => {prevValue}");
                _syncedIndex = prevValue;

                RequestSerialization();
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
                Log("taking ownership and serializing");
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
            for (var i = 0; i < interactCallbacks.Length; i++)
            {
                interactCallbacks[i].OnAccessChanged(IsAuthorized);
                // interactCallbacks[i].DisableInteractive = !IsAuthorized;
            }
        }

        // [NonSerialized] private int _interactIndex;
        public void _OnInteract(int index)
        {
            if (!IsAuthorized) return;

            TakeOwnership();
            Log($"interact {index}");
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

        // ReSharper disable InconsistentNaming
        /*[NonSerialized]*/
        private int prevDefault = -1;

        /*[NonSerialized]*/
        private Vector2Int[] prevRemap = { };

        /*[NonSerialized]*/
        private AccessControl prevAccessControl;

        /*[NonSerialized]*/
        private bool prevEnforceACL;

        /*[NonSerialized]*/
        private DebugLog prevDebugLog;

        /*[NonSerialized]*/
        private bool childrenInitialized = false;
        // ReSharper restore InconsistentNaming


#if UNITY_EDITOR && !COMPILER_UDONSHARP

        private void FindDrivers()
        {
            if (Utilities.IsValid(intDriverSource))
            {
                // Log("getting int drivers");
                intDrivers = intDriverSource.GetComponentsInChildren<IntDriver>();
            }
        }

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
                var callback = interactCallbacks[i];
                callback.selector = this;
                callback.index = i;
                callback.MarkDirty();
            }
        }

        public override bool OnPreprocess()
        {
            FindDrivers();
            FindCallbacks();
            return true;
        }
#endif

#if UNITY_EDITOR && !COMPILER_UDONSHARP

        protected override void OnValidate()
        {
            if (Application.isPlaying) return;
            base.OnValidate();
            UnityEditor.EditorUtility.SetDirty(this);

            if (!childrenInitialized
                || prevAccessControl != AccessControl
                || prevEnforceACL != EnforceACL
                || prevDebugLog != DebugLog
               )
            {
                ApplyACLsAndLog();
                prevAccessControl = AccessControl;
                prevDebugLog = DebugLog;
                childrenInitialized = true;
            }

            if (prevDefault != defaultIndex
                || prevRemap.SequenceEqual(remapValues)
               )
            {
                ApplyValues();
                prevDefault = defaultIndex;
                prevRemap = remapValues;
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

        [ContextMenu("Apply ACLs and Log")]
        private void ApplyACLsAndLog()
        {
            var children = gameObject.GetComponentsInChildren<SelectorCallback>(true);
            for (var index = 0; index < children.Length; index++)
            {
                var interactCallback = children[index];
                interactCallback.index = index;
                // interactCallback.EditorACL = AccessControl;
                interactCallback.EditorDebugLog = DebugLog;
                // interactCallback.EditorEnforceACL = EnforceACL;
                interactCallback.MarkDirty();
            }
        }
#endif
    }
}