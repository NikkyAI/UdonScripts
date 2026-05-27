using Texel;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

// ReSharper disable ForCanBeConvertedToForeach

namespace moe.nikky.common.utils
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ACLBaseManager : TexelAccessControl
    {
        [SerializeField] private GameObject aclComponents;
        private TexelAccessControl[] _aclBases = { };

        /*[NonSerialized]*/
        private AccessControl prevAccessControl;

        /*[NonSerialized]*/
        private bool prevEnforceACL;

        protected override string LogPrefix => nameof(ACLBaseManager);

        private void Start()
        {
            _EnsureInit();
        }

        // protected override void _Init()
        // {
        //     base._Init();
        //
        //     if (Utilities.IsValid(aclComponents))
        //     {
        //         _aclBases = aclComponents.GetComponentsInChildren<ACLBase>();
        //     }
        //
        //     // if (boolAuthorizedDrivers != null)
        //     // {
        //     //     _isAuthorizedBoolDrivers = boolAuthorizedDrivers.GetComponentsInChildren<BoolDriver>();
        //     // }
        // }

        protected override void AccessChanged()
        {
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected override void OnValidate()
        {
            if (Application.isPlaying) return;
            base.OnValidate();
            // UnityEditor.EditorUtility.SetDirty(this);

            if (prevAccessControl != AccessControl
                || prevEnforceACL != EnforceACL
                // || prevDebugLog != DebugLog
               )
            {
                ApplyACLs();
                prevAccessControl = AccessControl;
                // prevDebugLog = DebugLog;

                EditorUtility.SetDirty(this);
            }
        }

        public override bool OnPreprocess()
        {
            if (!base.OnPreprocess()) return false;

            ApplyACLs();
            return true;
        }

        [ContextMenu("Apply ACLs")]
        private void ApplyACLs()
        {
            if (Utilities.IsValid(aclComponents))
            {
                _aclBases = aclComponents.GetComponentsInChildren<TexelAccessControl>();
                foreach (var aclBase in _aclBases)
                {
                    aclBase.EditorACL = AccessControl;
                    // aclBase.EditorDebugLog = DebugLog;
                    aclBase.EditorEnforceACL = EnforceACL;
                }
            }
        }
#endif
    }
}