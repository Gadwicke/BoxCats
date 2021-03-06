﻿using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour
{
    public GameObject CatBoxPrefab;
    public GameObject BodyPrefab;
    public GameObject FoodPrefab;
    public GameObject FoodBodyPrefab;
    public GameObject WaterPrefab;
    public GameObject WaterBodyPrefab;

    EntityManager _manager;
    
    void Start()
    {
        //Initialize this here static
        var discard = SafeClock.Instance.UtcNow;

        _manager = World.Active.GetOrCreateManager<EntityManager>();

        //Demo only for now
        Demo();
    }

    private void Demo()
    {
        //Cats
        var count = 1000;
        NativeArray<Entity> entities = new NativeArray<Entity>(count, Allocator.Temp);
        _manager.Instantiate(CatBoxPrefab, entities);

        for (int i = 0; i < count; i++)
        {
            var pos = new float3(Random.Range(-200, 200), 0, Random.Range(-200, 200));

            _manager.SetComponentData(entities[i], new Position() { Value = pos });
            _manager.SetComponentData(entities[i], new Waypoint() { Reached = 0x1 });

            _manager.AddBuffer<NeedValue>(entities[i]);
            var needs = _manager.GetBuffer<NeedValue>(entities[i]);

            for (int j = 0; j < (int)Goals.Count; j++)
            {
                needs.Add(0);
            }

            _manager.AddBuffer<NeedModifierValue>(entities[i]);
            var modifiers = _manager.GetBuffer<NeedModifierValue>(entities[i]);

            for (int j = 0; j < (int)Goals.Count; j++)
            {
                modifiers.Add(Random.Range(0.5f, 1.5f));
            }

            _manager.AddBuffer<NearbyEntity>(entities[i]);

            //We're using traditional prefabs here because ECS doesn't really support prefab hierarchies or complex objects.
            //And we're too lazy to implement our own renderer component to make the complex entity we need. ...For now.
            //But we want complex objects. So we accept the limitations this imposes.
            var bodyGO = Instantiate(BodyPrefab);
            var updater = bodyGO.GetComponent<UpdateTransformFromEntity>();
            if (updater != null)
                updater.Parent = entities[i];
        }

        entities.Dispose();

        //Food
        count = 10;
        entities = new NativeArray<Entity>(count, Allocator.Temp);
        _manager.Instantiate(FoodPrefab, entities);

        for (int i = 0; i < count; i++)
        {
            var pos = new float3(Random.Range(-200, 200), 0, Random.Range(-200, 200));

            _manager.SetComponentData(entities[i], new Position() { Value = pos });

            var bodyGO = Instantiate(FoodBodyPrefab);
            var updater = bodyGO.GetComponent<UpdateTransformFromEntity>();
            if (updater != null)
                updater.Parent = entities[i];
        }

        entities.Dispose();

        //Water
        count = 10;
        entities = new NativeArray<Entity>(count, Allocator.Temp);
        _manager.Instantiate(WaterPrefab, entities);

        for (int i = 0; i < count; i++)
        {
            var pos = new float3(Random.Range(-200, 200), 0, Random.Range(-200, 200));

            _manager.SetComponentData(entities[i], new Position() { Value = pos });

            var bodyGO = Instantiate(WaterBodyPrefab);
            var updater = bodyGO.GetComponent<UpdateTransformFromEntity>();
            if (updater != null)
                updater.Parent = entities[i];
        }

        entities.Dispose();
    }
}
