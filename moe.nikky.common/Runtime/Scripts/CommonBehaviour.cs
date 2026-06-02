using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace moe.nikky.common
{
    public class CommonBehaviour : UdonSharpBehaviour
    {
        private bool init;

        public bool Initialized { get; private set; }

        // protected virtual void LogError(string message)
        // {
        // }
        // protected virtual void LogWarning(string message)
        // {
        // }
        // protected virtual void Log(string message)
        // {
        // }
        // protected virtual void LogAssert(string message)
        // {
        // }

        #region Network Sync

        public virtual bool NetworkSynced
        {
            get => false;
            set { }
        }

        #endregion

        // private int lastValidationHash = 0;
#if UNITY_EDITOR && !COMPILER_UDONSHARP

        protected virtual void OnValidate()
        {
            // if (Application.isPlaying) return;
            // _EnsureInit();
        }
#endif

        // [System.NonSerialized]
        // public System.Diagnostics.Stopwatch stopwatch;

        public void _EnsureInit()
        {
            if (init)
                return;

            init = true;

            // stopwatch = new System.Diagnostics.Stopwatch();
            // stopwatch.Start();

            _PreInit();
            _Init();

            // stopwatch.Stop();
            // LogWarning("Initialization time: " + stopwatch.ElapsedMilliseconds + "ms");

            Initialized = true;
        }

        protected virtual void _PreInit()
        {
            InitLocalPlayer();
            InitOwner();
        }

        protected virtual void _Init()
        {
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public virtual void OnPreprocess()
        {
        }
        
        [ContextMenu("Preprocess")]
        public void TriggerManually()
        {
            Debug.Log($"Manual Preprocess on {name.Color(RichTextColor.lightblue)}", this);
            OnPreprocess();
        }
#endif

        #region local player and ownership
        protected VRCPlayerApi LocalPlayer { get; private set; }
        
        protected string LocalPlayerName { get; private set; } = "???";
        
        protected bool IsInVR { get; private set; }


        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player == Networking.LocalPlayer)
            {
                LocalPlayer = player;
                if (Utilities.IsValid(LocalPlayer))
                {
                    // LocalPlayer = player;
                    LocalPlayerName = LocalPlayer.displayName;
                    IsInVR = LocalPlayer.IsUserInVR();
                    // _isInVR = true; // fakes being in VR during testing
                }
                else
                {
#if UNITY_EDITOR && !COMPILER_UDONSHARP
                    if (Application.isPlaying)
                    {
                        Debug.LogError($"[{name}] failed to init local player", this);
                    }
#else
                    Debug.LogError("failed to init local player", this);
#endif
                }
            }
        }

        private void InitLocalPlayer()
        {
            LocalPlayer = Networking.LocalPlayer;
            if (Utilities.IsValid(LocalPlayer))
            {
                // LocalPlayer = player;
                LocalPlayerName = LocalPlayer.displayName;
                IsInVR = LocalPlayer.IsUserInVR();
                // _isInVR = true; // fakes being in VR during testing
            }
            else
            {
#if UNITY_EDITOR && !COMPILER_UDONSHARP
                if (Application.isPlaying)
                {
                    Debug.LogError($"[{name}] failed to init local player", this);
                }
#else
                Debug.LogError("failed to init local player", this);
#endif
            }
        }

        private VRCPlayerApi _owner;
        protected VRCPlayerApi Owner => _owner;

        private void InitOwner()
        {
            _owner = Networking.GetOwner(gameObject);
        }
        
        public virtual void TakeOwnership()
        {
            // this is just to avoid the log spam for trying to take ownership of something you already own
            if (!Networking.IsOwner(gameObject))
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        // public override void OnPlayerJoined(VRCPlayerApi player)
        // {
        //     base.OnPlayerJoined(player);
        //     if (player == Networking.LocalPlayer)
        //     {
        //         LocalPlayer = player;
        //         LocalPlayerName = player.displayName;
        //         IsInVR = player.IsUserInVR();
        //         // _isInVR = true; // fakes being in VR during testing
        //     }
        // }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            base.OnOwnershipTransferred(player);
            _owner = player;
        }

        #endregion
    }
}