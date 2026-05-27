// #define DEBUG_LOGGING

using System.Collections.Generic;
using UnityEngine;

namespace moe.nikky.common.utils
{
    public static class ValidationCache
    {
        private static readonly Dictionary<string, int> Cache = new Dictionary<string, int>();

        public static bool ShouldRunValidation(
            Component component,
            int hash
        )
        {
            var key = $"{component.GetType().Name} {component.GetInstanceID()}";
            if (!Cache.TryGetValue(key, out var oldValue))
            {
                Cache[key] = hash;
#if DEBUG_LOGGING
                Debug.Log($"[{nameof(ValidationCache)}] checking key: {key}, is a new key, should run");
#endif
                return true;
            }

            Cache[key] = hash;

            var shouldRun = oldValue != hash;
#if DEBUG_LOGGING
            Debug.Log($"[{nameof(ValidationCache)}] checking key: {key}, existing key, should run? {shouldRun}");
#endif
            return shouldRun;
        }
    }
}