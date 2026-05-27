#define READONLY

using System;
using moe.nikky.common;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.control
{
    //TODO: rename to TweenControl ?
    public abstract class SmoothedControl : TexelAccessControl
    {
// #if UNITY_EDITOR && !COMPILER_UDONSHARP
//         public SmoothedControl()
//         {
//             aclReadOnly = true;
//         }
// #endif
        protected override bool AccessControlIsReadOnly => true;
        protected override bool LoggingIsReadOnly => true;

        protected abstract float MinPosOrRot { get; }

        protected abstract float MaxPosOrRot { get; }

        #region default value

        [Header("Base Smoothed Control")]
        [SerializeField, UdonSynced] //
#if READONLY
        [ReadOnly]
#endif
        internal bool synced = true;

        [SerializeField]
        [Tooltip("The range of values that this behaviour will send to any attached float drivers")]
#if READONLY
        [ReadOnly]
#endif
        internal Vector2 outputRange = new Vector2(0, 1);

        [SerializeField]
#if READONLY
        [ReadOnly]
#endif
        [Range(0, 1)]
        internal float defaultValueNormalized = 0.25f;

        [SerializeField]
#if READONLY
        [ReadOnly]
#endif
        internal float defaultValue = 0;

        internal float MinValue => outputRange.x;
        internal float MaxValue => outputRange.y;

        #endregion

        #region drivers

        [Header("Base Smoothed Control - Drivers")] // header
        [SerializeField]
#if READONLY
        [ReadOnly]
#endif
        internal GameObject floatTargetValueDrivers;

        [SerializeField]
#if READONLY
        [ReadOnly]
#endif
        internal GameObject floatSmoothedValueDrivers;

#if READONLY
        [ReadOnly]
        [NonReorderable]
#endif
        [SerializeField]
        public FloatDriver[] targetValueFloatDrivers = Array.Empty<FloatDriver>();

#if READONLY
        [ReadOnly]
        [NonReorderable]
#endif
        [SerializeField]
        internal FloatDriver[] smoothedValueFloatDrivers = Array.Empty<FloatDriver>();

        #endregion

        #region value smoothing

        [Header("Base Smoothed Control - Smoothing")] // header
        [Tooltip(
            "smoothes out value updates over time, may impact CPU frametimes AND cause more updates to FloatDrivers")]
        [SerializeField]
#if READONLY
        [ReadOnly]
#endif
        internal bool enableValueSmoothing = true;

        public bool ValueSmoothing
        {
            get => enableValueSmoothing;
            set => enableValueSmoothing = value;
        }

        [Tooltip("amount of frames to skip when approaching target value," +
                 "higher number == less load, but more choppy smoothing")]
        [SerializeField]
#if READONLY
        [ReadOnly]
#endif
        [Range(1, 10)]
        internal int smoothingUpdateInterval = 3;

        public int SmoothingFrames
        {
            get => smoothingUpdateInterval;
            set => smoothingUpdateInterval = value;
        }

        // [Tooltip("fraction of the distance covered within roughly 1s"),
        //  SerializeField, Min(0.05f),]
        // private float smoothingRate = 0.5f;
        //
        // public float SmoothingRate
        // {
        //     get => smoothingRate;
        //     set => smoothingRate = value;
        // }

        [Tooltip("higher values -> faster synchronization with the target maxSpeed")]
        [SerializeField]
#if READONLY
        [ReadOnly]
#endif
        [Range(0f, 2.5f)]
        public float smoothingTime = 0.1f;

        [Tooltip("Maximum speed that smoothing can move at (see Unity Mathf.SmoothDamp maxSpeed parameter)")]
        [SerializeField]
#if READONLY
        [ReadOnly]
#endif
        [Range(0f, 1f)]
        [FieldChangeCallback(nameof(SmoothingMaxSpeed))]
        public float smoothingMaxSpeed = 0.25f;

        public float SmoothingMaxSpeed
        {
            get => smoothingMaxSpeed;
            set
            {
                smoothingMaxSpeed = value;
                // _handle = _handle.SetDuration(smoothingMaxSpeed);
            }
        }

        protected float smoothingTargetNormalized;
        protected float smoothedCurrentNormalized;

        [SerializeField] [ReadOnly] internal bool isCyclic = false;

        private const float Epsilon = 0.005f;
        private bool _valueInitialized = false;
        private bool _isSmoothing = false;
        private float _lastFrameTime = 0;

        #endregion


        // IMPORTANT, DO NOT DELETE
        [UdonSynced] protected float SyncedValueNormalized;

        // IMPORTANT, DO NOT DELETE
        [UdonSynced] protected bool SyncedIsBeingManipulated;

        // protected VRCTweenHandle _handle;

        protected abstract void UpdateTargetIndicator(float clampedPosOrRotEuler);

        protected abstract void UpdateValueIndicator(float clampedPosOrRotEuler);

        // protected void InitValueSmoothing()
        // {
        //     // NOTE: maybe we can get away without it ?
        //     // smoothedCurrentNormalized = _normalizedDefault;
        //     // smoothingTargetNormalized = _normalizedDefault;
        //     enableValueSmoothing = enableValueSmoothing && smoothingUpdateInterval > 0;
        // }

        protected override void _Init()
        {
            base._Init();
            // FindDrivers();

            // _handle = VRCTween.TweenFloat(
            //     0f,
            //     0f,
            //     .1f,
            //     this,
            //     nameof(smoothedCurrentNormalized),
            //     nameof(OnTweenUpdate),
            //     VRCTweenEase.Linear
            // )
            // .OnComplete(this, nameof(OnFloatComplete))
            // .SetSpeedBased()
            // .Pause();

            defaultValueNormalized = Mathf.Clamp01(defaultValueNormalized);
            smoothedCurrentNormalized = defaultValueNormalized;
            smoothingTargetNormalized = defaultValueNormalized;
        }

        protected void _UpdateTargetValue(float normalizedTargetValue)
        {
            // Log($"update target value {normalizedTargetValue}");
            var clampedPosRotEuler = Mathf.Lerp(MinPosOrRot, MaxPosOrRot, normalizedTargetValue);
            UpdateTargetIndicator(clampedPosRotEuler);
            var floatValue = Mathf.Lerp(MinValue, MaxValue, normalizedTargetValue);
            for (var i = 0; i < targetValueFloatDrivers.Length; i++)
            {
                targetValueFloatDrivers[i].UpdateFloatRescale(floatValue);
            }

            // immediate update
            if (!enableValueSmoothing)
            {
                // for (var i = 0; i < _floatDrivers.Length; i++)
                // {
                //     _floatDrivers[i].UpdateFloat(floatValue);
                // }
                for (var i = 0; i < smoothedValueFloatDrivers.Length; i++)
                {
                    smoothedValueFloatDrivers[i].UpdateFloatRescale(floatValue);
                }

                UpdateValueIndicator(clampedPosRotEuler);

                return;
            }

            // value smoothing
            if (!_valueInitialized)
            {
                smoothingTargetNormalized = normalizedTargetValue;
                smoothedCurrentNormalized = normalizedTargetValue;
                _lastFrameTime = Time.time;
                _valueInitialized = true;
            }
            else
            {
                smoothingTargetNormalized = normalizedTargetValue;
            }

            if (!_isSmoothing)
            {
                _isSmoothing = true;
                
                this.SendCustomEventDelayedFrames(
                    nameof(_OnValueSmoothedUpdate),
                    0
                );
                
                // _handle = _handle
                //     .SetDuration(1f)
                //     .ChangeEndValue(normalizedTargetValue, true);
                // _handle.Restart();
            }
        }

        private float _velocity;

        // public void OnTweenUpdate()
        // {
        //     
        //     var floatValue = Mathf.Lerp(MinValue, MaxValue, smoothedCurrentNormalized);
        //     for (var i = 0; i < smoothedValueFloatDrivers.Length; i++)
        //     {
        //         smoothedValueFloatDrivers[i].UpdateFloatRescale(floatValue);
        //     }
        //
        //     UpdateValueIndicator(
        //         Mathf.Lerp(MinPosOrRot, MaxPosOrRot, smoothedCurrentNormalized)
        //     );
        //     
        // }
        //
        // public void OnFloatComplete()
        // {
        //     _isSmoothing = false;
        // }
        
        public void _OnValueSmoothedUpdate()
        {
            // Log($"UpdateLoop {smoothedCurrentNormalized} => {smoothingTargetNormalized}");

            var currentFrameTime = Time.time;
            var deltaTime = currentFrameTime - _lastFrameTime;
            _lastFrameTime = currentFrameTime;

            if (isCyclic)
            {
                // TODO: implement delta for 0-1 range to adjust target
                // var delta = Mathf.Repeat(smoothingTargetNormalized - smoothedCurrentNormalized, 1f);
                // if (delta > 0.5f)
                // {
                //     delta -= 1f;
                // }

                // Log($"cyclic smoothing current {smoothedCurrentNormalized}");
                // Log($"cyclic smoothing target  {smoothingTargetNormalized} + {delta}");

                // smoothedCurrentNormalized = Mathf.Lerp(
                //     smoothedCurrentNormalized + delta,
                //     smoothedCurrentNormalized,
                //     Mathf.Exp(-smoothingRate * deltaTime)
                // );
                // if (smoothedCurrentNormalized < 0f)
                // {
                //     smoothedCurrentNormalized += 1f;
                // }
                //
                // if (smoothedCurrentNormalized > 1f)
                // {
                //     smoothedCurrentNormalized -= 1f;
                // }

                var delta = Mathf.Repeat(
                    smoothingTargetNormalized - smoothedCurrentNormalized,
                    1f
                );
                if (delta > 0.5f)
                {
                    delta -= 1f;
                }

                var target = smoothedCurrentNormalized + delta;

                smoothedCurrentNormalized = SmoothDamp(
                    current: smoothedCurrentNormalized,
                    target: target,
                    currentVelocity: ref _velocity,
                    smoothTime: smoothingTime,
                    maxSpeed: smoothingMaxSpeed,
                    deltaTime: deltaTime
                );

                smoothedCurrentNormalized = Mathf.Repeat(smoothedCurrentNormalized, 1f);
            }
            else
            {
                // smoothedCurrentNormalized = Mathf.Lerp(
                //     smoothingTargetNormalized,
                //     smoothedCurrentNormalized,
                //     Mathf.Exp(-smoothingRate * deltaTime)
                // );

                smoothedCurrentNormalized = SmoothDamp(
                    current: smoothedCurrentNormalized,
                    target: smoothingTargetNormalized,
                    currentVelocity: ref _velocity,
                    smoothTime: smoothingTime,
                    maxSpeed: smoothingMaxSpeed,
                    deltaTime: deltaTime
                );
            }

            if (!SyncedIsBeingManipulated &&
                Mathf.Abs(smoothingTargetNormalized - smoothedCurrentNormalized) <= Epsilon)
            {
                smoothedCurrentNormalized = smoothingTargetNormalized;
                LogDebug($"value reached target {smoothingTargetNormalized}");
                _isSmoothing = false;
            }
            else
            {
                this.SendCustomEventDelayedFrames(
                    nameof(_OnValueSmoothedUpdate),
                    smoothingUpdateInterval
                );
            }

            var floatValue = Mathf.Lerp(MinValue, MaxValue, smoothedCurrentNormalized);
            for (var i = 0; i < smoothedValueFloatDrivers.Length; i++)
            {
                smoothedValueFloatDrivers[i].UpdateFloatRescale(floatValue);
            }

            UpdateValueIndicator(
                Mathf.Lerp(MinPosOrRot, MaxPosOrRot, smoothedCurrentNormalized)
            );
        }


        public virtual void Reset()
        {
            if (!IsAuthorized) return;
            Log("re-setting value to default");

            SetValue(defaultValueNormalized);
        }
        
        //TODO: implement SetValueImmediate that skips smoothing

        public virtual void SetValue(float normalizedValue)
        {
            if (!IsAuthorized) return;
            SyncedValueNormalized = normalizedValue;
            // should already be done in OnDeserialization?
            _UpdateTargetValue(normalizedValue);
            if (synced)
            {
                TakeOwnership();
                RequestSerialization();
            }

            OnDeserialization();
        }

        // copied from https://github.com/Unity-Technologies/UnityCsReference/blob/2023.1/Runtime/Export/Math/Mathf.cs#L308
        // because udonsharp cannot pass ref values to native code
        static float SmoothDamp(
            float current,
            float target,
            ref float currentVelocity,
            float smoothTime,
            [System.ComponentModel.DefaultValue("Mathf.Infinity")]
            float maxSpeed,
            [System.ComponentModel.DefaultValue("Time.deltaTime")]
            float deltaTime
        )
        {
            // Based on Game Programming Gems 4 Chapter 1.10
            smoothTime = Mathf.Max(0.0001F, smoothTime);
            float omega = 2F / smoothTime;

            float x = omega * deltaTime;
            float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);
            float change = current - target;
            float originalTo = target;

            // Clamp maximum speed
            float maxChange = maxSpeed * smoothTime;
            change = Mathf.Clamp(change, -maxChange, maxChange);
            target = current - change;

            float temp = (currentVelocity + omega * change) * deltaTime;
            currentVelocity = (currentVelocity - omega * temp) * exp;
            float output = target + (change + temp) * exp;

            // Prevent overshooting
            if (originalTo - current > 0.0F == output > originalTo)
            {
                output = originalTo;
                currentVelocity = (output - originalTo) / deltaTime;
            }

            return output;
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        internal void FindDrivers()
        {
            if (Utilities.IsValid(floatSmoothedValueDrivers))
            {
                smoothedValueFloatDrivers =
                    floatSmoothedValueDrivers.gameObject.GetComponentsInChildren<FloatDriver>();
                // Log($"found {_smoothedValueFloatDrivers.Length} drivers for value");
            }
            else
            {
                LogError("missing object for float value drivers");
            }

            if (Utilities.IsValid(floatTargetValueDrivers))
            {
                targetValueFloatDrivers = floatTargetValueDrivers.GetComponentsInChildren<FloatDriver>();
                // Log($"found {_targetValueFloatDrivers.Length} drivers for target");
            }
            else
            {
                LogError("missing object for float target drivers");
            }

            if (smoothedValueFloatDrivers != null)
            {
                LogDebug($"found {smoothedValueFloatDrivers.Length} drivers for value");
            }

            if (targetValueFloatDrivers != null)
            {
                LogDebug($"found {targetValueFloatDrivers.Length} drivers for target");
            }
        }

        // public override bool OnPreprocess()
        // {
        //     FindDrivers();
        //
        //     return base.OnPreprocess();
        // }

        internal virtual void UpdateIndicatorsInEditor()
        {
            UpdateValueIndicator(
                Mathf.Lerp(MinPosOrRot, MaxPosOrRot, defaultValueNormalized)
            );
            UpdateTargetIndicator(
                Mathf.Lerp(MinPosOrRot, MaxPosOrRot, defaultValueNormalized)
            );
        }
#endif
    }
}