using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class VelocityObjectSystem : JobComponentSystem
{
    //How far ahead to look for possible collisions
    public float PredictionWindow = 5;
    public float RegionSize = 20;

    private ComponentGroup _rVelocityGroup;

    [BurstCompile]
    public struct Partition : IJobProcessComponentDataWithEntity<Position>
    {
        public float RegionSize;

        [NativeDisableParallelForRestriction]
        public BufferFromEntity<NearbyEntity> Nearby;

        [ReadOnly, DeallocateOnJobCompletion]
        public NativeArray<ArchetypeChunk> Positions;

        [ReadOnly]
        public ArchetypeChunkEntityType EntityType;
        [ReadOnly]
        public ArchetypeChunkComponentType<Position> PositionType;

        public void Execute(Entity Entity, int index, [ReadOnly] ref Position position)
        {
            if (!Nearby.Exists(Entity))
                return;

            var pos = position.Value;

            var xMin = pos.x - RegionSize;
            var xMax = pos.x + RegionSize;
            var zMin = pos.z - RegionSize;
            var zMax = pos.z + RegionSize;

            var buffer = Nearby[Entity];

            buffer.Clear();

            for (int i = 0; i < Positions.Length; i++)
            {
                var chunk = Positions[i];
                var positions = chunk.GetNativeArray(PositionType);
                var entities = chunk.GetNativeArray(EntityType);

                for (int j = 0; j < chunk.Count; j++)
                {
                    var otherPos = positions[j].Value;

                    if (otherPos.x > xMin && otherPos.x < xMax &&
                        otherPos.z > zMin && otherPos.z < zMax)
                    {
                        buffer.Add(new NearbyEntity() { Value = entities[j] });
                    }
                }
            }
        }
    }

    [BurstCompile]
    public struct AdjustVelocity : IJobProcessComponentDataWithEntity<Position, ReciprocalVelocityData, VelocityData>
    {
        public float PredictionWindow;

        [NativeDisableParallelForRestriction]
        public BufferFromEntity<NearbyEntity> Nearby;

        [ReadOnly]
        public ComponentDataFromEntity<Position> Positions;
        [ReadOnly]
        public ComponentDataFromEntity<ReciprocalVelocityData> rVelocities;

        public void Execute(Entity Entity, int index, [ReadOnly] ref Position position, [ReadOnly] ref ReciprocalVelocityData marker, ref VelocityData velocity)
        {
            //For each other velocity thing
            //position difference
            //c = dot(diff, otherV) / dot(ourV, otherV)
            //intersect = ourPos + (ourV * c)
            var v = velocity.Value;
            var projection = v * PredictionWindow;
            var projectedDist = math.length(projection);
            var pos = position.Value;

            var up = new float3(0, 1, 0);

            var buffer = Nearby[Entity];

            for (int i = 0; i < buffer.Length; i++)
            {
                var entity = buffer[i].Value;
                
                var otherPos = Positions[entity].Value;

                if (!rVelocities.Exists(entity))
                    continue;

                var otherV = rVelocities[entity].Value;

                var diff = pos - otherPos;
                var otherProj = otherV * PredictionWindow;

                var t = math.dot(diff, otherProj) / math.dot(projection, otherProj);

                if (t > 0 && t <= projectedDist)
                {
                    //There's an intersection
                    //Is it near enough in time?
                    var point = pos + (t * v);

                    var otherT = math.length(point - otherPos) / math.length(otherV);

                    if (math.abs(t - otherT) < 1)
                    {
                        //A collision is expected
                        //As the other agent is reciprocal, we can assume they'll take the same action
                        //Therefore we only need to deflect enough to avoid them if they also deflect enough to avoid us.
                        var q = quaternion.AxisAngle(up, math.atan(1 / projectedDist));
                        velocity.Value = math.rotate(q, v);
                    }
                }
            }
        }
    }

    protected override void OnCreateManager()
    {
        _rVelocityGroup = GetComponentGroup(typeof(Position), typeof(ReciprocalVelocityData));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var rvChunks = _rVelocityGroup.CreateArchetypeChunkArray(Allocator.TempJob);

        var partition = new Partition()
        {
            Positions = rvChunks,
            Nearby = GetBufferFromEntity<NearbyEntity>(false),
            EntityType = GetArchetypeChunkEntityType(),
            PositionType = GetArchetypeChunkComponentType<Position>(true),
            RegionSize = RegionSize
        };

        var partDep = partition.Schedule(this, inputDeps);

        var adjust = new AdjustVelocity()
        {
            Nearby = GetBufferFromEntity<NearbyEntity>(true),
            Positions = GetComponentDataFromEntity<Position>(true),
            rVelocities = GetComponentDataFromEntity<ReciprocalVelocityData>(true),
            PredictionWindow = PredictionWindow
        };

        return adjust.Schedule(this, partDep);

        //return JobHandle.CombineDependencies(partDep, adjustDep);
    }
}

