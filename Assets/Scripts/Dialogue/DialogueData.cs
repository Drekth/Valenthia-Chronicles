using System.Collections.Generic;
using UnityEngine;

// Authored dialogue graph, stored as a DT_ ScriptableObject. Immutable at play time — the runtime
// DialogueRunner walks it without mutating it. Nodes are held polymorphically ([SerializeReference])
// and reference each other by Guid, never by object reference, so the graph survives reordering.
[CreateAssetMenu(menuName = "Valenthia/Narrative/Dialogue")]
public class DialogueData : ScriptableObject
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public string                      Id        => DialogueId;
    public string                      EntryGuid => EntryNodeGuid;
    public IReadOnlyList<DialogueNode> Nodes     => GraphNodes;

    public void AddNode(DialogueNode Node)
    {
        GraphNodes.Add(Node);
    }

    public void RemoveNode(DialogueNode Node)
    {
        GraphNodes.Remove(Node);
    }

    public void SetEntryGuid(string Guid)
    {
        EntryNodeGuid = Guid;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [SerializeField] private string DialogueId;

    [SerializeField, HideInInspector] private string EntryNodeGuid;
    [SerializeReference, HideInInspector] private List<DialogueNode> GraphNodes = new List<DialogueNode>();
}
