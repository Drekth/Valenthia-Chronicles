using System;
using UnityEngine;


/// <summary>
/// Lightweight MonoBehaviour that auto-attaches to VFX instances for delayed pool release.
/// Self-destructs when the host is returned to the pool.
/// </summary>
public sealed class VFXReleaseTimer : MonoBehaviour
{
    public static void Schedule(GameObject target, float delay, Action onRelease)
    {
        var timer = target.GetComponent<VFXReleaseTimer>();
        if (timer == null)
            timer = target.AddComponent<VFXReleaseTimer>();

        timer.releaseCallback = onRelease;
        timer.timer = delay;
        timer.enabled = true;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            enabled = false;
            releaseCallback?.Invoke();
        }
    }

    private void OnDisable()
    {
        releaseCallback = null;
    }

    private Action releaseCallback;
    private float timer;
}
