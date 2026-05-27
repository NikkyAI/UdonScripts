using System;
using moe.nikky.common;
using moe.nikky.common.utils;
using moe.nikky.kinetic_controls.control.kinetic;
using Texel;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using VRC;
using VRC.SDK3.Midi;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.Editor
{
    [ExecuteAlways]
    [RequireComponent(typeof(Lever))]
    public class LeverEditorHelper : MonoBehaviour, IEditorOnly, IPreprocessCallbackBehaviour
    {
        [Header("Hover over fields for details and setup instructions")]
        // [HelpBox("Hover over fields for details and setup instructions")]

        // public Lever lever;
        [Header("Lever Settings")]
        [SerializeField]
        private Axis axis = Axis.Y;

        [FormerlySerializedAs("synced")] //
        [SerializeField]
        [Tooltip(
            "whether network sync is enabled or not, this can be 'animated' at runtime to stage changes and apply them when syncing is enabled again")]
        public bool networkSynced = true;

        [Header("Value Remapping")] //
        // [SerializeField]
        // public bool useRemapRange = false;
        [FormerlySerializedAs("outputRange")] //
        [SerializeField]
        public Vector2 remapTo = new Vector2(0, 1);

        [Header("Default Value")] //
        [SerializeField] //
        [Range(0, 1)]
        public float defaultValueNormalized = 0.25f;

        [FormerlySerializedAs("defaultValue")] //
        [SerializeField]
        public float defaultValueRemapped = 0;

        #region Handle

        [Header("Handle (required)")] //
        //[HelpBox("required for basic functionality")]
        [FormerlySerializedAs("handle")]
        [SerializeField]
        public HandleAbstract handleReference;

        [FormerlySerializedAs("handleReset")]
        [SerializeField]
        [Tooltip("has to match the position of the handle mesh inside targetIndicator\n" +
                 "the handle pickup / contact receiver will be updated to this position")]
        public Transform handlePosition;

        [FormerlySerializedAs("minRot")] //
        [Range(-180, 180)] //
        [SerializeField]
        private float minRotation = -45;

        [FormerlySerializedAs("maxRot")] //
        [Range(-180, 180)] //
        [SerializeField]
        private float maxRotation = 45;

        [Header("Handle - Desktop Support (required)")]
        [Tooltip("required for desktop support - " +
                 "this needs to be a thin, flat box collider, " +
                 "it will be used for getting where the center of the screen is facing in desktop mode, " +
                 "if it is too thick then facing it even from a slight angle will provide the wrong values, " +
                 "leading to the the controls 'jumping' upon pickup in desktop mode")]
        [SerializeField]
        public Collider desktopRaycastCollider;

        #endregion


        # region Driver Sources

        [Header("Driver Sources (required)")] //
        //[HelpBox("object containing Driver Components, will be collected at build time")]
        [FormerlySerializedAs("floatTargetValueDrivers")] //
        [SerializeField] //
        [Tooltip("object containing FloatDrivers, will update to the current target/preview value")]
        public GameObject floatTargetDriverSource;

        [FormerlySerializedAs("floatSmoothedValueDrivers")]
        [SerializeField] //
        [Tooltip("object containing FloatDrivers, will update to the current smoothed value")]
        public GameObject floatSmoothedDriverSource;

        [FormerlySerializedAs("boolAuthorizedDrivers")]
        [SerializeField] //
        [Tooltip("object containing BoolDrivers, will update to the current auth status")]
        public GameObject boolAuthorizedDriverSource;

        #endregion

        // [Header("Drivers - Readonly")] [ReadOnly]
        // public FloatDriver[] targetValueDrivers;
        //
        // [ReadOnly] public FloatDriver[] smoothedValueDrivers;
        // [ReadOnly] public BoolDriver[] authDrivers;

        #region Indicators

        [Header("Indicators (optional)")]
        //[HelpBox("transforms that will be updated to indicate state as well as range limits")]
        [SerializeField] //
        [Tooltip("will be moved to follow the handle (target value)")]
        public Transform targetIndicator;

        [FormerlySerializedAs("valueIndicator")]
        [SerializeField] //
        [Tooltip("will be moved to follow the smoothed value")]
        public Transform smoothedIndicator;

        [SerializeField] //
        [Tooltip("this transform will rotate to indicate the lower bound of the lever handle movement range\n" +
                 "will be hidden when the lever is cyclic")]
        public Transform minLimitIndicator;

        [SerializeField] //
        [Tooltip("this transform will rotate to indicate the upper bound of the lever handle movement range\n" +
                 "will be hidden when the lever is cyclic")]
        public Transform maxLimitIndicator;

        #endregion

        #region Value Smoothing

        [Header("Value Smoothing")]
        //[HelpBox("smoothes out value updates over time, may impact CPU frame times AND cause more updates sent to FloatDrivers")]
        [Tooltip(
            "smoothes out value updates over time, may impact CPU frametimes AND cause more updates to FloatDrivers")]
        [SerializeField]
        public bool enableValueSmoothing = true;

        [Tooltip("amount of frames to skip when approaching target value," +
                 "higher number == less load, but more choppy smoothing"),
         SerializeField, Range(1, 10)]
        public int smoothingUpdateInterval = 3;

        [Tooltip("higher values -> faster synchronization with the target maxSpeed \n" +
                 "(see Unity Mathf.SmoothDamp smoothTime parameter)")]
        [SerializeField]
        [Range(0f, 2.5f)]
        public float smoothingTime = 0.1f;

        [Tooltip("Maximum speed that smoothing can move at (see Unity Mathf.SmoothDamp maxSpeed parameter)")]
        [SerializeField]
        [Range(0f, 1f)]
        public float smoothingMaxSpeed = 0.25f;

        #endregion

        #region Access Control

        [Header("Access Control")] //
        //[HelpBox("You either need to assign AccessControl here or disable enforceACL, otherwise the Lever WILL NOT work")]
        [SerializeField]
        [Tooltip("this is for preview purposes in the editor only")]
        public bool authStateInEditor = true;
        
        [SerializeField]
        public bool enforceACL = true;

        [Tooltip("ACL used to check who can use the toggle")] //
        [SerializeField]
        public AccessControl accessControl;

        [Header("MIDI")] //
        [SerializeField, Tooltip("Sets up the required VRC MIDI Listener")]
        public bool addMidiListenerComponent = false;

        [SerializeField, Tooltip("Requires a VRC MIDI Listened with CC enabled")]
        public bool midiEnabled = true;

        [SerializeField, Range(0, 15)] public int midiChannel = 0;
        [SerializeField, Range(0, 127)] public int midiNumber = 0;
        [SerializeField, Range(0, 127)] public int midiInputRangeStart = 0;
        [SerializeField, Range(0, 127)] public int midiInputRangeEnd = 127;

        #endregion

        [Header("Debug")] //
        [SerializeField]
        [Tooltip("enabled and moves this transform to where the raytrace collision happens, desktop only")]
        internal Transform debugDesktopRaytrace;

        [SerializeField] public DebugLog debugLog;

        private bool _initialized = false;
        private float outputRangeMin => remapTo.x;
        private float outputRangeMax => remapTo.y;

        private float prevDefaultNormalized, prevDefault, prevMin, prevMax;

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        // private Lever GetLever()
        // {
        //     if (Utilities.IsValid(lever)) return lever;
        //     Debug.Log("getting fader component", this);
        //     lever = gameObject.GetComponent<Lever>();
        //     return lever;
        // }
        private Lever GetLever() => gameObject.GetComponent<Lever>();

        [ContextMenu("ApplyValues")]
        private void ApplyValuesContext()
        {
            ApplyValues(onValidate: false);
        }

        private void ApplyValues(bool onValidate = false, bool onPreProcess = false)
        {
            if (!onPreProcess && !_initialized)
            {
                Debug.LogWarning("not initalized yet", this);
                return;
            }

            // Debug.Log("Applying values", this);

            var l = GetLever();
            if (!Utilities.IsValid(l))
            {
                Debug.LogError("lever was not valid", this);
                return;
            }

            if (!Mathf.Approximately(prevDefaultNormalized, defaultValueNormalized))
            {
                defaultValueRemapped = Mathf.Lerp(outputRangeMin, outputRangeMax, defaultValueNormalized);
                prevDefault = defaultValueRemapped;
                prevMin = outputRangeMin;
                prevMax = outputRangeMax;
            }

            if (!Mathf.Approximately(prevDefault, defaultValueRemapped))
            {
                defaultValueNormalized = Mathf.InverseLerp(outputRangeMin, outputRangeMax, defaultValueRemapped);
                prevDefaultNormalized = defaultValueNormalized;
                prevMin = outputRangeMin;
                prevMax = outputRangeMax;
            }

            if (!Mathf.Approximately(prevMin, outputRangeMin) || !Mathf.Approximately(prevMax, outputRangeMax))
            {
                defaultValueNormalized = Mathf.InverseLerp(outputRangeMin, outputRangeMax, defaultValueRemapped);
                prevDefaultNormalized = defaultValueNormalized;
                prevMin = outputRangeMin;
                prevMax = outputRangeMax;
            }

            prevDefaultNormalized = defaultValueNormalized;
            prevDefault = defaultValueRemapped;

            l.axis = axis;
            l.outputRange = remapTo;
            l.defaultValueNormalized = defaultValueNormalized;
            l.defaultValue = defaultValueRemapped;
            l.minRot = minRotation;
            l.maxRot = maxRotation;

            var isCyclic = Mathf.Approximately(minRotation, -180f) && Mathf.Approximately(maxRotation, 180f);
            l.isCyclic = isCyclic;
            if (Utilities.IsValid(minLimitIndicator))
            {
                // minLimitIndicator.transform.
                minLimitIndicator.gameObject.SetActive(!isCyclic);
            }

            if (Utilities.IsValid(maxLimitIndicator))
            {
                maxLimitIndicator.gameObject.SetActive(!isCyclic);
            }

            l.enableValueSmoothing = enableValueSmoothing;
            l.smoothingUpdateInterval = smoothingUpdateInterval;
            l.smoothingTime = smoothingTime;
            l.smoothingMaxSpeed = smoothingMaxSpeed;

            l.synced = networkSynced;

            l.desktopRaycastCollider = desktopRaycastCollider;

            if (Utilities.IsValid(minLimitIndicator))
                l.minLimitIndicator = minLimitIndicator;
            if (Utilities.IsValid(maxLimitIndicator))
                l.maxLimitIndicator = maxLimitIndicator;
            if (Utilities.IsValid(targetIndicator))
                l.targetIndicator = targetIndicator;
            if (Utilities.IsValid(smoothedIndicator))
                l.valueIndicator = smoothedIndicator;
            l._EnsureInit();
            l.SetupLeverValuesAndComponents();
            l.UpdateIndicatorsInEditor();

            // f.SetupHandle();
            if (Utilities.IsValid(handleReference))
            {
                l.handle = handleReference;
                if (Utilities.IsValid(handlePosition))
                {
                    if (handlePosition.IsChildOf(targetIndicator))
                    {
                        handleReference.resetTransform = handlePosition;
                    }
                    else if (handlePosition == targetIndicator)
                    {
                        Debug.LogWarning($"possible mistake: 'handle position' is the same as 'target indicator' {targetIndicator}, this will break levers");
                        handleReference.resetTransform = handlePosition;
                    }
                    else
                    {
                        Debug.LogError(
                            $"'handle position' {handlePosition} must be a child of 'target indicator' {targetIndicator}",
                            this);
                        Debug.LogWarning(
                            $"handle position defaulted to {targetIndicator}, please assign it a child that matches the handle position and rotation");
                        handleReference.resetTransform = targetIndicator;
                    }
                }
                else
                {
                    Debug.LogWarning(
                        $"handle position defaulted to {targetIndicator}, please assign it a child that matches the handle position and rotation");
                    handleReference.resetTransform = targetIndicator;
                }

                // handle.UseContactsInVR = useContactsInVR;
                handleReference.AuthStateInEditor = authStateInEditor;
                handleReference.EditorACL = accessControl;
                handleReference.EditorEnforceACL = enforceACL;
                handleReference.EditorDebugLog = debugLog;
                handleReference.EditorBoolAuthorizedDrivers = boolAuthorizedDriverSource;

                handleReference._EnsureInit();
                handleReference.RegisterRuntime(l);
                handleReference.Setup();
                // handle.SetupPickupRigidbody();
                handleReference.ResetTransform();

                if (!Application.isPlaying)
                {
                    handleReference.MarkDirty();
                }
            }
            else
            {
                Debug.LogError($"missing handle in {name} editor helper", gameObject);
            }

            l.floatTargetValueDrivers = floatTargetDriverSource;
            l.floatSmoothedValueDrivers = floatSmoothedDriverSource;
            l.EditorBoolAuthorizedDrivers = boolAuthorizedDriverSource;

            l.AuthStateInEditor = authStateInEditor;
            l.EditorEnforceACL = enforceACL;
            l.EditorACL = accessControl;

            SetupMidiListener(onValidate: onValidate);
            l.midiEnabled = midiEnabled;
            l.midiChannel = midiChannel;
            l.midiNumber = midiNumber;
            l.midiInputRangeStart = midiInputRangeStart;
            l.midiInputRangeEnd = midiInputRangeEnd;

            l.debugDesktopRaytrace = debugDesktopRaytrace;
            l.EditorDebugLog = debugLog;

            var minValue = Mathf.Min(l.MinValue, l.MaxValue);
            var maxValue = Mathf.Max(l.MinValue, l.MaxValue);

            l.FindDrivers();
            // targetValueDrivers = f._targetValueFloatDrivers;
            // smoothedValueDrivers = f._smoothedValueFloatDrivers;
            l.FindBoolAuthDrivers();
            // authDrivers = f.IsAuthorizedBoolDrivers;

            foreach (var valueFloatDriver in l.smoothedValueFloatDrivers)
            {
                valueFloatDriver.EditorUpdateFloatRescale(
                    Math.Clamp(defaultValueRemapped, minValue, maxValue)
                );
            }

            foreach (var targetFloatDriver in l.targetValueFloatDrivers)
            {
                targetFloatDriver.EditorUpdateFloatRescale(
                    Math.Clamp(defaultValueRemapped, minValue, maxValue)
                );
            }


            if (!Application.isPlaying)
            {
                l.MarkDirty();
            }
        }


        protected void SetupMidiListener(bool onValidate = false)
        {
            if (addMidiListenerComponent)
            {
                var listener = GetComponent<VRCMidiListener>();
                if (listener == null)
                {
                    listener = gameObject.AddComponent<VRCMidiListener>();
                }

                if (listener != null)
                {
                    SerializedObject listenerSerialized = new SerializedObject(listener);
                    var behaviourProperty = listenerSerialized.FindProperty("behaviour");
                    var lever = GetLever();
                    if (Utilities.IsValid(lever))
                    {
                        behaviourProperty.objectReferenceValue = new SerializedObject(lever)
                            .FindProperty("_udonSharpBackingUdonBehaviour")
                            .objectReferenceValue;
                        listenerSerialized.ApplyModifiedProperties();
                    }
                }

                if (listener != null)
                {
                    listener.activeEvents = VRCMidiListener.MidiEvents.CC;
                    listener.enabled = true;
                }
            }
            else
            {
                var listeners = GetComponents<VRCMidiListener>();
                foreach (var vrcMidiListener in listeners)
                {
                    if (vrcMidiListener == null) continue;
                    // vrcMidiListener.enabled = false;
                    if (onValidate)
                    {
                        vrcMidiListener.enabled = false;
                        EditorApplication.delayCall += () => { Undo.DestroyObjectImmediate(vrcMidiListener); };
                    }
                    else if (Application.isEditor)
                    {
                        DestroyImmediate(vrcMidiListener);
                    }
                    else
                    {
                        Destroy(vrcMidiListener);
                    }
                }
            }
        }

        void OnValidate()
        {
            if (!_initialized)
            {
                CopyFromLever();
            }

            if (
                _initialized && ValidationCache.ShouldRunValidation(
                    this,
                    HashCode.Combine(
                        HashCode.Combine(
                            axis,
                            networkSynced,
                            remapTo,
                            defaultValueNormalized,
                            defaultValueRemapped
                        ),
                        HashCode.Combine(
                            minRotation,
                            maxRotation
                        ),
                        HashCode.Combine(
                            enableValueSmoothing,
                            smoothingUpdateInterval,
                            smoothingTime,
                            smoothingMaxSpeed
                        ),
                        HashCode.Combine(
                            addMidiListenerComponent,
                            midiEnabled,
                            midiChannel,
                            midiNumber,
                            midiInputRangeStart,
                            midiInputRangeEnd
                        ),
                        HashCode.Combine(
                            handleReference,
                            // handleReset,
                            minLimitIndicator,
                            maxLimitIndicator,
                            targetIndicator,
                            smoothedIndicator),
                        HashCode.Combine(
                            floatTargetDriverSource,
                            floatSmoothedDriverSource,
                            boolAuthorizedDriverSource,
                            authStateInEditor,
                            enforceACL,
                            accessControl,
                            debugLog
                        )
                    )
                )
            )
            {
                ApplyValues(onValidate: true);
            }
        }

        [ContextMenu("Setup Values from Lever")]
        internal void CopyFromLever()
        {
            if (Application.isPlaying)
            {
                Debug.Log("is playing");
                return;
            }

            var l = GetLever();
            // lever = l;
            axis = l.axis;
            remapTo = l.outputRange;
            defaultValueNormalized = l.defaultValueNormalized;
            defaultValueRemapped = l.defaultValue;
            minRotation = l.minRot;
            maxRotation = l.maxRot;

            enableValueSmoothing = l.enableValueSmoothing;
            smoothingUpdateInterval = l.smoothingUpdateInterval;
            smoothingTime = l.smoothingTime;
            smoothingMaxSpeed = l.smoothingMaxSpeed;

            networkSynced = l.synced;
            // useContactsInVR = l.handle.useContactsInVR;
            desktopRaycastCollider = l.desktopRaycastCollider;

            handleReference = l.handle;
            if (Utilities.IsValid(handleReference))
            {
                handlePosition = handleReference.resetTransform;
            }

            minLimitIndicator = l.minLimitIndicator;
            maxLimitIndicator = l.maxLimitIndicator;
            targetIndicator = l.targetIndicator;
            smoothedIndicator = l.valueIndicator;

            floatTargetDriverSource = l.floatTargetValueDrivers;
            floatSmoothedDriverSource = l.floatSmoothedValueDrivers;
            boolAuthorizedDriverSource = l.EditorBoolAuthorizedDrivers;

            authStateInEditor = l.AuthStateInEditor;
            enforceACL = l.EditorEnforceACL;
            accessControl = l.EditorACL;

            midiEnabled = l.midiEnabled;
            midiChannel = l.midiChannel;
            midiNumber = l.midiNumber;
            midiInputRangeStart = l.midiInputRangeStart;
            midiInputRangeEnd = l.midiInputRangeEnd;

            debugDesktopRaytrace = l.debugDesktopRaytrace;
            debugLog = l.EditorDebugLog;

            _initialized = true;
            if (!Application.isPlaying)
            {
                this.MarkDirty();
            }
        }

        // private void Awake()
        // {
        //     if (Application.isPlaying) return;
        //     if (!initialized)
        //     {
        //         CopyFromLever();
        //     }
        //
        //     ApplyValues(onValidate: true);
        // }
#endif

        public bool OnPreprocess()
        {
            Debug.Log($"Preprocess: is editor: {Application.isEditor}");
#if UNITY_EDITOR && !COMPILER_UDONSHARP
            ApplyValues(onPreProcess: true);
#else
            Debug.LogWarning("Preprocess: is not in editor");
#endif
            return true;
        }
       
#if UNITY_EDITOR && !COMPILER_UDONSHARP 
        [ContextMenu("Find drivers")]
        public void FindDrivers()
        {
            var l = GetLever();
            l.FindDrivers();
            l.FindBoolAuthDrivers();
        }
#endif

        public int PreprocessOrder { get; }
    }
}