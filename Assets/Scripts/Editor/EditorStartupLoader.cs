using UnityEditor;
using UnityEditor.SceneManagement;

// Forces Play mode to always start from SC_GameRoot (Bootstrap), regardless of which
// scenes are currently open in the editor. The editor context is preserved after exiting.
[InitializeOnLoad]
public static class EditorStartupLoader
{
    private const string BootstrapScenePath = "Assets/Scenes/SC_GameRoot.unity";

    static EditorStartupLoader()
    {
        SceneAsset BootstrapScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapScenePath);

        if (BootstrapScene == null)
        {
            UnityEngine.Debug.LogWarning("[EditorStartupLoader] SC_GameRoot.unity not found — playModeStartScene not set.");
            return;
        }

        EditorSceneManager.playModeStartScene = BootstrapScene;
    }
}
