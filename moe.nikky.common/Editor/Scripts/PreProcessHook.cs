using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace moe.nikky.common.Editor.Scripts
{
    internal static class PreProcessHook
    {
        public class UploadHook : IProcessSceneWithReport {
            public int callbackOrder => -1000;
            public void OnProcessScene(Scene scene, BuildReport report)
            {
                if (Application.isPlaying) return;
                Process(scene);
            }
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void PlayHook() {
            Process(SceneManager.GetActiveScene());
        }

        private static string logPrefix = nameof(PreProcessHook).Color(new Color(0.25f, 0.5f, 0.5f));
        public static void Process(Scene scene)
        {
            //if (Application.isPlaying) return;
            Debug.Log($"[{logPrefix}] running preprocess on Scene {scene.path}");

            var root = scene.GetRootGameObjects();
            var components = root.SelectMany(r => r.GetComponentsInChildren<CommonBehaviour>()).ToList();
            foreach (var c in components)
            {
                Debug.Log($"[{logPrefix}] running preprocess on {c.name.Color(RichTextColor.cyan)}");
                c.OnPreprocess();
            }
            
            var monoBehaviours = root.SelectMany(r => r.GetComponentsInChildren<CommonMonoBehaviour>()).ToList();
            foreach (var c in monoBehaviours)
            {
                Debug.Log($"[{logPrefix}] running preprocess on {c.name.Color(RichTextColor.cyan)} {nameof(c)}", c);
                c.OnPreprocess();
            }
        }

        [MenuItem("Tools/NikkyAI/PreProcessHook")]
        private static void RunManually()
        {
            Process(SceneManager.GetActiveScene());
        }

    }
}