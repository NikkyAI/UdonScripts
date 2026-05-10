using System.Linq;
using UdonSharp;
using VRC.Udon;

namespace moe.nikky.common.utils
{
    public class UdonsharpFinder
    {
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public static bool Find(UdonBehaviour udonBehaviour, out UdonSharpBehaviour udonSharpBehaviour)
        {
            var asset = udonBehaviour.programSource;
            if (asset is UdonSharpProgramAsset usharpAsset)
            {
                udonBehaviour.GetComponents<UdonSharpBehaviour>();
                // Log($"script id is {usharpAsset.scriptID}");
                udonSharpBehaviour = udonBehaviour
                    .GetComponents<UdonSharpBehaviour>()
                    .First(u => u.GetUdonTypeID() == usharpAsset.scriptID);
                return udonSharpBehaviour != null;
            }

            udonSharpBehaviour = null;
            return false;
        }

#endif
    }
}