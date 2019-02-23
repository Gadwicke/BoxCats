using Unity.Entities;

[InternalBufferCapacity(6)]
public struct NeedValue: IBufferElementData
{
    public static implicit operator float(NeedValue e) { return e.Value; }
    public static implicit operator NeedValue(float e) { return new NeedValue { Value = e }; }

    public float Value;
}

