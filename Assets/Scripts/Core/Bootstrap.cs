using Cysharp.Threading.Tasks;
using UnityEngine;

// Single entry point for the game. This scene is persistent and never unloaded.
// All global services are registered here and available for the lifetime of the application.
public class Bootstrap : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        if (!ValidateServices()) { return; }

        AudioManagerRef.gameObject.SetActive(true);
        TimeManagerRef.gameObject.SetActive(true);
        SceneLoaderRef.gameObject.SetActive(true);
    }

    private async UniTaskVoid Start()
    {
        await SceneLoaderRef.LoadSceneAdditive(UISceneName);
        await SceneLoaderRef.SwapZone(FirstZoneName);
    }

    private bool ValidateServices()
    {
        bool Valid = true;

        if (AudioManagerRef == null) { Debug.LogError("[Bootstrap] AudioManager is not assigned.", this); Valid = false; }
        if (TimeManagerRef  == null) { Debug.LogError("[Bootstrap] TimeManager is not assigned.",  this); Valid = false; }
        if (SceneLoaderRef  == null) { Debug.LogError("[Bootstrap] SceneLoader is not assigned.",  this); Valid = false; }

        return Valid;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Services")]
    [SerializeField] private AudioManager AudioManagerRef;
    [SerializeField] private TimeManager TimeManagerRef;
    [SerializeField] private SceneLoader SceneLoaderRef;

    [Header("Initial Scenes")]
    [SerializeField] private string UISceneName   = "SC_UI";
    [SerializeField] private string FirstZoneName = "SC_Zone_Test";
}
