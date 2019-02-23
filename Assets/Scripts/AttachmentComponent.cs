using System;
using Unity.Entities;

[Serializable]
public struct AttachmentData : IComponentData
{
    public byte Attached;
    public Entity AttachedEntity;
    public float AttachmentDuration;
    public double AttachmentBeganTimestamp;
    public double LastTimeProcessed;
}

public class AttachmentComponent : ComponentDataWrapper<AttachmentData> { }
