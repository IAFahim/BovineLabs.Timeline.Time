using BovineLabs.Core.Utility;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BovineLabs.Timeline.Time
{
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation | WorldSystemFilterFlags.ClientSimulation |
                       WorldSystemFilterFlags.ServerSimulation)]
    public unsafe partial struct WorldTimeScaleApplySystem : ISystem
    {
        private bool captured;
        private float baseTimeScale;
        private float baseFixedDeltaTime;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        private static void InitializeTrampolines()
        {
            Burst.WorldTimeScale.Data = new BurstTrampoline(&ApplyWorldTimeScalePacked);
            Burst.CaptureBase.Data = new BurstTrampoline(&CaptureBasePacked);
            Burst.RestoreBase.Data = new BurstTrampoline(&RestoreBasePacked);
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WorldTimeScale>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!captured)
            {
                var baseValues = default(BaseTime);
                Burst.CaptureBase.Data.Invoke(ref baseValues);
                baseTimeScale = baseValues.TimeScale;
                baseFixedDeltaTime = baseValues.FixedDeltaTime;
                captured = true;
            }

            foreach (var worldScale in SystemAPI.Query<RefRO<WorldTimeScale>>())
                Burst.WorldTimeScale.Data.Invoke(worldScale.ValueRO);
        }

        [BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
            Restore();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            Restore();
        }

        private void Restore()
        {
            if (!captured)
                return;

            captured = false;

            var baseValues = new BaseTime { TimeScale = baseTimeScale, FixedDeltaTime = baseFixedDeltaTime };
            Burst.RestoreBase.Data.Invoke(ref baseValues);
        }

        private static void CaptureBasePacked(void* argumentsPtr, int argumentsSize)
        {
            ref var baseValues = ref BurstTrampoline.ArgumentsFromPtr<BaseTime>(argumentsPtr, argumentsSize);
            baseValues.TimeScale = UnityEngine.Time.timeScale;
            baseValues.FixedDeltaTime = UnityEngine.Time.fixedDeltaTime;
        }

        private static void RestoreBasePacked(void* argumentsPtr, int argumentsSize)
        {
            ref var baseValues = ref BurstTrampoline.ArgumentsFromPtr<BaseTime>(argumentsPtr, argumentsSize);

            if (Mathf.Abs(UnityEngine.Time.timeScale - baseValues.TimeScale) > 0.001f)
                UnityEngine.Time.timeScale = baseValues.TimeScale;

            if (Mathf.Abs(UnityEngine.Time.fixedDeltaTime - baseValues.FixedDeltaTime) > 0.00001f)
                UnityEngine.Time.fixedDeltaTime = baseValues.FixedDeltaTime;
        }

        private static void ApplyWorldTimeScalePacked(void* argumentsPtr, int argumentsSize)
        {
            ref var worldScale = ref BurstTrampoline.ArgumentsFromPtr<WorldTimeScale>(argumentsPtr, argumentsSize);
            var targetScale = WorldTimeScaleResolve.ResolveScale(worldScale);
            var targetFixedDeltaTime = WorldTimeScaleResolve.ResolveFixedDeltaTime(worldScale.DefaultFixedDeltaTime, worldScale);

            if (Mathf.Abs(UnityEngine.Time.timeScale - targetScale) > 0.001f) UnityEngine.Time.timeScale = targetScale;

            if (Mathf.Abs(UnityEngine.Time.fixedDeltaTime - targetFixedDeltaTime) > 0.00001f)
                UnityEngine.Time.fixedDeltaTime = targetFixedDeltaTime;
        }

        private struct BaseTime
        {
            public float TimeScale;
            public float FixedDeltaTime;
        }

        private static class Burst
        {
            public static readonly SharedStatic<BurstTrampoline> WorldTimeScale =
                SharedStatic<BurstTrampoline>.GetOrCreate<WorldTimeScaleApplySystem, WorldTimeScale>();

            public static readonly SharedStatic<BurstTrampoline> CaptureBase =
                SharedStatic<BurstTrampoline>.GetOrCreate<WorldTimeScaleApplySystem, BaseTime>();

            public static readonly SharedStatic<BurstTrampoline> RestoreBase =
                SharedStatic<BurstTrampoline>.GetOrCreate<WorldTimeScaleApplySystem, RestoreKey>();
        }

        private struct RestoreKey
        {
        }
    }
}