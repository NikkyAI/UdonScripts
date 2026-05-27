using System.Linq;
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

        public static void Process(Scene scene)
        {
            //if (Application.isPlaying) return;
            Debug.Log($"[PreProcessHook] running preprocess on Scene {scene.path}");

            var root = scene.GetRootGameObjects();
            var components = root.SelectMany(r => r.GetComponentsInChildren<CommonBehaviour>()).ToList();
            foreach (var c in components)
            {
                Debug.Log($"[PreProcessHook] running preprocess on {c}");
                c.OnPreprocess();
            }
        }

    }
}