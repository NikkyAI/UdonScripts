
using moe.nikky.common;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FloatToVector : FloatDriver
{
    protected override string LogPrefix => nameof(FloatToVector);
    void Start()
    {
        _EnsureInit();
    }

    protected override void OnUpdateFloat(float value)
    {
        
    }
}
