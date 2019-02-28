using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct ReciprocalVelocityData : IComponentData
{
    public float3 Value;
}

public class ReciprocalVelocityComponent : ComponentDataWrapper<ReciprocalVelocityData> { }
