using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class GoalSystem : JobComponentSystem
{
    [BurstCompile]
    public struct AssessGoal : IJobProcessComponentDataWithEntity<GoalData, AttachmentData>
    {
        [ReadOnly]
        public BufferFromEntity<NeedValue> Needs;

        public void Execute(Entity entity, int i, ref GoalData goals, [ReadOnly] ref  AttachmentData attach)
        {
            //Utility function to establish goals based on needs
            //For now  this is simple: Convert the highest need into a goal.
            //In the future, we might weigh needs differently or consider other factors.

            //Do not change goals if we're attached to something.
            if (attach.AttachmentState == (byte)AttachmentState.Attached)
                return;

            var needs = Needs[entity];

            float highest = float.MinValue;
            int goal = -1;

            for (int j = 0; j < (int)Goals.Count; j++)
            {
                var need = needs[j].Value;

                if (need > highest)
                {
                    goal = j;
                    highest = need;
                }
            }

            if (highest >= 30)
            {
                //Increment because goals have 0 == None
                goals.CurrentGoal = (byte)goal;
            }
            else
            {
                goals.CurrentGoal = (byte)Goals.None;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var assessGoals = new AssessGoal() { Needs = GetBufferFromEntity<NeedValue>(true) };

        return assessGoals.Schedule(this, inputDeps);
    }
}
