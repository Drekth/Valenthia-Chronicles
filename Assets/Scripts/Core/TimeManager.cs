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
        OnPaused?.Raise();
    }

    public void Resume()
    {
        if (!IsPaused) { return; }

        Time.timeScale = TimeScaleBeforePause;
        OnResumed?.Raise();
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

    [Header("Events")]
    [SerializeField] private VoidEventChannel OnPaused;
    [SerializeField] private VoidEventChannel OnResumed;

    private float TimeScaleBeforePause = 1f;
}
