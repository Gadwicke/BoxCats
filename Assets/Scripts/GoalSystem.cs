using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class GoalSystem : JobComponentSystem
{
    [BurstCompile]
    public struct AssessGoal : IJobProcessComponentDataWithEntity<GoalData>
    {
        [ReadOnly]
        public BufferFromEntity<NeedValue> Needs;

        public void Execute(Entity entity, int i, ref GoalData goals)
        {
            //Utility function to establish goals based on needs
            //For now  this is simple: Convert the highest need into a goal.
            //In the future, we might weigh needs differently or consider other factors.
            var needs = Needs[entity];

            float highest = float.MinValue;
            int goal = -1;

            for (int j = 0; j < (int)Goals.Count - 1; j++)
            {
                var need = needs[j].Value;

                if (need > highest)
                {
                    goal = j;
                    highest = need;
                }
            }

            //Increment because goals have 0 == None
            goals.CurrentGoal = (byte)(goal + 1);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var assessGoals = new AssessGoal() { Needs = GetBufferFromEntity<NeedValue>(true) };

        return assessGoals.Schedule(this, inputDeps);
    }
}
