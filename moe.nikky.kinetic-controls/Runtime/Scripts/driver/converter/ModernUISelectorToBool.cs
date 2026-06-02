using JetBrains.Annotations;
using moe.nikky.common;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;

namespace moe.nikky.kinetic_controls.driver.converter
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ModernUISelectorToBool : CommonLogger
    {
        [SerializeField] private Vector2Int selectedIdMatch = Vector2Int.up;
        [FormerlySerializedAs("boolDrivers")] //
        [SerializeField] private GameObject boolDriverSource;
        [SerializeField] [ReadOnly] [NonReorderable] private BoolDriver[] _boolDrivers = {};
    
        protected override string LogPrefix => nameof(ModernUISelectorToBool);

        void Start()
        {
            _EnsureInit();
        }

        protected override void _Init()
        {
            base._Init();

           
        }
        // ReSharper disable once InconsistentNaming
        [HideInInspector, UsedImplicitly] public int selectionId;
        [UsedImplicitly]
        public void _SelectionChanged()
        {
            if (!enabled) return;
            if (!Initialized)
            {
                _EnsureInit();
            }
            Log($"Selection changed: {selectionId}");
            if (selectionId == selectedIdMatch.x)
            {
                // OnUpdateBool(false);
                for (var i = 0; i < _boolDrivers.Length; i++)
                {
                    var boolDriver = _boolDrivers[i];
                    if (Utilities.IsValid(boolDriver) && boolDriver.enabled)
                    {
                        boolDriver.OnUpdateBool(false);
                    }
                }
            } else if(selectionId == selectedIdMatch.y)
            {
                // OnUpdateBool(true);
                for (var i = 0; i < _boolDrivers.Length; i++)
                {
                    var boolDriver = _boolDrivers[i];
                    if (Utilities.IsValid(boolDriver) && boolDriver.enabled)
                    {
                        boolDriver.OnUpdateBool(true);
                    }
                }
            }
        }
#if UNITY_EDITOR && !COMPILER_UDONSHARP

        private void FindDrivers()
        {
            if (Utilities.IsValid(boolDriverSource))
            {
                //TODO: implement 
                _boolDrivers = boolDriverSource.GetComponentsInChildren<BoolDriver>();
                LogDebug($"found {_boolDrivers.Length} bool drivers");
            }
        }

        public override void OnPreprocess()
        {
            base.OnPreprocess();
            FindDrivers();
        }
#endif
    }
}
