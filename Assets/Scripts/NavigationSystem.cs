﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Selects waypoints to reach a given target.
/// </summary>
public class NavigationSystem : JobComponentSystem
{
    ComponentGroup _providerData;

    [BurstCompile]
    public struct Navigate : IJobProcessComponentData<Position, Waypoint, GoalData, AttachmentData>
    {
        public Random Rand;

        [ReadOnly, DeallocateOnJobCompletion]
        public NativeArray<ArchetypeChunk> Providers;

        [ReadOnly] public ArchetypeChunkComponentType<Position> PositionType;
        [ReadOnly] public ArchetypeChunkComponentType<NeedProviderData> ProviderType;
        [ReadOnly] public EntityArray Entities;

        public void Execute([ReadOnly] ref Position OurPosition, ref Waypoint waypoint, [ReadOnly] ref GoalData goals, ref AttachmentData attach)
        {
            if ((goals.CurrentGoal == (byte)Goals.None && waypoint.Reached == 0x1) || goals.CurrentGoal != (byte)Goals.None)
            {
                if (goals.CurrentGoal != (byte)Goals.None)
                {
                    float3 nearest = float3.zero;
                    float shortest = float.MaxValue;
                    NeedProviderData providerData = new NeedProviderData();
                    Entity providerEntity = new Entity();
                    bool found = false;

                    for (int i = 0; i < Providers.Length; i++)
                    {
                        var chunk = Providers[i];
                        var positions = chunk.GetNativeArray(PositionType);
                        var providers = chunk.GetNativeArray(ProviderType);

                        for (int j = 0; j < chunk.Count; j++)
                        {
                            var provider = providers[j];

                            if (provider.NeedSatisfied != goals.CurrentGoal)
                                continue;

                            var pos = positions[j];

                            var dist = math.distance(pos.Value, OurPosition.Value);

                            if (dist == 0)
                                continue;

                            if (dist < shortest)
                            {
                                shortest = dist;
                                nearest = pos.Value;
                                providerData = provider;
                                providerEntity = Entities[j];
                                found = true;
                            }
                        }
                    }

                    if (found)
                    {
                        if (attach.AttachmentState != (byte)AttachmentState.Attached)
                        {
                            //Set the waypoint to the provider
                            //waypoint.x = nearest.x;
                            //waypoint.y = nearest.y;
                            //waypoint.z = nearest.z;
                            waypoint.Value = nearest;
                            waypoint.Goal = goals.CurrentGoal;

                            //Prepare the attachment
                            attach.AttachmentDuration = providerData.TimePerUse;
                            attach.AttachedEntity = providerEntity;
                            attach.AttachmentState = (byte)AttachmentState.Navigating;
                        }
                    }
                    else
                    {
                        //We're wandering, so set a completely random waypoint.
                        var x = (Rand.NextFloat() * 400) - 200;
                        var z = (Rand.NextFloat() * 400) - 200;
                        waypoint.Value = new float3(x, 0, z);
                        waypoint.Goal = goals.CurrentGoal;

                        attach.AttachmentDuration = 0;
                        attach.AttachedEntity = Entity.Null;
                        attach.AttachmentState = (byte)AttachmentState.Unattached;
                    }
                }
                else
                {
                    //We're wandering, so set a completely random waypoint.
                    var x = (Rand.NextFloat() * 400) - 200;
                    var z = (Rand.NextFloat() * 400) - 200;
                    waypoint.Value = new float3(x, 0, z);
                    waypoint.Goal = goals.CurrentGoal;

                    attach.AttachmentDuration = 0;
                    attach.AttachedEntity = Entity.Null;
                    attach.AttachmentState = (byte)AttachmentState.Unattached;
                }
            }

            if (attach.AttachmentState == (byte)AttachmentState.Navigating)
            {
                if (waypoint.Reached == 0x1 && goals.CurrentGoal == waypoint.Goal)
                {
                    //We've reached the target, so attach to it.
                    attach.AttachmentState = (byte)AttachmentState.Attached;
                    var time = SafeClock.TimeSinceInitialization;
                    attach.AttachmentBeganTimestamp = time;
                    attach.LastTimeProcessed = time;
                }
            }
        }
    }

    protected override void OnCreateManager()
    {
        _providerData = EntityManager.CreateComponentGroup(typeof(NeedProviderData), typeof(Position));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var chunks = _providerData.CreateArchetypeChunkArray(Allocator.TempJob);
        var entities = _providerData.GetEntityArray();

        var navigate = new Navigate()
        {
            Rand = new Random((uint)UnityEngine.Random.Range(0, int.MaxValue)),
            Providers = chunks,
            PositionType = GetArchetypeChunkComponentType<Position>(true),
            ProviderType = GetArchetypeChunkComponentType<NeedProviderData>(true),
            Entities = entities
        };

        return navigate.Schedule(this, inputDeps);
    }
}
