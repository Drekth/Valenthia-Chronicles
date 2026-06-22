using UnityEngine;

// Marks a world entity (an NPC) as talkable and points at the dialogue graph it opens. Sits on the
// same GameObject as the NPC's Unit. Pure data — the right-click input resolves the DialogueRunner
// and starts the conversation, mirroring how ContainerInteractor drives Container.
public class DialogueSource : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public DialogueData Dialogue => DialogueAsset;

    // Name shown as the dialogue panel title. Falls back to the GameObject name when left empty.
    public string DisplayName => string.IsNullOrEmpty(NpcName) ? name : NpcName;

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [SerializeField] private DialogueData DialogueAsset;
    [SerializeField] private string       NpcName;
}
