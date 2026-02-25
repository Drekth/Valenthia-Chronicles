using UnityEngine;

namespace ValenthiaChronicles.Core.Patterns
{
    public interface IObjectPool<T> where T : Component
    {
        T Get();
        void Release(T instance);
        void PreWarm(int count);
        int CountActive { get; }
        int CountInactive { get; }
    }
}
