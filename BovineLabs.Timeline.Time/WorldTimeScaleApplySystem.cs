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
            var defaultFixedDeltaTime = Mathf.Max(0.0001f, worldScale.DefaultFixedDeltaTime);
            var targetFixedDeltaTime = worldScale.ScaleFixedDeltaTime ? defaultFixedDeltaTime * targetScale : defaultFixedDeltaTime;
            targetFixedDeltaTime = Mathf.Max(0.0001f, targetFixedDeltaTime);

            if (Mathf.Abs(UnityEngine.Time.timeScale - targetScale) > 0.001f)
            {
                UnityEngine.Time.timeScale = targetScale;
            }

            if (Mathf.Abs(UnityEngine.Time.fixedDeltaTime - targetFixedDeltaTime) > 0.00001f)
            {
                UnityEngine.Time.fixedDeltaTime = targetFixedDeltaTime;
            }
        }

        private static class Burst
        {
            public static readonly SharedStatic<BurstTrampoline> WorldTimeScale =
                SharedStatic<BurstTrampoline>.GetOrCreate<WorldTimeScaleApplySystem, WorldTimeScale>();
        }
    }
}
