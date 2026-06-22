using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// Right-hand panel of the dialogue editor. On selection it renders the chosen node's serialized
// fields via IMGUI (reliable for [SerializeReference]). When an edit changes the node's output count
// (e.g. a ChoiceNode gains or loses a choice), it raises NodeStructureChanged so the host can resync
// that node's ports. The node is re-found by Guid every repaint, so the view self-heals.
public class DialogueInspectorView : VisualElement
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public event Action NodeStructureChanged;
    public event Action NodeContentChanged;

    public DialogueInspectorView()
    {
        style.paddingTop = Padding;
        style.paddingRight = Padding;
        style.paddingBottom = Padding;
        style.paddingLeft = Padding;

        SetNode(null, null);
    }

    public void SetNode(DialogueNodeView Node, SerializedObject SerializedAsset)
    {
        Clear();

        CurrentNode = Node;
        CurrentSerialized = SerializedAsset;
        LastOutputCount = Node != null ? Node.Data.OutputCount : 0;

        if (Node == null || SerializedAsset == null)
        {
            Add(new Label("No node selected."));
            return;
        }

        Label Title = new Label(Node.Data.DisplayName);
        Title.style.unityFontStyleAndWeight = FontStyle.Bold;
        Title.style.marginBottom = 6.0f;
        Add(Title);

        Add(new IMGUIContainer(DrawBody));
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void DrawBody()
    {
        if (CurrentNode == null || CurrentSerialized == null || CurrentSerialized.targetObject == null)
        {
            return;
        }

        CurrentSerialized.Update();

        SerializedProperty Element = FindNodeProperty(CurrentSerialized, CurrentNode.Data.Guid);
        if (Element == null)
        {
            EditorGUILayout.LabelField("Node not in asset — Save it.");
            return;
        }

        // PropertyField handles the managed-reference descent; base wiring fields are
        // [HideInInspector], leaving only the concrete node's payload.
        if (Element.hasVisibleChildren)
        {
            Element.isExpanded = true;
            EditorGUILayout.PropertyField(Element, GUIContent.none, true);
        }
        else
        {
            EditorGUILayout.LabelField("This node has no editable fields.");
        }

        bool Changed = CurrentSerialized.ApplyModifiedProperties();
        if (Changed)
        {
            NodeContentChanged?.Invoke();
            if (CurrentNode.Data.OutputCount != LastOutputCount)
            {
                LastOutputCount = CurrentNode.Data.OutputCount;
                NodeStructureChanged?.Invoke();
            }
        }
    }

    private static SerializedProperty FindNodeProperty(SerializedObject SerializedAsset, string Guid)
    {
        SerializedProperty NodesProperty = SerializedAsset.FindProperty(NodesFieldName);
        if (NodesProperty == null)
        {
            return null;
        }

        for (int Index = 0; Index < NodesProperty.arraySize; Index++)
        {
            SerializedProperty Element = NodesProperty.GetArrayElementAtIndex(Index);
            SerializedProperty GuidProperty = Element.FindPropertyRelative(GuidFieldName);
            if (GuidProperty != null && GuidProperty.stringValue == Guid)
            {
                return Element;
            }
        }
        return null;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private const float  Padding        = 8.0f;
    private const string NodesFieldName = "GraphNodes";
    private const string GuidFieldName  = "NodeGuid";

    private DialogueNodeView CurrentNode;
    private SerializedObject CurrentSerialized;
    private int              LastOutputCount;
}
