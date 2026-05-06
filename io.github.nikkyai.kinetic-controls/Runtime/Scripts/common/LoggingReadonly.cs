#define READONLY

using System;
using moe.nikky.kinetic_controls.attribute;
using moe.nikky.kinetic_controls.extensions;
using Texel;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.common
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