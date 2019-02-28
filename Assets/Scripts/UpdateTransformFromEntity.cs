using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class UpdateTransformFromEntity : MonoBehaviour
{
    public Entity Parent;
    public bool SetOnce = false;

    private EntityManager _manager;

    private void Start()
    {
        _manager = World.Active.GetOrCreateManager<EntityManager>();

        if (SetOnce)
        {
            if (Parent != null)
            {
                var pos = _manager.GetComponentData<Position>(Parent);
                var rot = _manager.GetComponentData<Rotation>(Parent);

                var t = transform;
                t.SetPositionAndRotation(pos.Value, rot.Value);
            }

            enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Parent != null)
        {
            var pos = _manager.GetComponentData<Position>(Parent);
            var rot = _manager.GetComponentData<Rotation>(Parent);

            var t = transform;
            t.SetPositionAndRotation(pos.Value, rot.Value);
        }
    }
}
