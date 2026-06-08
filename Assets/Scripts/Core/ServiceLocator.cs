using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public static void Register<T>(T Service) where T : class
    {
        Type Key = typeof(T);

        if (Services.ContainsKey(Key))
        {
            Debug.LogWarning($"[ServiceLocator] Overwriting existing registration for {Key.Name}.");
        }

        Services[Key] = Service;
    }

    public static T Get<T>() where T : class
    {
        Type Key = typeof(T);

        if (Services.TryGetValue(Key, out object Found))
        {
            return (T)Found;
        }

        Debug.LogError($"[ServiceLocator] Service not found: {Key.Name}.");
        return null;
    }

    public static bool TryGet<T>(out T Service) where T : class
    {
        Type Key = typeof(T);

        if (Services.TryGetValue(Key, out object Found))
        {
            Service = (T)Found;
            return true;
        }

        Service = null;
        return false;
    }

    public static void Unregister<T>() where T : class
    {
        Services.Remove(typeof(T));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Clear()
    {
        Services.Clear();
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>();
}
