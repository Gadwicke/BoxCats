using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class NeedProviderSystem : JobComponentSystem
{
    [BurstCompile]
    public struct ApplyNeeds : IJobProcessComponentDataWithEntity<AttachmentData>
    {
        [ReadOnly]
        public ComponentDataFromEntity<NeedProviderData> ProviderData;

        [NativeDisableParallelForRestriction]
        public BufferFromEntity<NeedValue> Needs;

        public void Execute(Entity thisEntity, int i, ref AttachmentData attach)
        {
            if (attach.Attached != 0x1)
                return;

            var time = SafeClock.TimeSinceInitialization;
            double deltaTime = 0;
            var expiry = attach.AttachmentBeganTimestamp + attach.AttachmentDuration;

            if (expiry < time)
            {
                //Only use the time up until the expiry
                deltaTime = expiry - attach.LastTimeProcessed;
            }
            else
            {
                deltaTime = time - attach.LastTimeProcessed;
            }

            attach.LastTimeProcessed = time;

            //If we can somehow get the attached entity...
            Entity entity = attach.AttachedEntity;

            if (entity != Entity.Null)
            {
                var needProvider = ProviderData[entity];

                var amount = math.max(0, (needProvider.AmountPerUse / needProvider.TimePerUse) * (float)deltaTime);

                var needs = Needs[thisEntity];

                var index = (int)needProvider.NeedSatisfied;

                var need = needs[index];

                need -= amount;
                need = math.max(0, need);

                needs[index] = need;

                if (need == 0)
                    attach.Attached = 0x0;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var apply = new ApplyNeeds() { ProviderData = GetComponentDataFromEntity<NeedProviderData>(true), Needs = GetBufferFromEntity<NeedValue>(false) };

        return apply.Schedule(this, inputDeps);
    }
}
