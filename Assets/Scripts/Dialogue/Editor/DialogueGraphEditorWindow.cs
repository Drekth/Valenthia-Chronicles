using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// Editor window hosting the dialogue node-graph (Valenthia/Narrative/Dialogue Editor). Edits a
// DialogueData asset: load via the toolbar object field or by double-clicking the asset, author on
// the canvas, persist with Save. The right pane shows the selected node's serialized fields and
// asks for a port resync when an edit changes the node's output count.
public class DialogueGraphEditorWindow : EditorWindow
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    [MenuItem("Valenthia/Narrative/Dialogue Editor")]
    public static void Open()
    {
        DialogueGraphEditorWindow Window = GetWindow<DialogueGraphEditorWindow>();
        Window.titleContent = new GUIContent("Dialogue Editor");
        Window.minSize = new Vector2(800.0f, 500.0f);
    }

    [OnOpenAsset]
    public static bool OnOpenDialogueAsset(EntityId Id, int Line)
    {
        Object Target = EditorUtility.EntityIdToObject(Id);
        if (Target is DialogueData Dialogue)
        {
            Open();
            GetWindow<DialogueGraphEditorWindow>().Load(Dialogue);
            return true;
        }
        return false;
    }

    public void Load(DialogueData Dialogue)
    {
        ActiveAsset = Dialogue;
        SerializedAsset = Dialogue != null ? new SerializedObject(Dialogue) : null;

        if (AssetField != null)
        {
            AssetField.SetValueWithoutNotify(Dialogue);
        }
        if (GraphView != null)
        {
            GraphView.Load(Dialogue);
        }
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void CreateGUI()
    {
        Toolbar Bar = new Toolbar();

        AssetField = new ObjectField("Dialogue")
        {
            objectType = typeof(DialogueData),
            allowSceneObjects = false,
        };
        AssetField.RegisterValueChangedCallback(Event => Load(Event.newValue as DialogueData));

        ToolbarButton SaveButton = new ToolbarButton(Save) { text = "Save" };

        Bar.Add(AssetField);
        Bar.Add(SaveButton);

        TwoPaneSplitView Split = new TwoPaneSplitView(1, InspectorWidth, TwoPaneSplitViewOrientation.Horizontal);

        GraphView = new DialogueGraphView();
        GraphView.NodeSelectionChanged += HandleNodeSelectionChanged;

        Inspector = new DialogueInspectorView();
        Inspector.NodeStructureChanged += HandleNodeStructureChanged;
        Inspector.NodeContentChanged += HandleNodeContentChanged;

        Split.Add(GraphView);
        Split.Add(Inspector);

        rootVisualElement.Add(Bar);
        rootVisualElement.Add(Split);

        // Re-bind after a domain reload: ActiveAsset survives as a serialized field.
        if (ActiveAsset != null)
        {
            Load(ActiveAsset);
        }
    }

    private void OnDisable()
    {
        if (GraphView != null)
        {
            GraphView.NodeSelectionChanged -= HandleNodeSelectionChanged;
        }
        if (Inspector != null)
        {
            Inspector.NodeStructureChanged -= HandleNodeStructureChanged;
            Inspector.NodeContentChanged -= HandleNodeContentChanged;
        }
    }

    private void Save()
    {
        if (ActiveAsset == null)
        {
            return;
        }

        GraphView.SaveTo(ActiveAsset);
        EditorUtility.SetDirty(ActiveAsset);
        AssetDatabase.SaveAssets();
    }

    private void HandleNodeSelectionChanged(DialogueNodeView Node)
    {
        CurrentNodeView = Node;

        // Rebuild the SerializedObject on every selection so a freshly added (unsaved) node is
        // present in the serialized node list and can be found by Guid.
        if (ActiveAsset != null)
        {
            SerializedAsset = new SerializedObject(ActiveAsset);
        }

        Inspector.SetNode(Node, SerializedAsset);
    }

    private void HandleNodeStructureChanged()
    {
        DialogueNodeView Target = CurrentNodeView;
        if (Target == null)
        {
            return;
        }

        // Defer out of the inspector's IMGUI pass before mutating the graph's visual tree.
        EditorApplication.delayCall += () =>
        {
            if (GraphView != null && Target != null)
            {
                GraphView.RefreshNodePorts(Target);
            }
        };
    }

    private void HandleNodeContentChanged()
    {
        if (CurrentNodeView != null)
        {
            CurrentNodeView.RefreshContent();
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private const float InspectorWidth = 280.0f;

    [SerializeField] private DialogueData ActiveAsset;

    private SerializedObject      SerializedAsset;
    private DialogueGraphView     GraphView;
    private DialogueInspectorView Inspector;
    private ObjectField           AssetField;
    private DialogueNodeView      CurrentNodeView;
}
