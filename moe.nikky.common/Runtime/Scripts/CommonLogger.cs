using Texel;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace moe.nikky.common
{
    // name needs to end in `Logger`
    // methods need to start with `Log`
    public abstract class CommonLogger : CommonBehaviour
    {
        protected virtual bool LoggingIsReadOnly => false;

        [Header("Logging")] // header
        [SerializeField]
        [ReadOnly(nameof(LoggingIsReadOnly))]
        private DebugLog debugLog;

        [SerializeField]
        // [ReadOnly(nameof(LoggingIsReadOnly))]
        private LogLevel logLevel = LogLevel.INFO;
        
        protected DebugLog DebugLog
        {
            get => debugLog;
            set => debugLog = value;
        }

        private string _colorPostfix = "";
        private string _colorPrefix = "";
        private bool _colorsInitialized;
        private bool _logPrefixInitialized;

        private string _path = "";
        private bool _pathInitialized;

        // [SerializeField] private LogLevel logLevel = LogLevel.INFO;

        // protected string _logPrefix;
        private string _logPrefix = "";

        protected abstract string LogPrefix { get; }

        protected virtual string LogColor => "#407070";
        protected bool LogPath => false;

        private void InitPath()
        {
            // Color _pathColor = new Color(.75f, .75f, .75f, 1f);
            var pathColor = RichTextColor.teal;

            var t = transform;
            _path = name.Color(Color.cyan);
            t = t.parent;
            while (t != null)
            {
                _path = $"{t.name.Color(pathColor)}/{_path}";
                t = t.parent;
            }

            _path = $"/{_path}";

            _pathInitialized = true;
        }

        private void InitColors()
        {
            var c = LogColor;
            // _logPrefix = LogPrefix;
            if (c != Color.white.ToHex())
            {
                _colorPrefix = $"<color={c}>";
                _colorPostfix = "</color>";
            }

            _colorsInitialized = true;
        }

        private void InitLogPrefix()
        {
            if (!_colorsInitialized) InitColors();
            if (LogPath && !_pathInitialized) InitPath();
            if (LogPath)
            {
                _logPrefix = $"{_colorPrefix}{LogPrefix}{_colorPostfix} @ {_path}";
            }
            else
            {
                _logPrefix = $"{_colorPrefix}{LogPrefix}{_colorPostfix} @ {name.Color(RichTextColor.teal)}";
            }
            _logPrefixInitialized = true;
        }

        protected override void _PreInit()
        {
            base._PreInit();

            if (!_logPrefixInitialized) InitLogPrefix();
        }

        [HideInCallstack]
        protected void LogError(string message)
        {
            if (logLevel > LogLevel.ERROR) return;
            if (!_logPrefixInitialized) InitLogPrefix();

            // var logPrefix = $"{_colorPrefix}{LogPrefix}{_colorPostfix} @ {_path}";
            Debug.LogError($"[{_logPrefix}] {message}", this);
#if UNITY_EDITOR && !COMPILER_UDONSHARP
            return;
#endif
            if (Utilities.IsValid(debugLog))
                debugLog._WriteError(
                    _logPrefix,
                    message
                );
        }

        [HideInCallstack]
        protected void LogWarning(string message)
        {
            if (logLevel > LogLevel.WARN) return;
            if (!_logPrefixInitialized) InitLogPrefix();

            // var logPrefix = $"{_colorPrefix}{LogPrefix}{_colorPostfix} @ {_path}";
            Debug.LogWarning($"[{_logPrefix}] {message}", this);
#if UNITY_EDITOR && !COMPILER_UDONSHARP
            return;
#endif
            if (Utilities.IsValid(debugLog))
                debugLog._WriteError(
                    _logPrefix,
                    message
                );
        }

        [HideInCallstack]
        protected void Log(string message)
        {
            if (logLevel > LogLevel.INFO) return;
            if (!_logPrefixInitialized) InitLogPrefix();

            // var logPrefix = $"{_colorPrefix}{LogPrefix}{_colorPostfix} @ {_path}";
            Debug.Log($"[{_logPrefix}] {message}", this);
#if UNITY_EDITOR && !COMPILER_UDONSHARP
            return;
#endif
            if (Utilities.IsValid(debugLog))
                debugLog._Write(
                    _logPrefix,
                    message
                );
        }
        [HideInCallstack]
        protected void LogDebug(string message)
        {
            if (logLevel > LogLevel.DEBUG) return;
            if (!_logPrefixInitialized) InitLogPrefix();
            
            
            // var logPrefix = $"{_colorPrefix}{LogPrefix}{_colorPostfix} @ {_path}";
            Debug.Log($"[{_logPrefix}] {message}", this);
#if UNITY_EDITOR && !COMPILER_UDONSHARP
            return;
#endif
            if (Utilities.IsValid(debugLog))
                debugLog._Write(
                    _logPrefix,
                    message
                );
        }

        [HideInCallstack]
        protected void LogAssertion(string message)
        {
            if (!_logPrefixInitialized) InitLogPrefix();
            
            // var logPrefix = $"{_colorPrefix}{LogPrefix}{_colorPostfix} @ {_path}";
            Debug.LogAssertion($"[{_logPrefix}] {message}", this);
#if UNITY_EDITOR && !COMPILER_UDONSHARP
            return;
#endif
            if (Utilities.IsValid(debugLog))
                debugLog._Write(
                    _logPrefix,
                    message
                );
        }

        /*
        protected void LogTrace(string message)
        {
            if (logLevel < LogLevel.TRACE) return;
            var logPrefix = $"[TRC] {_colorPrefix}{LogPrefix}{_colorPostfix}";
            Debug.Log($"[{logPrefix}] {message}", this);
#if UNITY_EDITOR && !COMPILER_UDONSHARP
            return;
#endif
            if (Utilities.IsValid(DebugLog))
            {
                DebugLog._Write(
                    logPrefix,
                    message
                );
            }
        }
        protected void LogDebug(string message)
        {
            if (logLevel < LogLevel.DEBUG) return;
            var logPrefix = $"[DBG] {_colorPrefix}{LogPrefix}{_colorPostfix}";
            Debug.Log($"[{logPrefix}] {message}", this);
#if UNITY_EDITOR && !COMPILER_UDONSHARP
            return;
#endif
            if (Utilities.IsValid(DebugLog))
            {
                DebugLog._Write(
                    logPrefix,
                    message
                );
            }
        }
        protected void LogInfo(string message)
        {
            if (logLevel < LogLevel.INFO) return;
            var logPrefix = $"[INF] {_colorPrefix}{LogPrefix}{_colorPostfix}";
            Debug.Log($"[{logPrefix}] {message}", this);
#if UNITY_EDITOR && !COMPILER_UDONSHARP
            return;
#endif
            if (Utilities.IsValid(DebugLog))
            {
                DebugLog._Write(
                    logPrefix,
                    message
                );
            }
        }
        */
        
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public DebugLog EditorDebugLog
        {
            get => debugLog;
            set
            {
                // if (value != null)
                // {
                //     Log($"Setting DebugLog to {value} on {name}");
                // }
                // else
                // {
                //     Log($"Setting DebugLog to null on {name}");
                // }
                if (debugLog != value) EditorUtility.SetDirty(this);

                debugLog = value;
            }
        }
#endif
    }
}