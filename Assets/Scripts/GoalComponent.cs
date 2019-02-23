using System;
using Unity.Entities;

public enum Goals : byte
{
    None,
    Eat,
    Drink,
    Socialize,
    Count
}

[Serializable]
public struct GoalData : IComponentData
{
    public byte CurrentGoal;
}

public class GoalComponent : ComponentDataWrapper<GoalData> { }
