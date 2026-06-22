using System.Collections.Generic;
using UnityEngine;

// Runtime traversal of a DialogueData graph. ServiceLocator-registered service living on the
// persistent player rig. Fully decoupled from the UI: it raises dialogue events and waits for the
// panel to call back (Advance / SubmitChoice), so there is no coroutine tied to a GameObject
// lifetime — the conversation state is held between events in plain fields.
//
// Node types are interpreted here (the nodes themselves are pure data, addressed by Guid):
//   StartNode / ConditionNode resolve immediately and step to the next node;
//   LineNode and ChoiceNode pause the walk after raising their event;
//   EndNode (or a dangling target) ends the conversation.
//
// ConditionNode always takes its True branch for now — the shared WorldState/INarrativeCondition
// layer that would evaluate it does not exist yet.
public class DialogueRunner : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    // Safety bound on a single resolve pass, so a malformed graph (e.g. a condition cycle) can
    // never spin forever between two paused nodes.
    private const int MaxResolveSteps = 256;

    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public bool IsActive => InDialogue;

    // Begins walking the given graph. NpcName titles the panel before the first line is shown.
    public void StartDialogue(DialogueData Dialogue, string NpcName)
    {
        if (Dialogue == null || InDialogue)
        {
            return;
        }

        Active     = Dialogue;
        InDialogue = true;

        NodesByGuid.Clear();
        foreach (DialogueNode Node in Dialogue.Nodes)
        {
            if (Node != null && !string.IsNullOrEmpty(Node.Guid))
            {
                NodesByGuid[Node.Guid] = Node;
            }
        }

        // Announce first so the panel shows (and clears) before the opening line arrives.
        EventBus<DialogueStartedEvent>.Raise(new DialogueStartedEvent
        {
            DialogueId = Dialogue.Id,
            NpcName    = NpcName,
        });

        ResolveFrom(FindEntryNode(Dialogue));
    }

    // Continues past the current LineNode (the panel's "Continue" button).
    public void Advance()
    {
        if (!InDialogue)
        {
            return;
        }

        // Both the greeting and a spoken line advance through their single "Next" output.
        if (CurrentNode is LineNode || CurrentNode is StartNode)
        {
            ResolveFrom(GetNode(CurrentNode.GetOutputTarget(0)));
        }
    }

    // Routes the flow through the chosen option of the current ChoiceNode, or the accept/decline of
    // a paused quest node (0 = accept / complete, 1 = decline / later). The same button channel as
    // a choice, so the panel needs no extra callback.
    public void SubmitChoice(int Index)
    {
        if (!InDialogue)
        {
            return;
        }

        if (CurrentNode is ChoiceNode Choice)
        {
            ResolveFrom(GetNode(Choice.GetOutputTarget(Index)));
            return;
        }

        if (CurrentNode is QuestOfferNode Offer)
        {
            if (Index == 0)
            {
                IQuestManager Quests = ResolveQuests();
                if (Quests != null)
                {
                    Quests.StartQuest(Offer.Quest);
                }
            }
            ResolveFrom(GetNode(Offer.GetOutputTarget(Index)));
            return;
        }

        if (CurrentNode is QuestTurnInNode TurnIn)
        {
            if (Index == 0 && TurnIn.Quest != null)
            {
                IQuestManager Quests = ResolveQuests();
                if (Quests != null)
                {
                    Quests.CompleteQuest(TurnIn.Quest.Id);
                }
            }
            ResolveFrom(GetNode(TurnIn.GetOutputTarget(Index)));
            return;
        }
    }

    // Cancels an in-progress conversation (the panel's close button).
    public void Abort()
    {
        if (InDialogue)
        {
            EndDialogue();
        }
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        ServiceLocator.Register<DialogueRunner>(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<DialogueRunner>();
    }

    // Walks the graph until it reaches a node that needs player input (Line / Choice) or the end.
    private void ResolveFrom(DialogueNode Node)
    {
        int Steps = 0;
        while (Node != null)
        {
            Steps++;
            if (Steps > MaxResolveSteps)
            {
                Debug.LogWarning("[DialogueRunner] Resolve step limit reached — aborting (graph cycle?).");
                break;
            }

            if (Node is StartNode Start)
            {
                // An authored greeting is shown like a line (text + advance button); an empty
                // greeting is a pure pass-through entry, stepping straight to the next node.
                if (string.IsNullOrEmpty(Start.Text))
                {
                    Node = GetNode(Start.GetOutputTarget(0));
                    continue;
                }

                CurrentNode = Start;

                DialogueNode AfterGreeting = GetNode(Start.GetOutputTarget(0));
                bool GreetingIsFinal = AfterGreeting == null || AfterGreeting is EndNode;

                EventBus<DialogueLineShownEvent>.Raise(new DialogueLineShownEvent
                {
                    Speaker = string.Empty,
                    Text    = Start.Text,
                    IsFinal = GreetingIsFinal,
                });
                return;
            }

            if (Node is ConditionNode Condition)
            {
                // TODO WorldState: evaluate the condition; always take the True branch (output 0) for now.
                Node = GetNode(Condition.GetOutputTarget(0));
                continue;
            }

            if (Node is QuestOfferNode Offer)
            {
                IQuestManager Quests = ResolveQuests();
                if (Quests != null && Quests.CanOffer(Offer.Quest))
                {
                    CurrentNode = Offer;
                    EventBus<DialogueQuestOfferedEvent>.Raise(BuildQuestOffer(Offer.Quest, false));
                    return;
                }

                // Nothing to offer (already taken / completed / unavailable) — take the Declined path.
                Node = GetNode(Offer.GetOutputTarget(1));
                continue;
            }

            if (Node is QuestTurnInNode TurnIn)
            {
                IQuestManager Quests = ResolveQuests();
                if (Quests != null && TurnIn.Quest != null && Quests.IsReadyToTurnIn(TurnIn.Quest.Id))
                {
                    CurrentNode = TurnIn;
                    EventBus<DialogueQuestOfferedEvent>.Raise(BuildQuestOffer(TurnIn.Quest, true));
                    return;
                }

                // Not ready to hand in — take the NotReady path.
                Node = GetNode(TurnIn.GetOutputTarget(1));
                continue;
            }

            if (Node is LineNode Line)
            {
                CurrentNode = Line;

                DialogueNode Next = GetNode(Line.GetOutputTarget(0));
                bool IsFinal = Next == null || Next is EndNode;

                EventBus<DialogueLineShownEvent>.Raise(new DialogueLineShownEvent
                {
                    Speaker = Line.Speaker,
                    Text    = Line.Text,
                    IsFinal = IsFinal,
                });
                return;
            }

            if (Node is ChoiceNode Choice)
            {
                CurrentNode = Choice;

                EventBus<DialogueChoicesShownEvent>.Raise(new DialogueChoicesShownEvent
                {
                    Options = BuildOptions(Choice),
                });
                return;
            }

            // EndNode or any other terminal node closes the conversation.
            break;
        }

        EndDialogue();
    }

    private static DialogueOption[] BuildOptions(ChoiceNode Choice)
    {
        IReadOnlyList<DialogueChoice> List = Choice.Choices;
        DialogueOption[] Options = new DialogueOption[List.Count];
        for (int Index = 0; Index < List.Count; Index++)
        {
            Options[Index] = new DialogueOption
            {
                Index = Index,
                Text  = List[Index].Text,
            };
        }
        return Options;
    }

    private static IQuestManager ResolveQuests()
    {
        ServiceLocator.TryGet<IQuestManager>(out IQuestManager Manager);
        return Manager;
    }

    // Builds the panel payload for a quest offer or hand-in: objective lines (offers only) and
    // reward lines, as plain strings so the UI never touches QuestData. Runs on a discrete offer
    // event, never per frame.
    private static DialogueQuestOfferedEvent BuildQuestOffer(QuestData Quest, bool IsTurnIn)
    {
        return new DialogueQuestOfferedEvent
        {
            QuestTitle = Quest.Title,
            Summary    = Quest.Summary,
            Objectives = BuildObjectiveLines(Quest, IsTurnIn),
            Rewards    = BuildRewardLines(Quest),
            IsTurnIn   = IsTurnIn,
        };
    }

    private static string[] BuildObjectiveLines(QuestData Quest, bool IsTurnIn)
    {
        if (IsTurnIn || Quest.Stages.Count == 0)
        {
            return System.Array.Empty<string>();
        }

        IReadOnlyList<QuestObjective> Objectives = Quest.Stages[0].Objectives;
        string[] Lines = new string[Objectives.Count];
        for (int Index = 0; Index < Objectives.Count; Index++)
        {
            Lines[Index] = Objectives[Index].Describe();
        }
        return Lines;
    }

    private static string[] BuildRewardLines(QuestData Quest)
    {
        IReadOnlyList<QuestReward> Rewards = Quest.Rewards;
        if (Rewards == null || Rewards.Count == 0)
        {
            return System.Array.Empty<string>();
        }

        List<string> Lines = new List<string>(Rewards.Count);
        for (int Index = 0; Index < Rewards.Count; Index++)
        {
            string Line = Rewards[Index].Describe();
            if (!string.IsNullOrEmpty(Line))
            {
                Lines.Add(Line);
            }
        }
        return Lines.ToArray();
    }

    // The configured entry node by Guid, falling back to the first StartNode in the graph.
    private DialogueNode FindEntryNode(DialogueData Dialogue)
    {
        if (!string.IsNullOrEmpty(Dialogue.EntryGuid) && NodesByGuid.TryGetValue(Dialogue.EntryGuid, out DialogueNode Entry))
        {
            return Entry;
        }

        foreach (DialogueNode Node in Dialogue.Nodes)
        {
            if (Node is StartNode)
            {
                return Node;
            }
        }
        return null;
    }

    private DialogueNode GetNode(string Guid)
    {
        if (string.IsNullOrEmpty(Guid))
        {
            return null;
        }

        NodesByGuid.TryGetValue(Guid, out DialogueNode Node);
        return Node;
    }

    private void EndDialogue()
    {
        Active      = null;
        CurrentNode = null;
        InDialogue  = false;
        NodesByGuid.Clear();

        EventBus<DialogueEndedEvent>.Raise(new DialogueEndedEvent());
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private readonly Dictionary<string, DialogueNode> NodesByGuid = new Dictionary<string, DialogueNode>();

    private DialogueData Active;
    private DialogueNode CurrentNode;
    private bool         InDialogue;
}
