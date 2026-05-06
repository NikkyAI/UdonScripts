using System;
using moe.nikky.common;
using UnityEngine;
using VRC.SDKBase;

namespace moe.nikky.common.Editor
{
    [ExecuteAlways]
    [RequireComponent(typeof(BaseBehaviour))]
    public class PreProcessEditorHelper: MonoBehaviour, IEditorOnly, IPreprocessCallbackBehaviour
    {
        [Tooltip("This component ensures that OnPreprocess runs on other components in the same object at build time")]
        [SerializeField, ReadOnly] private bool enabled = true;
        
        public bool OnPreprocess()
        {
            Debug.Log($"Starting Preprocess on {name}", this);
            DoPreprocess();
            return true;
        }

        public void DoPreprocess()
        {
            
            var behaviours = GetComponents<BaseBehaviour>();
            foreach (var behaviour in behaviours)
            {
                behaviour.OnPreprocess();
            }
        }

        [ContextMenu("Preprocess")]
        public void TriggerManually()
        {
            Debug.Log($"Manual Preprocess on {name}", this);
            DoPreprocess();
        }

        public void Awake()
        {
            Debug.Log($"Awake Preprocess on {name}", this);
            OnPreprocess();
        }

        public int PreprocessOrder { get; }
    }
}