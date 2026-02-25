using System.Collections.Generic;
using UnityEngine;

namespace ValenthiaChronicles.Core.Patterns
{
    public class ObjectPool<T> : IObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _pool = new();
        private readonly List<T> _active = new();

        public int CountActive => _active.Count;
        public int CountInactive => _pool.Count;

        public ObjectPool(T prefab, Transform parent = null, int initialSize = 0)
        {
            _prefab = prefab;
            _parent = parent;
            if (initialSize > 0) PreWarm(initialSize);
        }

        public void PreWarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var instance = Object.Instantiate(_prefab, _parent);
                instance.gameObject.SetActive(false);
                _pool.Enqueue(instance);
            }
        }

        public T Get()
        {
            T instance;
            if (_pool.Count > 0)
            {
                instance = _pool.Dequeue();
            }
            else
            {
                GameLogger.Warn($"[ObjectPool] Pool<{typeof(T).Name}> exhausted — auto-expanding");
                instance = Object.Instantiate(_prefab, _parent);
            }

            instance.gameObject.SetActive(true);
            _active.Add(instance);
            return instance;
        }

        public void Release(T instance)
        {
            instance.gameObject.SetActive(false);
            _active.Remove(instance);
            _pool.Enqueue(instance);
        }
    }
}
