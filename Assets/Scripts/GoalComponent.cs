using System;
using Unity.Entities;

public enum Goals : byte
{
    Eat,
    Drink,
    Socialize,
    Count,
    None = 0xFF
}

[Serializable]
public struct GoalData : IComponentData
{
    public byte CurrentGoal;
}

public class GoalComponent : ComponentDataWrapper<GoalData> { }
