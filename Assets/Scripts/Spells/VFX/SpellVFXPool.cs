using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


/// <summary>
/// Object pool for spell VFX prefabs. One pool per unique prefab.
/// Avoids Instantiate/Destroy on hot paths.
/// </summary>
public sealed class SpellVFXPool
{
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        var pool = GetOrCreatePool(prefab);
        var instance = pool.Get();
        
        instance.transform.SetParent(parent);
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);

        return instance;
    }

    public void Release(GameObject prefab, GameObject instance, float delay = 0f)
    {
        if (delay > 0f)
        {
            // Use coroutine-free delayed release via a helper MonoBehaviour
            VFXReleaseTimer.Schedule(instance, delay, () => ReleaseImmediate(prefab, instance));
        }
        else
        {
            ReleaseImmediate(prefab, instance);
        }
    }

    private void ReleaseImmediate(GameObject prefab, GameObject instance)
    {
        if (instance == null) return;
        
        instance.SetActive(false);
        
        if (pools.TryGetValue(prefab, out var pool))
        {
            pool.Release(instance);
        }
        else
        {
            Object.Destroy(instance);
        }
    }

    private ObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out var pool))
        {
            pool = new ObjectPool<GameObject>(
                createFunc: () => Object.Instantiate(prefab),
                actionOnGet: obj => obj.SetActive(true),
                actionOnRelease: obj => obj.SetActive(false),
                actionOnDestroy: obj => Object.Destroy(obj),
                collectionCheck: false,
                defaultCapacity: 4,
                maxSize: 20
            );
            pools[prefab] = pool;
        }

        return pool;
    }

    private readonly Dictionary<GameObject, ObjectPool<GameObject>> pools = new();
}
