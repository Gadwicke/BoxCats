using System;
using Unity.Entities;

[Serializable]
public struct NeedProviderData : IComponentData
{
    public byte NeedSatisfied;
    public float AmountPerUse;
    public float TimePerUse;
}

public class NeedProvider : ComponentDataWrapper<NeedProviderData> { }
