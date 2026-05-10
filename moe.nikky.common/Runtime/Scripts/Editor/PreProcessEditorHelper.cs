using UnityEngine;
using VRC.SDKBase;

namespace moe.nikky.common.Editor
{
    [ExecuteAlways]
    public class PreProcessEditorHelper : MonoBehaviour, IEditorOnly, IPreprocessCallbackBehaviour
    {
        [Tooltip("This component ensures that OnPreprocess runs on other components in the same object at build time")]
        [SerializeField]
        [ReadOnly]
        private bool enabled = true;

        // public void Awake()
        // {
        //     if (Application.isPlaying) return;
        //     
        //     Debug.Log($"Awake Preprocess on {name}", this);
        //     OnPreprocess();
        // }

        public bool OnPreprocess()
        {
            Debug.Log($"Starting Preprocess on {name}", this);
            DoPreprocess();
            return true;
        }

        public int PreprocessOrder { get; }

        private void DoPreprocess()
        {
            var behaviours = GetComponents<CommonBehaviour>();
            foreach (var behaviour in behaviours) behaviour.OnPreprocess();
        }

        [ContextMenu("Preprocess")]
        public void TriggerManually()
        {
            Debug.Log($"Manual Preprocess on {name}", this);
            DoPreprocess();
        }
    }
}