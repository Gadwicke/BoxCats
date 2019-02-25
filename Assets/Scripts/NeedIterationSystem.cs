using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Iterates needs over time, taking modifiers into account.
/// </summary>
public class NeedIterationSystem : JobComponentSystem
{
    [BurstCompile]
    private struct IterateNeeds : IJobProcessComponentDataWithEntity<Waypoint> //Feeding in position because there
    {
        public float deltaTime;

        [NativeDisableParallelForRestriction]
        public BufferFromEntity<NeedValue> Needs;

        [ReadOnly]
        public BufferFromEntity<NeedModifierValue> Modifiers;

        public void Execute(Entity entity, int i, [ReadOnly] ref Waypoint pos)
        {
            float change = (float)(0.2f * deltaTime);

            var needs = Needs[entity].Reinterpret<float>();
            var modifiers = Modifiers[entity];

            for (int j = 0; j < (int)Goals.Count; j++)
            {
                var need = needs[j];
                var mod = modifiers[j];

                need += mod * change;
                needs[j] = need;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var iterator = new IterateNeeds() { deltaTime = Time.deltaTime, Needs = GetBufferFromEntity<NeedValue>(), Modifiers = GetBufferFromEntity<NeedModifierValue>(true) };

        return iterator.Schedule(this, inputDeps);
    }
}
