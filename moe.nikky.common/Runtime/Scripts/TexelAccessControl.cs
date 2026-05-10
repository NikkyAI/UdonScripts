using moe.nikky.common.Editor;
using Texel;
using UnityEngine;
using VRC;
using VRC.SDKBase;

namespace moe.nikky.common
{
#if UNITY_EDITOR && !COMPILER_UDONSHARP
    [RequireComponent(typeof(PreProcessEditorHelper))]
#endif
    public abstract class TexelAccessControl : CommonLogger
    {
        // [SerializeField, HideInInspector]
        // [NonSerialized]
        // public bool aclReadOnly = false;
        protected virtual bool AccessControlIsReadOnly => false;

        [Header("Access Control")] // header
        [SerializeField]
        [ReadOnly(nameof(AccessControlIsReadOnly))]
        private bool enforceACL = true;

        [Tooltip("ACL used to check who can use the toggle")] //
        [SerializeField]
        [ReadOnly(nameof(AccessControlIsReadOnly))]
        private AccessControl accessControl;

        [SerializeField] //
        [Tooltip("object containing bool drivers, drivers will be updated with current auth status")]
        [ReadOnly(nameof(AccessControlIsReadOnly))]
        private GameObject boolAuthorizedDrivers;

        [SerializeField] [ReadOnly] [NonReorderable]
        protected BoolDriver[] authorizedDrivers = { };

        protected bool IsAuthorized { get; private set; }


        protected bool EnforceACL
        {
            get => enforceACL;
            private set => enforceACL = value;
        }

        protected AccessControl AccessControl
        {
            get => accessControl;
            private set => accessControl = value;
        }

        protected override void _Init()
        {
            base._Init();

            // FindBoolAuthDrivers();

            // Log($"queueing up LateInitACL");
            SendCustomEventDelayedFrames(nameof(_PostInitACL), 1);
        }

        public void _PostInitACL()
        {
            if (EnforceACL)
            {
                if (AccessControl)
                {
                    // Log($"registering events on {AccessControl}");
                    AccessControl._Register(AccessControl.EVENT_VALIDATE, this, nameof(_TXL_ACL_OnValidate));
                    AccessControl._Register(AccessControl.EVENT_ENFORCE_UPDATE, this, nameof(_TXL_ACL_OnValidate));

                    _TXL_ACL_OnValidate();
                }
                else
                {
                    LogError($"No ACL set on {name}");
                    IsAuthorized = false;
                    AccessChanged();
                }
            }
            else
            {
                Log("not using ACL, setting isAuthorized to true");
                IsAuthorized = true;
                AccessChanged();
            }
        }

        public void _TXL_ACL_OnValidate()
        {
            var oldAuth = IsAuthorized;
            IsAuthorized = AccessControl._LocalHasAccess();
            if (IsAuthorized != oldAuth)
            {
                // TODO: move to Base class to reduce lookups
                // var localPlayer = Networking.LocalPlayer;
                // var localName = "???";
                // if (Utilities.IsValid(localPlayer))
                // {
                //     localName = localPlayer.displayName;
                // }

                Log($"setting isAuthorized to {IsAuthorized} for {LocalPlayerName}");

                Log($"updating {authorizedDrivers.Length} drivers");
                for (var i = 0; i < authorizedDrivers.Length; i++) authorizedDrivers[i].OnUpdateBool(IsAuthorized);

                AccessChanged();
            }
        }
        //
        // private VRCPlayerApi _localPlayer;
        // protected VRCPlayerApi LocalPlayer => _localPlayer;
        // private bool _isInVR;
        // protected bool IsInVR => _isInVR;
        // private string _localName = "???";
        // public override void OnPlayerJoined(VRCPlayerApi player)
        // {
        //     base.OnPlayerJoined(player);
        //     if (player == Networking.LocalPlayer)
        //     {
        //         _localPlayer = player;
        //         _localName = player.displayName;
        //         _isInVR = player.IsUserInVR();
        //     }
        // }

        protected abstract void AccessChanged();

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public void FindBoolAuthDrivers()
        {
            if (Utilities.IsValid(boolAuthorizedDrivers))
                // Log($"loading auth drivers");
                authorizedDrivers = boolAuthorizedDrivers.GetComponentsInChildren<BoolDriver>();
            // Log($"found {AuthorizedDrivers.Length} auth bool drivers");
        }
#endif

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        // protected override int ValidationHash =>
        //     HashCode.Combine(base.ValidationHash, AccessControl, boolAuthorizedDriversTransform, editorIsAuthorized);
        //
        // public override void OnValidateApplyValues()
        // {
        //     if (Application.isPlaying) return;
        //     base.OnValidateApplyValues();
        //
        //     FindBoolAuthDrivers();
        //     Log($"updating {IsAuthorizedBoolDrivers.Length} drivers");
        //     for (var i = 0; i < IsAuthorizedBoolDrivers.Length; i++)
        //     {
        //         IsAuthorizedBoolDrivers[i].UpdateBool(editorIsAuthorized);
        //     }
        //     
        // }

        // protected override void OnValidate()
        // {
        //     base.OnValidate();
        //     if (Utilities.IsValid(boolAuthorizedDriversTransform))
        //     {
        //         boolAuthorizedDrivers = boolAuthorizedDriversTransform.gameObject;
        //         this.MarkDirty();
        //     }
        // }


        public AccessControl EditorACL
        {
            get => AccessControl;
            set
            {
                if (AccessControl != value) this.MarkDirty();

                AccessControl = value;
            }
        }

        public bool EditorEnforceACL
        {
            get => EnforceACL;
            set
            {
                if (EnforceACL != value) this.MarkDirty();
                // Log($"Setting EnforceACL to {value} on {name}");
                EnforceACL = value;
            }
        }

        public GameObject EditorBoolAuthorizedDrivers
        {
            get => boolAuthorizedDrivers;
            set
            {
                if (boolAuthorizedDrivers != value) this.MarkDirty();
                // Log($"Setting BoolAuthorizedDrivers to {value} on {name}");
                boolAuthorizedDrivers = value;
            }
        }

#endif

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public override bool OnPreprocess()
        {
            if (!base.OnPreprocess()) return false;
            FindBoolAuthDrivers();

            return true;
        }
#endif
    }
}