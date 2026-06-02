using UnityEngine;

namespace moe.nikky.common.Editor
{
    public abstract class CommonMonoBehaviour: MonoBehaviour
    {
        private bool init;

        public bool Initialized { get; private set; }

        public void _EnsureInit()
        {
            if (init)
                return;

            init = true;

            // stopwatch = new System.Diagnostics.Stopwatch();
            // stopwatch.Start();

            _PreInit();
            _Init();

            // stopwatch.Stop();
            // LogWarning("Initialization time: " + stopwatch.ElapsedMilliseconds + "ms");

            Initialized = true;
        }

        protected virtual void _PreInit()
        {
            if (!_logPrefixInitialized) InitLogPrefix();
        }

        protected virtual void _Init()
        {
        }
        
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public virtual void OnPreprocess()
        {
            
        }
#endif
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

        protected virtual string LogColor => new Color(0.2f, 0.5f, 05f).ToHex();

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
            if (!_pathInitialized) InitPath();
            _logPrefix = $"{_colorPrefix}{LogPrefix}{_colorPostfix} @ {_path}";
            _logPrefixInitialized = true;
        }

        [HideInCallstack]
        protected void LogError(string message)
        {
            if (!_logPrefixInitialized) InitLogPrefix();

            // var logPrefix = $"{_colorPrefix}{LogPrefix}{_colorPostfix} @ {_path}";
            Debug.LogError($"[{_logPrefix}] {message}", this);
        }

        [HideInCallstack]
        protected void LogWarning(string message)
        {
            if (!_logPrefixInitialized) InitLogPrefix();

            // var logPrefix = $"{_colorPrefix}{LogPrefix}{_colorPostfix} @ {_path}";
            Debug.LogWarning($"[{_logPrefix}] {message}", this);
        }

        [HideInCallstack]
        protected void Log(string message)
        {
            if (!_logPrefixInitialized) InitLogPrefix();

            // var logPrefix = $"{_colorPrefix}{LogPrefix}{_colorPostfix} @ {_path}";
            Debug.Log($"[{_logPrefix}] {message}", this);
        }
    }
}