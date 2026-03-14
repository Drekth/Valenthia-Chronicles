using Sisus.Init;
using System.Collections.Generic;
using UnityEngine;

[Service]
public class MovementSystem : MonoBehaviour
{
    private readonly List<MovementAuthoring> activeEntities = new List<MovementAuthoring>();
    private readonly Dictionary<MovementType, IMovementProcessor> processors = new Dictionary<MovementType, IMovementProcessor>();

    private void Awake()
    {
        // Register processors
        var wanderProcessor = new WanderProcessor();
        processors.Add(wanderProcessor.HandledType, wanderProcessor);
    }

    public void Register(MovementAuthoring entity)
    {
        if (!activeEntities.Contains(entity))
        {
            activeEntities.Add(entity);
        }
    }

    public void Unregister(MovementAuthoring entity)
    {
        activeEntities.Remove(entity);
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        
        // Zero-allocation loop for performance processing
        for (int i = 0; i < activeEntities.Count; i++)
        {
            var entity = activeEntities[i];
            if (entity.CurrentType != MovementType.None && processors.TryGetValue(entity.CurrentType, out var processor))
            {
                processor.Process(entity, deltaTime);
            }
        }
    }
}
