using BovineLabs.Core.Utility;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace BovineLabs.Timeline.Time
{
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public unsafe partial struct WorldTimeScaleApplySystem : ISystem
    {
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        private static void InitializeTrampolines()
        {
            Burst.WorldTimeScale.Data = new BurstTrampoline(&ApplyWorldTimeScalePacked);
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WorldTimeScale>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var worldScale in SystemAPI.Query<RefRO<WorldTimeScale>>())
            {
                Burst.WorldTimeScale.Data.Invoke(worldScale.ValueRO);
            }
        }

        private static void ApplyWorldTimeScalePacked(void* argumentsPtr, int argumentsSize)
        {
            ref var worldScale = ref BurstTrampoline.ArgumentsFromPtr<WorldTimeScale>(argumentsPtr, argumentsSize);
            var targetScale = worldScale.IsActive ? worldScale.ActiveScale : worldScale.DefaultScale;

            if (Mathf.Abs(UnityEngine.Time.timeScale - targetScale) <= 0.001f)
            {
                return;
            }

            UnityEngine.Time.timeScale = targetScale;

            if (worldScale.ScaleFixedDeltaTime)
            {
                UnityEngine.Time.fixedDeltaTime = Mathf.Max(0.0001f, worldScale.DefaultFixedDeltaTime * targetScale);
            }
        }

        private static class Burst
        {
            public static readonly SharedStatic<BurstTrampoline> WorldTimeScale =
                SharedStatic<BurstTrampoline>.GetOrCreate<WorldTimeScaleApplySystem, WorldTimeScale>();
        }
    }
}
