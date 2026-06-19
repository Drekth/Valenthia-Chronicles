using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public bool IsLoading { get; private set; }

    public async UniTask SwapZone(string NewZoneName)
    {
        if (IsLoading) { return; }

        IsLoading = true;
        EventBus<LoadStartEvent>.Raise(new LoadStartEvent());

        await LoadSceneAdditive(NewZoneName);

        if (!string.IsNullOrEmpty(CurrentZone))
        {
            await UnloadScene(CurrentZone);
        }

        CurrentZone = NewZoneName;
        IsLoading = false;
        EventBus<LoadCompleteEvent>.Raise(new LoadCompleteEvent());
    }

    public async UniTask LoadSceneAdditive(string SceneName)
    {
        if (SceneManager.GetSceneByName(SceneName).isLoaded) { return; }

        await SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive)
                          .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    public async UniTask UnloadScene(string SceneName)
    {
        await SceneManager.UnloadSceneAsync(SceneName)
                          .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        ServiceLocator.Register<SceneLoader>(this);
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private string CurrentZone;
}
