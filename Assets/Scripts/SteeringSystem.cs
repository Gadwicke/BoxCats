using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Updates position and rotation to steer a character toward input waypoints.
/// </summary>
public class SteeringSystem : JobComponentSystem
{
    public float MaximumSpeed = 1;
    public float MaximumRotation = 30;
    
    [BurstCompile]
    private struct SteerCharacter : IJobProcessComponentData<Position, Rotation, Waypoint>
    {
        public float DeltaTime;
        public float MaximumSpeed;
        public float MaximumRotation;

        public void Execute(ref Position position, ref Rotation rotation, [ReadOnly] ref Waypoint waypoint)
        {
            //For now just lerp rotation and position as fast as possible.
            //Ideally we would S curve this but for now we're KISSing.
            var point = waypoint.Value;

            var posDelta = point - position.Value;
            var dist = math.length(posDelta);

            if (dist < 0.5f)
            {
                waypoint.Reached = 0x1;
                return;
            }

            waypoint.Reached = 0x0;

            var dir = (float3)(posDelta / dist);
            position.Value += dir * MaximumSpeed * DeltaTime;

            var rot = Quaternion.RotateTowards(rotation.Value, quaternion.LookRotationSafe(dir, new float3(0, 1, 0)), MaximumRotation * DeltaTime);
            rotation.Value = rot;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var steering = new SteerCharacter() { DeltaTime = Time.deltaTime, MaximumSpeed = MaximumSpeed, MaximumRotation = MaximumRotation };

        return steering.Schedule(this, inputDeps);
    }
}
