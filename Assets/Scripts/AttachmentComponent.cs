using System;
using Unity.Entities;

public enum AttachmentState : byte
{
    Unattached,
    Navigating,
    Attached,
    Complete
}

[Serializable]
public struct AttachmentData : IComponentData
{
    public byte AttachmentState;
    public Entity AttachedEntity;
    public float AttachmentDuration;
    public double AttachmentBeganTimestamp;
    public double LastTimeProcessed;
}

public class AttachmentComponent : ComponentDataWrapper<AttachmentData> { }
