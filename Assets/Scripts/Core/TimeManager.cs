using UnityEngine;

public class TimeManager : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public bool IsPaused => Time.timeScale == 0f;

    public void Pause()
    {
        if (IsPaused) { return; }

        TimeScaleBeforePause = Time.timeScale;
        Time.timeScale = 0f;
        EventBus<PausedEvent>.Raise(new PausedEvent());
    }

    public void Resume()
    {
        if (!IsPaused) { return; }

        Time.timeScale = TimeScaleBeforePause;
        EventBus<ResumedEvent>.Raise(new ResumedEvent());
    }

    public void SetTimeScale(float Scale)
    {
        TimeScaleBeforePause = Scale;

        if (!IsPaused)
        {
            Time.timeScale = Scale;
        }
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        ServiceLocator.Register<TimeManager>(this);
    }

    private void OnApplicationPause(bool Paused)
    {
        if (Paused) { Pause(); }
        else        { Resume(); }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private float TimeScaleBeforePause = 1f;
}
