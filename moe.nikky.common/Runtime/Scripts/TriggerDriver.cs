using UdonSharp;

namespace moe.nikky.common
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class TriggerDriver: LoggingSimple
    {
        public abstract void OnTrigger();
    }
}