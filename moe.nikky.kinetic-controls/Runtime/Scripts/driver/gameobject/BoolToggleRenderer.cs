
using System.Linq;
using moe.nikky.common;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class BoolToggleRenderer : BoolDriver
{
    [SerializeField] private GameObject[] renderers = {};
    [SerializeField]
    [ReadOnly]
    [NonReorderable]
    private MeshRenderer[] _renderers = {};
    
    protected override string LogPrefix => nameof(BoolToggleRenderer);
    void Start()
    {
        _EnsureInit();
    }

    public override void OnUpdateBool(bool value)
    {
        foreach (var meshRenderer in _renderers)
        {
            meshRenderer.enabled = value;
        }
    }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
    public override void OnPreprocess()
    {
        base.OnPreprocess();

        if (Utilities.IsValid(renderers))
        {
            _renderers = renderers.SelectMany(x => x.GetComponentsInChildren<MeshRenderer>()).ToArray();
        }
    }
#endif
}
