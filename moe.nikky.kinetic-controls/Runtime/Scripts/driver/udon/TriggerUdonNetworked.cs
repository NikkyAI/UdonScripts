using moe.nikky.common;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class TriggerUdonNetworked : TriggerDriver
{
    [SerializeField] private UdonBehaviour[] udonBehaviours = { };
    [SerializeField] private string eventName = "";
    protected override string LogPrefix => nameof(TriggerUdonNetworked);

    void Start()
    {
        _EnsureInit();
    }

    public override void OnTrigger()
    {
        Log("OnTrigger");
        NetworkCalling.SendCustomNetworkEvent(this, NetworkEventTarget.All, nameof(OnTriggerNetworked));
    }

    [NetworkCallable]
    public void OnTriggerNetworked()
    {
        Log("OnTriggerNetworked");
        foreach (var udonBehaviour in udonBehaviours)
        {
            if (Utilities.IsValid(udonBehaviour))
            {
                udonBehaviour.SendCustomEvent(eventName);
            }
        }
    }
}