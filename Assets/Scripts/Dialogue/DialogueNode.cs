using System;
using System.Collections.Generic;
using UnityEngine;

// Polymorphic dialogue-graph node, serialized inside DialogueData via [SerializeReference]. The base
// holds graph wiring only (identity, editor layout, and output ports addressed by index); concrete
// types add their own payload, plus presentation hints the editor renders on the node:
//   - DisplayName : the node title.
//   - InputLabel  : the incoming connector's name.
//   - GetOutputLabel(i) : each outgoing connector's name (e.g. Next, True/False, Choice N).
//   - Summary     : a short one-line preview of the node's content shown in its body.
//
// Connections are stored by target Guid (string), never by object reference, so the graph survives
// reordering. Adding a node type is a new [Serializable] class — the editor discovers it by
// reflection.

[Serializable]
public abstract class DialogueNode
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public abstract string DisplayName { get; }

    public string  Guid           { get => NodeGuid;     set => NodeGuid = value; }
    public Vector2 EditorPosition { get => NodePosition; set => NodePosition = value; }

    // StartNode is the entry, so it has no incoming connection. Everything else accepts one.
    public virtual bool   AcceptsInput => true;
    public virtual string InputLabel   => "Previous";

    public virtual int    OutputCount               => 1;
    public virtual string GetOutputLabel(int Index)  => "Next";
    public virtual string GetOutputTarget(int Index) => NextNodeGuid;
    public virtual void   SetOutputTarget(int Index, string TargetGuid) => NextNodeGuid = TargetGuid;

    // One-line preview of the node's content, shown in its body on the canvas.
    public virtual string Summary => string.Empty;

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [SerializeField, HideInInspector] private string  NodeGuid;
    [SerializeField, HideInInspector] private Vector2 NodePosition;
    [SerializeField, HideInInspector] private string  NextNodeGuid;
}

// Entry point of the graph. No incoming connection and a single "Next" output; the runner starts
// here. Titled "Greeting" — a conversation opens on it. Carries the opening line shown when the
// dialogue is first opened (the WoW-style greeting), so no separate LineNode is needed for it.
// Leaving the text empty makes it a pure pass-through entry.
[Serializable]
public class StartNode : DialogueNode
{
    public override string DisplayName  => "Greeting";
    public override bool   AcceptsInput => false;

    public string Text => GreetingText;

    public override string Summary
    {
        get
        {
            if (string.IsNullOrEmpty(GreetingText))
            {
                return "Conversation start";
            }
            return DialoguePreview.Shorten(GreetingText, PreviewLength);
        }
    }

    private const int PreviewLength = 40;

    [TextArea]
    [SerializeField] private string GreetingText;
}

// Terminates the conversation. No outgoing connection.
[Serializable]
public class EndNode : DialogueNode
{
    public override string DisplayName => "End";
    public override int    OutputCount => 0;
    public override string Summary     => "Conversation end";
}

// One spoken line: who says it and what they say.
[Serializable]
public class LineNode : DialogueNode
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public override string DisplayName => "Line";

    public string Speaker => SpeakerName;
    public string Text     => LineText;

    public override string Summary
    {
        get
        {
            string Body = string.IsNullOrEmpty(LineText) ? "(empty line)" : DialoguePreview.Shorten(LineText, PreviewLength);
            if (!string.IsNullOrEmpty(SpeakerName))
            {
                return $"{SpeakerName}: {Body}";
            }
            return Body;
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private const int PreviewLength = 40;

    [SerializeField] private string SpeakerName;

    [TextArea]
    [SerializeField] private string LineText;
}

// Branches the flow on a condition. Outputs are True / False. v1 stores the condition as a
// human-readable placeholder string; it becomes a polymorphic INarrativeCondition once the shared
// WorldState layer exists.
[Serializable]
public class ConditionNode : DialogueNode
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public override string DisplayName => "Condition";

    public override int    OutputCount              => 2;
    public override string GetOutputLabel(int Index) => Index == 0 ? "True" : "False";

    public override string GetOutputTarget(int Index) => Index == 0 ? TrueGuid : FalseGuid;

    public override void SetOutputTarget(int Index, string TargetGuid)
    {
        if (Index == 0)
        {
            TrueGuid = TargetGuid;
        }
        else
        {
            FalseGuid = TargetGuid;
        }
    }

    public override string Summary => string.IsNullOrEmpty(ConditionText) ? "(no condition)" : DialoguePreview.Shorten(ConditionText, PreviewLength);

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private const int PreviewLength = 40;

    [SerializeField] private string ConditionText;

    [SerializeField, HideInInspector] private string TrueGuid;
    [SerializeField, HideInInspector] private string FalseGuid;
}

// Presents player options. Each choice is one output port; the player's pick routes the flow.
// Per-choice display conditions arrive later, once the shared INarrativeCondition library exists.
[Serializable]
public class ChoiceNode : DialogueNode
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public override string DisplayName => "Choice";

    // Exposes the authored choices so the runtime DialogueRunner can read their text; the targets
    // are still reached through GetOutputTarget(Index), keeping the by-Guid wiring authoritative.
    public IReadOnlyList<DialogueChoice> Choices => ChoiceList;

    public override int    OutputCount              => ChoiceList.Count;
    public override string GetOutputLabel(int Index) => $"Choice {Index + 1}";

    public override string GetOutputTarget(int Index)
    {
        if (Index < 0 || Index >= ChoiceList.Count)
        {
            return string.Empty;
        }
        return ChoiceList[Index].TargetGuid;
    }

    public override void SetOutputTarget(int Index, string TargetGuid)
    {
        if (Index < 0 || Index >= ChoiceList.Count)
        {
            return;
        }
        ChoiceList[Index].TargetGuid = TargetGuid;
    }

    public override string Summary => $"{ChoiceList.Count} choice(s)";

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [SerializeField] private List<DialogueChoice> ChoiceList = new List<DialogueChoice>();
}

// One option inside a ChoiceNode: the text shown to the player and where picking it leads. The
// target is wired in the graph (hidden here); only the text is authored in the inspector.
[Serializable]
public class DialogueChoice
{
    public string Text       => ChoiceText;
    public string TargetGuid  { get => Target; set => Target = value; }

    [SerializeField] private string ChoiceText = "New choice";

    [SerializeField, HideInInspector] private string Target;
}

// Shared helper for the one-line body previews: collapses newlines and trims to a max length.
public static class DialoguePreview
{
    public static string Shorten(string Text, int MaxLength)
    {
        if (string.IsNullOrEmpty(Text))
        {
            return string.Empty;
        }

        string OneLine = Text.Replace('\n', ' ').Replace('\r', ' ');
        if (OneLine.Length <= MaxLength)
        {
            return OneLine;
        }
        return OneLine.Substring(0, MaxLength) + "…";
    }
}
