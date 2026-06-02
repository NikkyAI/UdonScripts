using System;
using moe.nikky.common.utils;
using Texel;
using UnityEngine;
using UnityEngine.Serialization;
using VRC;
using VRC.SDKBase;

namespace moe.nikky.common
{

    public abstract class TexelAccessControl : CommonLogger
    {
        // [SerializeField, HideInInspector]
        // [NonSerialized]
        // public bool aclReadOnly = false;
        protected virtual bool AccessControlIsReadOnly => false;

        [Header("Access Control")] // header
        [SerializeField] //
        [ReadOnly(nameof(AccessControlIsReadOnly))]
        [Tooltip("this is for preview purposes in the editor only")]
        private bool authStateInEditor = true;

        [SerializeField] //
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

        protected bool IsAuthorized { get; private set; } = false;


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
                    // AccessChanged();
                }
            }
            else
            {
                Log("not using ACL, setting isAuthorized to true");
                IsAuthorized = true;
                // AccessChanged();
            }

            AccessChanged();
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
                foreach (var t in authorizedDrivers)
                {
                    if (Utilities.IsValid(t))
                    {
                        t.OnUpdateBool(IsAuthorized);
                    }
                }

                AccessChanged();
            }
        }

        protected abstract void AccessChanged();

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public bool FindBoolAuthDrivers()
        {
            if (Utilities.IsValid(boolAuthorizedDrivers))
            {
                // Log($"loading auth drivers");
                authorizedDrivers = boolAuthorizedDrivers.GetComponentsInChildren<BoolDriver>();
                return true;
            }
            else
            {
                // this is not a error.. just means it isn't assigned
                // LogWarning("Could not find BoolDrivers, boolAuthorizedDrivers was not valid");
                return false;
            }
            // Log($"found {AuthorizedDrivers.Length} auth bool drivers");
        }
#endif

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        // protected int ValidationHash =>
        // HashCode.Combine(base.ValidationHash, AccessControl, boolAuthorizedDriversTransform, editorIsAuthorized);
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

        protected override void OnValidate()
        {
            if (Application.isPlaying) return;
            base.OnValidate();
            if (Utilities.IsValid(boolAuthorizedDrivers))
            {
                if (ValidationCache.ShouldRunValidation(this, HashCode.Combine(authStateInEditor)))
                {
                    UpdateEditorState();
                }
            }
        }

        private void UpdateEditorState()
        {
             LogDebug($"updating auth state in editor to {authStateInEditor}");
             if (FindBoolAuthDrivers())
             {
                foreach (var authorizedDriver in authorizedDrivers)
                {
                    authorizedDriver.ApplyBoolValue(authStateInEditor);
                }
             }
        }

        // private void Awake()
        // {
        //     if (!Application.isPlaying)
        //     {
        //         Log("Awake - updating auth driver state to editor");
        //         FindBoolAuthDrivers();
        //         foreach (var authorizedDriver in authorizedDrivers)
        //         {
        //             authorizedDriver.ApplyBoolValue(editorState);
        //         }
        //     }
        // }

        public bool AuthStateInEditor
        {
            get => authStateInEditor;
            set
            {
                var changed = authStateInEditor != value;
                // Log($"Setting editorState to {value} on {name}");
                authStateInEditor = value;
                if (changed)
                {
                    UpdateEditorState();
                    this.MarkDirty();
                }
            }
        }

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
        public override void OnPreprocess()
        {
            base.OnPreprocess();
            FindBoolAuthDrivers();
            foreach (var authorizedDriver in authorizedDrivers)
            {
                authorizedDriver.ApplyBoolValue(false);
            }
        }
#endif
    }
}