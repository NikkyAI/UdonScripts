using System;
using moe.nikky.common;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.control.headless
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class CyclingFloat : CommonLogger
    {
        [Header("Cycling Float")] //
        [Range(0, 1)]
        [FieldChangeCallback(nameof(Offset))]
        public float offset = 0f;

        public float Offset
        {
            get => offset;
            set
            {
                offset = value;
                UpdateCyclingValue();
            }
        }

        [FormerlySerializedAs("rate")] [Range(-1, 1)] [FieldChangeCallback(nameof(Speed))]
        public float speed = 0f;

        public float Speed
        {
            get => speed;
            set
            {
                if (Mathf.Abs(value) < minSpeed)
                {
                    value = 0f;
                }

                if (speed == 0 && value != 0)
                {
                    _zeroHandle.Kill();
                    _handle.Restart();
                }

                if (speed != 0 && value == 0)
                {
                    _handle.Pause();
                    var from = (1 + _accumulator % 1f) % 1;
                    if (from > 0.5)
                    {
                        from -= 1;
                    }
                    _zeroHandle = VRCTween.TweenFloat(
                            from: from,
                            to: 0f,
                            duration: minSpeed,
                            callback: this,
                            variableName: nameof(tweenProgress),
                            onUpdate: nameof(TweenTowardsZero),
                            easeType: VRCTweenEase.Linear
                        )
                        .SetSpeedBased()
                        .OnComplete(this, nameof(ReachedZero));
                }
                // else
                // {
                //     _dynamicOffset = tweenProgress % 100f;
                //     _handle.SetDuration(value)
                //         .Restart();
                // }


                speed = value;
                // -\frac{1}{\operatorname{abs}\left(x\cdot10\right)+1}+1
                lerpTowardValue = 1 - (1 / (Mathf.Abs(speed * 10) + 1));
                maxAccumulate = 1 + (1 / (Mathf.Abs(speed * 10) + 1));

                // if (speed == 0)
                // {
                //     _handle.Pause();
                // }
            }
        }

        private float lerpTowardValue = 0f;
        private float maxAccumulate = 1f;

        [SerializeField] [Range(0.001f, 0.1f)] [FieldChangeCallback(nameof(MinSpeed))]
        public float minSpeed = 0.01f;

        public float MinSpeed
        {
            get => minSpeed;
            set { minSpeed = value; }
        }

        // private float _targetValue = 0f;
        [FormerlySerializedAs("floatDrivers")] //
        [SerializeField]
        private GameObject floatDriverSource;

        [SerializeField] [ReadOnly] [NonReorderable]
        private FloatDriver[] _floatDrivers = { };

        private float _smoothedCurrent = 0f;

        // [Header("Smoothing")] // header
        //
        // [SerializeField]
        // [Range(0f, 5f)]
        // public float smoothTime = 0.1f;
        //
        // [Tooltip("amount of frames to skip when approaching target value," +
        //          "higher number == less load, but more choppy smoothing"),
        //  SerializeField]
        // [Range(1, 10)]
        // private int smoothingUpdateInterval = 3;
        
        [Tooltip("tween will update every x frames, this includes calling ALL float drivers" +
                 "higher number == less load, but less smooth"),
         SerializeField]
        [Range(1, 10)]
        private int tweenUpdateInterval = 3;

        protected override string LogPrefix => nameof(CyclingFloat);

        private VRCTweenHandle _handle;
        private VRCTweenHandle _zeroHandle;

        void Start()
        {
            _EnsureInit();
        }

        protected override void _Init()
        {
            base._Init();
            // _lastTime = Time.time;

            // SendCustomEventDelayedFrames(nameof(OnUpdateCyclingValue), smoothingUpdateInterval);
            SendCustomEventDelayedFrames(nameof(PostInitResetValues), 5);

            _handle = VRCTween.TweenFloat(0f, 1f, 0.1f, this, nameof(tweenProgress), nameof(UpdateCyclingValue),
                        VRCTweenEase.Linear)
                    .SetLoops(-1, VRCTweenLoopType.Incremental)
                    .SetSpeedBased()
                // .OnComplete(this, nameof(OnComplete))
                ;
            if (speed != 0)
            {
                _handle.Play();
            }
        }

        private float lastFloat = 0f;
        [NonSerialized] public float tweenProgress = 0f;
        private float _dynamicOffset = 0f;
        private float _accumulator = 0f;
        private int frames = 0;

        private float _lastValue = float.NegativeInfinity;

        // private static float lerp(float a, float b, float t) => a + (b - a) * t;

        public void TweenTowardsZero()
        {
            if (frames++ % tweenUpdateInterval != 0)
            {
                return;
            }
            _accumulator = tweenProgress;

            var value = UpdateDrivers();
            if (frames++ % 5 == 0)
            {
                LogDebug($"Tween update {tweenProgress:F3} >> ({_accumulator:F3} + o:{offset:F3}) % 1 ==> {value:F3}");
            }
        }

        public void ReachedZero()
        {
            _accumulator = 0;
            _zeroHandle.Kill();
            var value = UpdateDrivers();
            LogDebug($"Tween update {tweenProgress:F3} >> ({_accumulator:F3} + o:{offset:F3}) % 1 ==> {value:F3}");
        }

        public void UpdateCyclingValue()
        {
            if (frames++ % tweenUpdateInterval != 0)
            {
                return;
            }

            var diff = tweenProgress - lastFloat;
            lastFloat = tweenProgress;
            _accumulator += diff * speed;

            var value = UpdateDrivers();
            if (frames++ % 5 == 0)
            {
                LogDebug($"Tween update {tweenProgress:F3} >> ({_accumulator:F3} + o:{offset:F3}) % 1 ==> {value:F3}");
            }
#if UNITY_EDITOR && !COMPILER_UDONSHARP
            if (!Application.isPlaying)
            {
                LogDebug($"setting float drivers: {value}");
                foreach (var driver in _floatDrivers)
                {
                    driver.EditorUpdateFloatRescale(value);
                }
            }
#endif
        }

        private float UpdateDrivers()
        {
            var value = Mathf.Repeat(_accumulator + offset, 1f);

            if (!Mathf.Approximately(_lastValue, value))
            {
                foreach (var driver in _floatDrivers)
                {
                    driver.UpdateFloatRescale(value);
                }

                _lastValue = value;
            }

            return value;
        }

        public void OnComplete()
        {
            //_accumulator += tweenProgress;
        }

        public void PostInitResetValues()
        {
            foreach (var driver in _floatDrivers)
            {
                driver.UpdateFloatRescale(0f);
            }
        }

        public void Reset()
        {
            //TODO: reset rate?
            speed = 0;
            //throw new System.NotImplementedException();
        }

        // private int _frames = 0;

        // private float _lastTime;
        // private float _velocity;
        //
        // private float _lastValue = float.NegativeInfinity;
        //
        // // private int schedulecCount = 0;
        // 
        //  public void OnUpdateCyclingValue()
        //  {
        //      SendCustomEventDelayedFrames(nameof(OnUpdateCyclingValue), smoothingUpdateInterval);
        //      UpdateCyclingValue();
        //  }
        //  //TODO: tween every x seconds and just update on callback of virtual float
        //  //  keep track of current value and normalize occasionally if possible
        //
        // // [UsedImplicitly]
        //  public void UpdateCyclingValue()
        //  {
        //      var time = Time.time;
        //      var deltaTime = time - _lastTime;
        //      _lastTime = time;
        //
        //      var target = (speed * time) + offset;
        //      float delta = Mathf.Repeat(target - _smoothedCurrent, 1f);
        //      // if (delta > 0.5f)
        //      // {
        //      //     delta -= 1f;
        //      // }
        //
        //      if (Mathf.Approximately(delta, 0f))
        //      {
        //          return;
        //      }
        //
        //      if (target > _smoothedCurrent)
        //      {
        //          target = _smoothedCurrent + delta;
        //      }
        //      else if (target < _smoothedCurrent)
        //      {
        //          target = _smoothedCurrent + delta - 1f;
        //      }
        //      else
        //      {
        //          return;
        //      }
        //
        //      if (debug)
        //      {
        //          LogDebug($"delta {delta:0.00}");
        //          LogDebug($"before {_smoothedCurrent:0.00} => {target:0.00f}");
        //      }
        //
        //      var maxSpeed = Mathf.Max(speed, minSpeed);
        //
        //      _smoothedCurrent = SmoothDamp(
        //          current: _smoothedCurrent,
        //          target: target,
        //          currentVelocity: ref _velocity,
        //          smoothTime: smoothTime,
        //          maxSpeed: maxSpeed,
        //          deltaTime: deltaTime
        //      );
        //      if (debug)
        //      {
        //          LogDebug($"velocity:  {_velocity:0.00}");
        //      }
        //
        //      var value = Mathf.Repeat(_smoothedCurrent, 1f);
        //      if (!Mathf.Approximately(_lastValue, value))
        //      {
        //          for (var i = 0; i < _floatDrivers.Length; i++)
        //          {
        //              _floatDrivers[i].UpdateFloatRescale(value);
        //          }
        //
        //          _lastValue = value;
        //      }
        //  }
        //
        //
        //  // public bool Approximately(float a, float b)
        //  // {
        //  //     return (double)Mathf.Abs(b - a) <
        //  //            (double)Mathf.Max(1E-03f * Mathf.Max(Mathf.Abs(a), Mathf.Abs(b)), Mathf.Epsilon * 8f);
        //  // }
        //
        //  public static float DeltaWrapping(float current, float target)
        //  {
        //      float delta = Mathf.Repeat(target - current, 1f);
        //      if ((double)delta > (0.5f))
        //          delta -= 1f;
        //      return delta;
        //  }
        //
        //  // copied from https://github.com/Unity-Technologies/UnityCsReference/blob/2023.1/Runtime/Export/Math/Mathf.cs#L308
        //  // because udonsharp cannot pass ref values to native code
        //  static float SmoothDamp(
        //      float current,
        //      float target,
        //      ref float currentVelocity,
        //      float smoothTime,
        //      [System.ComponentModel.DefaultValue("Mathf.Infinity")] float maxSpeed,
        //      [System.ComponentModel.DefaultValue("Time.deltaTime")] float deltaTime
        //  )
        //  {
        //      // Based on Game Programming Gems 4 Chapter 1.10
        //      smoothTime = Mathf.Max(0.0001F, smoothTime);
        //      float omega = 2F / smoothTime;
        //
        //      float x = omega * deltaTime;
        //      float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);
        //      float change = current - target;
        //      float originalTo = target;
        //
        //      // Clamp maximum speed
        //      float maxChange = maxSpeed * smoothTime;
        //      change = Mathf.Clamp(change, -maxChange, maxChange);
        //      target = current - change;
        //
        //      float temp = (currentVelocity + omega * change) * deltaTime;
        //      currentVelocity = (currentVelocity - omega * temp) * exp;
        //      float output = target + (change + temp) * exp;
        //
        //      // Prevent overshooting
        //      if (originalTo - current > 0.0F == output > originalTo)
        //      {
        //          output = originalTo;
        //          currentVelocity = (output - originalTo) / deltaTime;
        //      }
        //
        //      return output;
        //  }

        // private int validationhashCycling = 0;

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void FindFloatDrivers()
        {
            _floatDrivers = Array.Empty<FloatDriver>();
            if (Utilities.IsValid(floatDriverSource))
            {
                _floatDrivers = floatDriverSource.GetComponentsInChildren<FloatDriver>();
            }

            Log($"found {_floatDrivers.Length} float drivers");
        }

        public override bool OnPreprocess()
        {
            if (!base.OnPreprocess())
            {
                return false;
            }

            FindFloatDrivers();

            return true;
        }
#endif
    }
}