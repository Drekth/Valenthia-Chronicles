using UnityEngine;

public class AudioManager : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public void PlayMusic(AudioClip Clip, bool Loop = true)
    {
        MusicSource.clip = Clip;
        MusicSource.loop = Loop;
        MusicSource.Play();
    }

    public void StopMusic()
    {
        MusicSource.Stop();
    }

    public void PlaySfx(AudioClip Clip)
    {
        SfxSource.PlayOneShot(Clip);
    }

    public void PlayAmbient(AudioClip Clip, bool Loop = true)
    {
        AmbientSource.clip = Clip;
        AmbientSource.loop = Loop;
        AmbientSource.Play();
    }

    public void StopAmbient()
    {
        AmbientSource.Stop();
    }

    public void SetMusicVolume(float Volume)
    {
        MusicSource.volume = Mathf.Clamp01(Volume);
    }

    public void SetSfxVolume(float Volume)
    {
        SfxSource.volume = Mathf.Clamp01(Volume);
    }

    public void SetAmbientVolume(float Volume)
    {
        AmbientSource.volume = Mathf.Clamp01(Volume);
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        ServiceLocator.Register<AudioManager>(this);
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Sources")]
    [SerializeField] private AudioSource MusicSource;
    [SerializeField] private AudioSource SfxSource;
    [SerializeField] private AudioSource AmbientSource;
}
