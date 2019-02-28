using Unity.Entities;

[InternalBufferCapacity(20)]
public struct NearbyEntity : IBufferElementData
{
    public Entity Value;
}
