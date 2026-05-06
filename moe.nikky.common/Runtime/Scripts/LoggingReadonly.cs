#define READONLY

using Texel;
using UnityEngine;

namespace moe.nikky.common
{
    public abstract class LoggingReadonly : Logging
    {
        [Header("Logging")] // header
        [ReadOnly]
        private DebugLog debugLog;

        protected override DebugLog DebugLog
        {
            get => debugLog;
            set => debugLog = value;
        }
    }
}