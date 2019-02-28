using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct VelocityData : IComponentData
{
    public float3 Value;
}

public class VelocityComponent : ComponentDataWrapper<VelocityData> { }
