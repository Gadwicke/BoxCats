using System;
using Unity.Entities;

[Serializable]
public struct Waypoint : IComponentData
{
    public float x;
    public float y;
    public float z;
    public byte Reached;
    public byte Goal;
}

public class WaypointComponent : ComponentDataWrapper<Waypoint> { }
