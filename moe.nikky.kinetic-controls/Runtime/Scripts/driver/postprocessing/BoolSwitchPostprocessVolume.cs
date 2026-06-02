using System;
using moe.nikky.common;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.driver.postprocessing
{
    public class BoolSwitchPostprocessVolume : BoolDriver
    {
        [SerializeField] //
        private PostProcessVolume volume;

        [SerializeField, Range(0, 10)] //
        private float timeToSwitch = 2.5f;

        void Start()
        {
            _EnsureInit();

            LogDebug("init tween handle");
            _handle = VRCTween
                .TweenFloat(
                    0f,
                    1f,
                    timeToSwitch,
                    this,
                    nameof(_currentWeight),
                    nameof(OnFloatUpdate),
                    VRCTweenEase.InOutSine
                )
                .OnComplete(this, nameof(OnFloatComplete))
                // .OnRewind(this, nameof(OnFloatRewind))
                .Pause();
        }

        protected override string LogPrefix => nameof(BoolSwitchPostprocessVolume);

        private VRCTweenHandle _handle;

        public override void OnUpdateBool(bool value)
        {
            // float _startWeight = volume.weight;
            // _currentWeight = volume.weight;
            // float _startTime = Time.time;
            float _targetWeight = value ? 1 : 0;
            volume.enabled = true;
            // SendCustomEventDelayedFrames(nameof(OnUpdateFrame), 1);

            // float currentTime = Mathf.Lerp(0, timeToSwitch, volume.weight);
            // LogDebug($"Goto {currentTime}");
            // LogDebug($"current {_handle.Elapsed}");
            // LogDebug($"target {_targetWeight}");

            // _handle.Pause();
            // _handle.Goto(currentTime, false);
            _handle = _handle
                .SetDuration(timeToSwitch)
                .ChangeEndValue(_targetWeight, true);
            LogDebug("Play");
            _handle.Restart();

            // // dispose and make new handle
            // _handle.Kill();
            //
            // LogDebug("init tween handle");
            // _handle = VRCTween
            //     .TweenFloat(
            //         volume.weight,
            //         _targetWeight,
            //         timeToSwitch,
            //         this,
            //         nameof(_currentWeight),
            //         nameof(OnFloatUpdate),
            //         VRCTweenEase.InOutSine
            //     )
            //     .OnComplete(this, nameof(OnFloatComplete))
            //     ;
            // if (value)
            // {
            //     _handle = _handle.OnComplete(this, nameof(OnFloatComplete));
            // }
            // else
            // {
            //     _handle = _handle.OnComplete(this, nameof(OnFloatRewind));
            // }
            // _handle.Play();


            // _handle = _handle.ChangeEndValue(1f, true);
            // if (value)
            // {
            //     LogDebug("PlayForwards");
            //     _handle.PlayForwards();
            // }
            // else
            // {
            //     LogDebug("PlayBackwards");
            //     _handle.PlayBackwards();
            // }
        }

        [NonSerialized] public float _currentWeight = 0f;

        public void OnFloatUpdate()
        {
            LogDebug($"On Update {_currentWeight}");
            volume.weight = _currentWeight;
        }

        public void OnFloatComplete()
        {
            LogDebug("On Complete");
            if (Mathf.Approximately(volume.weight, 0f))
            {
                volume.enabled = false;
            }
            // _handle.Kill();
        }

        public void OnFloatRewind()
        {
            LogDebug("On Rewind");
            volume.enabled = false;
            // _handle.Kill();
        }

        // private float _startTime = 0f;
        // private float _startWeight = 0f;
        // private float _targetWeight = 0f;
        //
        // public void OnUpdateFrame()
        // {
        //     float currentTIme = Time.time;
        //     float deltaTime = currentTIme - _startTime;
        //     
        //     var normalized = Mathf.InverseLerp(0, timeToSwitch, deltaTime);
        //
        //     volume.weight = Mathf.Lerp(_startWeight, _targetWeight, normalized);
        //
        //     if (deltaTime < timeToSwitch)
        //     {
        //         SendCustomEventDelayedFrames(nameof(OnUpdateFrame), 1);
        //     }
        //     else
        //     {
        //         SendCustomEventDelayedFrames(nameof(OnTargetReached), 1);
        //     }
        //
        // }
        //
        // public void OnTargetReached()
        // {
        //     LogDebug("On Target Reached");
        //     if (Mathf.Approximately(volume.weight, 0f))
        //     {
        //         volume.enabled = false;
        //     }
        //
        // }
    }
}