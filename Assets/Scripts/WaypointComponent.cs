using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Waypoint : IComponentData
{
    public float3 Value;
    public byte Reached;
    public byte Goal;
}

public class WaypointComponent : ComponentDataWrapper<Waypoint> { }
