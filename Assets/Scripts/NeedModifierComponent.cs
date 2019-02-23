using Unity.Entities;

[InternalBufferCapacity(6)]
public struct NeedModifierValue : IBufferElementData
{
    public static implicit operator float(NeedModifierValue e) { return e.Value; }
    public static implicit operator NeedModifierValue(float e) { return new NeedModifierValue { Value = e }; }

    public float Value;
}