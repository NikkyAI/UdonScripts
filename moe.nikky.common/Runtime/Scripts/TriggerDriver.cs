using UdonSharp;

namespace moe.nikky.common
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class TriggerDriver : CommonLogger
    {
        public abstract void OnTrigger();
    }
}