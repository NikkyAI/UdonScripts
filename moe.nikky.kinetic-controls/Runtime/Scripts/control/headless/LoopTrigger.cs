using System;
using moe.nikky.common;
using moe.nikky.common.Editor;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using Random = UnityEngine.Random;

namespace moe.nikky.kinetic_controls.control.headless
{
#if UNITY_EDITOR && !COMPILER_UDONSHARP
    [RequireComponent(typeof(PreProcessEditorHelper))]
#endif
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LoopTrigger : CommonLogger
    {
        protected override string LogPrefix => nameof(LoopTrigger);

        [SerializeField] [Min(5f)] private Vector2 delay = new Vector2(20.0f, 30.0f);

        [SerializeField] private bool onlyInstanceMaster = false;
        [FormerlySerializedAs("triggerDrivers")] //
        [SerializeField] private GameObject triggerDriverSource;

        [SerializeField]
        [ReadOnly]
        [NonReorderable]
        private TriggerDriver[] _triggerDrivers = { };

        private float _minDelay, _maxDelay;

        void Start()
        {
            _EnsureInit();
        }

        protected override void _Init()
        {
            if (delay.x < delay.y)
            {
                _minDelay = delay.x;
                _maxDelay = delay.y;
            }
            else
            {
                _minDelay = delay.y;
                _maxDelay = delay.x;
            }

            if (triggerDriverSource == null)
            {
                triggerDriverSource = gameObject;
            }

            // _triggerDrivers = triggerDriverSource.GetComponentsInChildren<TriggerDriver>();
            // Log($"found {_triggerDrivers.Length} trigger drivers");
        }

        private bool _timerShouldRun = false;

        private bool _timerRunning = false;
        public bool TimerRunning
        {
            get => _timerRunning;
            set
            {
                Log($"running set {_timerShouldRun} -> {value}");

                if (!_timerShouldRun && value)
                {
                    _timerShouldRun = true;

                    // start timer
                    if (!_timerRunning)
                    {
                        TriggerTimer();
                    }
                    else
                    {
                        LogWarning("Timer already running");
                    }
                }

                if (!value && _timerShouldRun)
                {
                    _timerShouldRun = false;
                }
            }
        }

        public void TriggerTimer()
        {
            LogDebug("timer triggered");
            _timerRunning = false;

            if (!onlyInstanceMaster || Networking.IsMaster)
            {
                LogDebug($"running triggers {_timerShouldRun}");
                if (_timerShouldRun)
                {
                    foreach (var triggerDriver in _triggerDrivers)
                    {
                        triggerDriver.OnTrigger();
                    }
                }
            }

            if (!_timerRunning && _timerShouldRun)
            {
                // call timer on a delay
                _timerRunning = true;
                float nextDelay = Random.Range(_minDelay, _maxDelay);
                SendCustomEventDelayedSeconds(nameof(TriggerTimer), nextDelay);
            }
            else
            {
                LogWarning("too many timers already running or smth else broke");
            }
        }

        public override void OnMasterTransferred(VRCPlayerApi newMaster)
        {
            if (Utilities.IsValid(newMaster))
            {
                Log($"New master: {newMaster.displayName}");
            }
        }
        
        
        
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void FindTriggerDrivers()
        {
            _triggerDrivers = Array.Empty<TriggerDriver>();
            if (Utilities.IsValid(triggerDriverSource))
            {
                _triggerDrivers = triggerDriverSource.GetComponentsInChildren<TriggerDriver>();
            }

            Log($"found {_triggerDrivers.Length} trigger drivers");
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