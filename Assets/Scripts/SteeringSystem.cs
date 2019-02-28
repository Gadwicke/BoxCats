using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Updates position and rotation to steer a character toward input waypoints.
/// </summary>]
public class SteeringSystem : JobComponentSystem
{
    public float MaximumSpeed = 1;

    [BurstCompile]
    private struct SteerCharacter : IJobProcessComponentData<Waypoint, Position, VelocityData>
    {
        public float MaximumSpeed;

        public void Execute([ReadOnly] ref Waypoint waypoint, [ReadOnly] ref Position position, ref VelocityData velocity)
        {
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
            velocity.Value = dir * MaximumSpeed;
        }
    }

    [BurstCompile]
    private struct SetReciprocalVelocity : IJobProcessComponentData<VelocityData, ReciprocalVelocityData>
    {
        public void Execute([ReadOnly] ref VelocityData velocity, ref ReciprocalVelocityData rv)
        {
            rv.Value = velocity.Value;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var steering = new SteerCharacter() { MaximumSpeed = MaximumSpeed };
        var rv = new SetReciprocalVelocity() { };

        var steer = steering.Schedule(this, inputDeps);

        return rv.Schedule(this, steer);
    }
}

[UpdateAfter(typeof(VelocityObjectSystem))]
public class MovementSystem : JobComponentSystem
{
    public float MaximumSpeed = 1;
    public float MaximumRotation = 30;

    [BurstCompile]
    private struct MoveCharacter : IJobProcessComponentData<Position, Rotation, VelocityData>
    {
        public float DeltaTime;
        public float MaximumRotation;

        public void Execute(ref Position position, ref Rotation rotation, [ReadOnly] ref VelocityData velocity)
        {
            //For now just lerp rotation and position as fast as possible.
            //Ideally we would S curve this but for now we're KISSing.
            var v = velocity.Value;

            position.Value += v * DeltaTime;

            var rot = Quaternion.RotateTowards(rotation.Value, quaternion.LookRotationSafe(v, new float3(0, 1, 0)), MaximumRotation * DeltaTime);
            rotation.Value = rot;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var movement = new MoveCharacter() { DeltaTime = Time.deltaTime, MaximumRotation = MaximumRotation };

        return movement.Schedule(this, inputDeps);
    }
}
