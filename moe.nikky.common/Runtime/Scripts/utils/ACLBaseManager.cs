using moe.nikky.common.Editor;
using Texel;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

// ReSharper disable ForCanBeConvertedToForeach

namespace moe.nikky.common.utils
{
#if UNITY_EDITOR && !COMPILER_UDONSHARP
    [RequireComponent(typeof(PreProcessEditorHelper))]
#endif
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ACLBaseManager : ACLBaseSimple
    {

        [SerializeField] private GameObject aclComponents;
        private ACLBase[] _aclBases = { };
    
        protected override string LogPrefix => nameof(ACLBaseManager);
    
        void Start()
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
    
        /*[NonSerialized]*/ private AccessControl prevAccessControl;
        /*[NonSerialized]*/ private bool prevEnforceACL;
    
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        protected override void OnValidate()
        {
            if (Application.isPlaying) return;
            base.OnValidate();
            // UnityEditor.EditorUtility.SetDirty(this);

            if(prevAccessControl != AccessControl
               || prevEnforceACL != EnforceACL
               // || prevDebugLog != DebugLog
              )
            {
                ApplyACLs();
                prevAccessControl = AccessControl;
                // prevDebugLog = DebugLog;
            
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        public override bool OnPreprocess()
        {
            if (!base.OnPreprocess())
            {
                return false;
            }

            ApplyACLs();
            return true;
        }

        [ContextMenu("Apply ACLs")]
        private void ApplyACLs()
        {
            if (Utilities.IsValid(aclComponents))
            {
                _aclBases = aclComponents.GetComponentsInChildren<ACLBase>();
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
