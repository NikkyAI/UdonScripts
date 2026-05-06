using moe.nikky.kinetic_controls.common;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.control
{
    public abstract class BaseSyncedControl: ACLBaseReadonly
    {

        // private VRCPlayerApi _owner;
        // private VRCPlayerApi _localPLayer;
        //
        // public virtual void TakeOwnership()
        // {
        //     if (_owner != _localPLayer) // !Networking.IsOwner(gameObject)
        //     {
        //         Networking.SetOwner(Networking.LocalPlayer, gameObject);
        //     }
        // }
        //
        // public override void OnPlayerJoined(VRCPlayerApi player)
        // {
        //     base.OnPlayerJoined(player);
        //     if (player == Networking.LocalPlayer)
        //     {
        //         _localPLayer = player;
        //     }
        // }
        //
        // public override void OnOwnershipTransferred(VRCPlayerApi player)
        // {
        //     base.OnOwnershipTransferred(player);
        //     _owner = player;
        // }
    }
}