using UdonSharp;

namespace moe.nikky.kinetic_controls.common
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public abstract class TriggerDriver: LoggingSimple
    {
        public abstract void OnTrigger();
    }
}