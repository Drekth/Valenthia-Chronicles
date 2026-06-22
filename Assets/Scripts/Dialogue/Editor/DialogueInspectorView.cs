using System;
using System.Collections.Generic;
using System.Reflection;
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

        // Draw the concrete node's own serialized fields. We resolve them by reflection rather than
        // SerializedProperty.hasVisibleChildren / NextVisible, which are unreliable on
        // [SerializeReference] managed references — they report "no visible children" even when the
        // concrete type has [SerializeField] fields. Base wiring fields are [HideInInspector] and so
        // are filtered out, leaving only the node's payload.
        object Boxed = Element.managedReferenceValue;
        List<string> FieldNames = Boxed != null ? GetInspectorFieldNames(Boxed.GetType()) : null;

        if (FieldNames == null || FieldNames.Count == 0)
        {
            EditorGUILayout.LabelField("This node has no editable fields.");
        }
        else
        {
            foreach (string FieldName in FieldNames)
            {
                SerializedProperty Child = Element.FindPropertyRelative(FieldName);
                if (Child != null)
                {
                    EditorGUILayout.PropertyField(Child, true);
                }
            }
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

    // Names of the serialized fields the concrete node type exposes to the inspector — every
    // instance field Unity serializes (public, or [SerializeField]) that is not [HideInInspector].
    // Walks the type hierarchy so inherited fields are included, and caches the result per type.
    private static List<string> GetInspectorFieldNames(Type NodeType)
    {
        if (FieldCache.TryGetValue(NodeType, out List<string> Cached))
        {
            return Cached;
        }

        List<Type> Chain = new List<Type>();
        for (Type Current = NodeType; Current != null && Current != typeof(object); Current = Current.BaseType)
        {
            Chain.Add(Current);
        }
        Chain.Reverse();

        List<string> Names = new List<string>();
        foreach (Type Level in Chain)
        {
            FieldInfo[] Fields = Level.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (FieldInfo Field in Fields)
            {
                if (IsInspectorField(Field))
                {
                    Names.Add(Field.Name);
                }
            }
        }

        FieldCache[NodeType] = Names;
        return Names;
    }

    private static bool IsInspectorField(FieldInfo Field)
    {
        if (Field.IsStatic || Attribute.IsDefined(Field, typeof(HideInInspector)))
        {
            return false;
        }

        if (Field.IsPublic)
        {
            return !Attribute.IsDefined(Field, typeof(NonSerializedAttribute));
        }
        return Attribute.IsDefined(Field, typeof(SerializeField));
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private const float  Padding        = 8.0f;
    private const string NodesFieldName = "GraphNodes";
    private const string GuidFieldName  = "NodeGuid";

    private static readonly Dictionary<Type, List<string>> FieldCache = new Dictionary<Type, List<string>>();

    private DialogueNodeView CurrentNode;
    private SerializedObject CurrentSerialized;
    private int              LastOutputCount;
}
