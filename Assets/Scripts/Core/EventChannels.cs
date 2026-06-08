using System;
using UnityEngine;

////////////////////////////////////////////////////////////
/// Void — no payload                                    ///
////////////////////////////////////////////////////////////

[CreateAssetMenu(menuName = "Valenthia/Events/Void Event Channel")]
public class VoidEventChannel : ScriptableObject
{
    public void Raise()
    {
        OnRaised?.Invoke();
    }

    public void Subscribe(Action Listener)
    {
        OnRaised += Listener;
    }

    public void Unsubscribe(Action Listener)
    {
        OnRaised -= Listener;
    }

    private event Action OnRaised;
}

////////////////////////////////////////////////////////////
/// Generic base — typed payload                         ///
////////////////////////////////////////////////////////////

public class EventChannel<T> : ScriptableObject
{
    public void Raise(T Value)
    {
        OnRaised?.Invoke(Value);
    }

    public void Subscribe(Action<T> Listener)
    {
        OnRaised += Listener;
    }

    public void Unsubscribe(Action<T> Listener)
    {
        OnRaised -= Listener;
    }

    private event Action<T> OnRaised;
}

////////////////////////////////////////////////////////////
/// Concrete typed channels                              ///
////////////////////////////////////////////////////////////

[CreateAssetMenu(menuName = "Valenthia/Events/Int Event Channel")]
public class IntEventChannel : EventChannel<int> { }

[CreateAssetMenu(menuName = "Valenthia/Events/Float Event Channel")]
public class FloatEventChannel : EventChannel<float> { }

[CreateAssetMenu(menuName = "Valenthia/Events/String Event Channel")]
public class StringEventChannel : EventChannel<string> { }
