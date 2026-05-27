using moe.nikky.common;
using UnityEngine;
using VRC;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace moe.nikky.kinetic_controls.control.kinetic
{
    public abstract class HandleAbstract : TexelAccessControl
    { 
        protected override bool AccessControlIsReadOnly => true;
        
        protected bool IsHeldLocally = false;

        [SerializeField]
        [ReadOnly]
        [NonReorderable]
        protected KineticControl[] controlBehaviours = { };

        [Header("Handle - Internals")]
        [Tooltip(
            "should be the same as targetIndicator or a child, " +
            "handle will be reset to the given transform position / rotation on release")]
        [SerializeField]
#if READONLY
        [ReadOnly]
#endif
        [ReadOnly]
        public Transform resetTransform;

        [SerializeField]
#if READONLY
        [ReadOnly]
#endif
        [ReadOnly]
        private Rigidbody rigidBody;

        protected override void AccessChanged()
        {
            
            // var player = Networking.LocalPlayer;
            // var isInVR = player != null && !player.IsUserInVR();
            
            if (IsInVR || IsHeldLocally)
            {
                DisableInteractive = true;
            }
            else if(IsAuthorized)
            {
                DisableInteractive = false;
                InteractionText = "Click and drag to adjust";
            }
            else
            {
                DisableInteractive = true;
                InteractionText = "";
            }
        }

        public override void Interact()
        {
            if (IsInVR || !IsAuthorized)
            {
                return;
            }

            IsHeldLocally = true;
            DisableInteractive = true;
            foreach (var baseKineticControl in controlBehaviours)
            {
                baseKineticControl.OnMoveHandle(transform.position);
            }

            _OnFollowInteract();
        }

        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            base.InputUse(value, args);
            if (IsInVR || !IsAuthorized)
            {
                return;
            }

            if (!value && IsHeldLocally)
            {
                IsHeldLocally = false;
                foreach (var baseKineticControl in controlBehaviours)
                {
                    baseKineticControl.OnDropHandle();
                    // baseKineticControl.UpdateHandlePosition();
                    if (!IsInVR)
                    {
                        baseKineticControl.DebugDesktopRaytrace(false);
                    }
                }

                ResetTransform();
                AccessChanged();
            }
        }

        // public void OnRelease()
        // {
        //     Log("OnRelease");
        //     if (!IsAuthorized)
        //         return;
        //
        //     if (!IsAuthorized)
        //     {
        //         return;
        //     }
        //
        //     // foreach (var baseKineticControl in controlBehaviours)
        //     // {
        //     //     baseKineticControl.TakeOwnership();
        //     // }
        //
        //     IsHeldLocally = false;
        //     foreach (var baseKineticControl in ControlBehaviours)
        //     {
        //         baseKineticControl.OnDropHandle();
        //         // baseKineticControl.UpdateHandlePosition();
        //         if (!IsInVR)
        //         {
        //             baseKineticControl.DebugDesktopRaytrace(false);
        //         }
        //     }
        //
        //     ResetTransform();
        //
        //     // Log("handle released, resetting position");
        //     // SendCustomNetworkEvent(NetworkEventTarget.All, nameof(UpdatePickupPosition));
        // }

        public void _OnFollowInteract()
        {
            if (!IsHeldLocally) return;
            foreach (var baseKineticControl in controlBehaviours)
            {
                baseKineticControl.FollowDesktop();
            }

            // FollowPickup();
            if (IsHeldLocally)
            {
                this.SendCustomEventDelayedFrames(nameof(_OnFollowInteract), 0);
            }
        }

        public abstract void ResetTransformIfNotManipulated();
        // {
        //     // if (!pickupHasObjectSync && !IsHeldLocally)
        //     // {
        //     //     ResetTransform();
        //     // }
        //     // ResetTransform();
        // }

        public void ResetTransform()
        {
            FreezeRigidBody();

            if (Utilities.IsValid(resetTransform))
            {
                //LogWarning($"handle reset");
                transform.SetPositionAndRotation(
                    resetTransform.position,
                    resetTransform.rotation
                );
            }
            else
            {
                LogWarning("reset transform is not valid");
            }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
            if(!Application.isPlaying)
            {
                transform.MarkDirty();
            }
#endif
        }

        private void FreezeRigidBody()
        {
            if (Utilities.IsValid(rigidBody))
            {
                if (rigidBody.isKinematic) return;
                rigidBody.velocity = Vector3.zero;
                rigidBody.maxAngularVelocity = 0;
                rigidBody.angularVelocity = Vector3.zero;
            }
            else
            {
                LogError("Rigid body is not valid");
            }
        }

        public void RegisterRuntime(KineticControl kineticControl)
        {
            LogDebug($"registering {kineticControl}");
            controlBehaviours = controlBehaviours.AddUnique(kineticControl);
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        internal virtual void Setup()
        {
            InteractionText = "Click and drag to adjust";
            FindBoolAuthDrivers();
            SetupRigidbody();
        }

        private void SetupRigidbody()
        {
            Log("SetupPickupRigidbody");
            rigidBody = GetComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            rigidBody.drag = 10f;
            rigidBody.angularDrag = 5f;
            if(!Application.isPlaying)
            {
                rigidBody.MarkDirty();
            }
        }
#endif
    }
}