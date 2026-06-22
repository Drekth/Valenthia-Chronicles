using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

// The graph canvas: pan/zoom/selection, a grid, a reflection-driven "add node" menu, and load/save
// against a DialogueData asset. Nodes expose output ports by index; edges are mapped to per-port
// target Guids. RefreshNodePorts re-syncs a single node's ports (preserving its wiring) after its
// shape changed. Raises NodeSelectionChanged so the host window can drive the side inspector.
public class DialogueGraphView : GraphView
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public event Action<DialogueNodeView> NodeSelectionChanged;

    public DialogueGraphView()
    {
        style.flexGrow = 1.0f;

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        // Left-drag pans the canvas (custom manipulator — ContentDragger captures every left-down,
        // which swallowed node selection). Panning starts only on the empty canvas, so a click on a
        // node still selects it. Node dragging itself is disabled via the node's capabilities.
        this.AddManipulator(new DialogueGraphPanner());

        GridBackground Grid = new GridBackground();
        Grid.StretchToParentSize();
        Insert(0, Grid);

        graphViewChanged = OnGraphViewChanged;

        // Delete / Backspace removes the selection. TrickleDown so the graph sees the key whichever
        // descendant holds focus; focusable so the graph can hold focus itself.
        focusable = true;
        RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
    }

    public void Load(DialogueData Dialogue)
    {
        CurrentAsset = Dialogue;

        DeleteElements(graphElements.ToList());
        NodeViews.Clear();

        if (Dialogue == null)
        {
            return;
        }

        foreach (DialogueNode Node in Dialogue.Nodes)
        {
            CreateNodeView(Node);
        }

        foreach (DialogueNodeView View in NodeViews)
        {
            ReconnectNodeOutputs(View);
        }

        ApplyAutoLayout();
    }

    public void SaveTo(DialogueData Dialogue)
    {
        if (Dialogue == null)
        {
            return;
        }

        foreach (DialogueNodeView View in NodeViews)
        {
            View.Data.EditorPosition = View.GetPosition().position;
        }

        SyncEdgesToData();
        Dialogue.SetEntryGuid(ResolveEntryGuid(Dialogue));
    }

    // Rebuilds one node's output ports after its shape changed, preserving its current visual wiring
    // by output index (so unsaved edges survive). Selection and other nodes are left untouched.
    public void RefreshNodePorts(DialogueNodeView View)
    {
        if (View == null || !NodeViews.Contains(View))
        {
            return;
        }

        Dictionary<int, string> PreservedTargets = new Dictionary<int, string>();
        List<Edge> Outgoing = new List<Edge>();
        foreach (Edge Connection in edges.ToList())
        {
            if (Connection.output == null || Connection.output.node != View)
            {
                continue;
            }

            int OldIndex = IndexOfOutput(View, Connection.output);
            DialogueNodeView To = Connection.input.node as DialogueNodeView;
            if (OldIndex >= 0 && To != null)
            {
                PreservedTargets[OldIndex] = To.Data.Guid;
            }
            Outgoing.Add(Connection);
        }

        foreach (Edge Connection in Outgoing)
        {
            Connection.input?.Disconnect(Connection);
            Connection.output.Disconnect(Connection);
            RemoveElement(Connection);
        }

        View.RebuildOutputPorts();

        for (int Index = 0; Index < View.OutputPorts.Count; Index++)
        {
            if (!PreservedTargets.TryGetValue(Index, out string TargetGuid))
            {
                continue;
            }

            DialogueNodeView Target = FindByGuid(TargetGuid);
            if (Target == null || Target.InputPort == null)
            {
                continue;
            }

            Edge Connection = View.OutputPorts[Index].ConnectTo(Target.InputPort);
            AddElement(Connection);
        }

        ScheduleLayout();
    }

    public override List<Port> GetCompatiblePorts(Port StartPort, NodeAdapter Adapter)
    {
        List<Port> Compatible = new List<Port>();
        foreach (Port Candidate in ports.ToList())
        {
            if (Candidate == StartPort)
            {
                continue;
            }
            if (Candidate.node == StartPort.node)
            {
                continue;
            }
            if (Candidate.direction == StartPort.direction)
            {
                continue;
            }
            Compatible.Add(Candidate);
        }
        return Compatible;
    }

    public override void AddToSelection(ISelectable Selectable)
    {
        base.AddToSelection(Selectable);
        NotifySelection();
    }

    public override void RemoveFromSelection(ISelectable Selectable)
    {
        base.RemoveFromSelection(Selectable);
        NotifySelection();
    }

    public override void ClearSelection()
    {
        base.ClearSelection();
        NotifySelection();
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent Event)
    {
        if (CurrentAsset == null)
        {
            Event.menu.AppendAction("Assign a Dialogue asset first", null, DropdownMenuAction.Status.Disabled);
            return;
        }

        Vector2 SpawnPosition = contentViewContainer.WorldToLocal(Event.mousePosition);
        foreach (Type NodeType in NodeTypes)
        {
            Type Captured = NodeType;
            Event.menu.AppendAction($"Add {NodeDisplayName(Captured)}", Action => CreateNode(Captured, SpawnPosition));
        }

        Event.menu.AppendSeparator();
        DropdownMenuAction.Status DeleteStatus = FirstSelectedNode() != null
            ? DropdownMenuAction.Status.Normal
            : DropdownMenuAction.Status.Disabled;
        Event.menu.AppendAction("Delete", Action => DeleteSelectedNodes(), DeleteStatus);
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void CreateNode(Type NodeType, Vector2 Position)
    {
        DialogueNode Node = Activator.CreateInstance(NodeType) as DialogueNode;
        Node.Guid = System.Guid.NewGuid().ToString();
        Node.EditorPosition = Position;

        CurrentAsset.AddNode(Node);

        DialogueNodeView View = CreateNodeView(Node);

        AutoConnectFromSelection(View);

        EditorUtility.SetDirty(CurrentAsset);

        // Select the new node so the next add chains from it.
        ClearSelection();
        AddToSelection(View);

        ScheduleLayout();
    }

    private DialogueNodeView CreateNodeView(DialogueNode Node)
    {
        DialogueNodeView View = new DialogueNodeView(Node);
        AddElement(View);
        NodeViews.Add(View);
        return View;
    }

    private void ReconnectNodeOutputs(DialogueNodeView View)
    {
        for (int Index = 0; Index < View.OutputPorts.Count; Index++)
        {
            string TargetGuid = View.Data.GetOutputTarget(Index);
            if (string.IsNullOrEmpty(TargetGuid))
            {
                continue;
            }

            DialogueNodeView Target = FindByGuid(TargetGuid);
            if (Target == null || Target.InputPort == null)
            {
                continue;
            }

            Edge Connection = View.OutputPorts[Index].ConnectTo(Target.InputPort);
            AddElement(Connection);
        }
    }

    private void SyncEdgesToData()
    {
        foreach (DialogueNodeView View in NodeViews)
        {
            for (int Index = 0; Index < View.Data.OutputCount; Index++)
            {
                View.Data.SetOutputTarget(Index, string.Empty);
            }
        }

        foreach (Edge Connection in edges.ToList())
        {
            DialogueNodeView From = Connection.output.node as DialogueNodeView;
            DialogueNodeView To   = Connection.input.node as DialogueNodeView;
            if (From == null || To == null)
            {
                continue;
            }

            int PortIndex = IndexOfOutput(From, Connection.output);
            if (PortIndex < 0)
            {
                continue;
            }

            From.Data.SetOutputTarget(PortIndex, To.Data.Guid);
        }
    }

    private void ScheduleLayout()
    {
        // Defer to next frame so edges from edgesToCreate are committed before we read them.
        schedule.Execute(() =>
        {
            if (CurrentAsset == null)
            {
                return;
            }
            SyncEdgesToData();
            ApplyAutoLayout();
            EditorUtility.SetDirty(CurrentAsset);
        });
    }

    // Arranges nodes into stages: column index = longest-path depth from the entry, nodes within a
    // stage stacked and the stage centred vertically. Positions are fully owned here.
    private void ApplyAutoLayout()
    {
        if (NodeViews.Count == 0)
        {
            return;
        }

        DialogueNodeView Entry = FindEntryView();

        Dictionary<string, int> Layers = new Dictionary<string, int>();
        Dictionary<int, List<DialogueNodeView>> Columns = new Dictionary<int, List<DialogueNodeView>>();
        HashSet<string> Seen = new HashSet<string>();

        if (Entry != null)
        {
            ComputeLayers(Entry, 0, Layers, new HashSet<string>());
            ComputeOrder(Entry, Layers, Columns, Seen);
        }

        // Nodes not reachable from the entry: park them in column 0.
        foreach (DialogueNodeView View in NodeViews)
        {
            if (!Seen.Contains(View.Data.Guid))
            {
                Seen.Add(View.Data.Guid);
                AddToColumn(Columns, 0, View);
            }
        }

        int MaxRows = 1;
        foreach (List<DialogueNodeView> Column in Columns.Values)
        {
            if (Column.Count > MaxRows)
            {
                MaxRows = Column.Count;
            }
        }

        foreach (KeyValuePair<int, List<DialogueNodeView>> Column in Columns)
        {
            List<DialogueNodeView> Nodes = Column.Value;
            float StartY = OriginY + (MaxRows - Nodes.Count) * RowSpacing * 0.5f;
            for (int Row = 0; Row < Nodes.Count; Row++)
            {
                DialogueNodeView View = Nodes[Row];
                float X = OriginX + Column.Key * ColumnSpacing;
                float Y = StartY + Row * RowSpacing;
                Rect CurrentRect = View.GetPosition();
                View.SetPosition(new Rect(X, Y, CurrentRect.width, CurrentRect.height));
                View.Data.EditorPosition = new Vector2(X, Y);
            }
        }
    }

    private void ComputeLayers(DialogueNodeView View, int Depth, Dictionary<string, int> Layers, HashSet<string> Stack)
    {
        string Guid = View.Data.Guid;
        if (Stack.Contains(Guid))
        {
            return;
        }
        if (Layers.TryGetValue(Guid, out int Existing) && Existing >= Depth)
        {
            return;
        }

        Layers[Guid] = Depth;

        Stack.Add(Guid);
        for (int Index = 0; Index < View.Data.OutputCount; Index++)
        {
            DialogueNodeView Child = FindByGuid(View.Data.GetOutputTarget(Index));
            if (Child != null)
            {
                ComputeLayers(Child, Depth + 1, Layers, Stack);
            }
        }
        Stack.Remove(Guid);
    }

    private void ComputeOrder(DialogueNodeView View, Dictionary<string, int> Layers, Dictionary<int, List<DialogueNodeView>> Columns, HashSet<string> Seen)
    {
        string Guid = View.Data.Guid;
        if (Seen.Contains(Guid))
        {
            return;
        }
        Seen.Add(Guid);

        int Layer = Layers.TryGetValue(Guid, out int Value) ? Value : 0;
        AddToColumn(Columns, Layer, View);

        for (int Index = 0; Index < View.Data.OutputCount; Index++)
        {
            DialogueNodeView Child = FindByGuid(View.Data.GetOutputTarget(Index));
            if (Child != null)
            {
                ComputeOrder(Child, Layers, Columns, Seen);
            }
        }
    }

    private DialogueNodeView FindEntryView()
    {
        foreach (DialogueNodeView View in NodeViews)
        {
            if (View.Data is StartNode)
            {
                return View;
            }
        }
        if (NodeViews.Count > 0)
        {
            return NodeViews[0];
        }
        return null;
    }

    private DialogueNodeView FirstSelectedNode()
    {
        foreach (ISelectable Item in selection)
        {
            if (Item is DialogueNodeView View)
            {
                return View;
            }
        }
        return null;
    }

    private void AutoConnectFromSelection(DialogueNodeView NewView)
    {
        if (NewView.InputPort == null)
        {
            return;
        }

        DialogueNodeView Source = ResolveConnectionSource(NewView);
        if (Source == null)
        {
            return;
        }

        Port FreeOutput = FirstFreeOutput(Source);
        if (FreeOutput == null)
        {
            return;
        }

        Edge Connection = FreeOutput.ConnectTo(NewView.InputPort);
        AddElement(Connection);
    }

    // The node a freshly added node attaches to: the selected node if any, otherwise the rightmost
    // node with a free output (the current tail), so adding without a selection extends the last stage.
    private DialogueNodeView ResolveConnectionSource(DialogueNodeView NewView)
    {
        DialogueNodeView Selected = FirstSelectedNode();
        if (Selected != null)
        {
            return Selected;
        }

        DialogueNodeView Tail = null;
        float TailX = float.NegativeInfinity;
        foreach (DialogueNodeView View in NodeViews)
        {
            if (View == NewView)
            {
                continue;
            }
            if (FirstFreeOutput(View) == null)
            {
                continue;
            }

            float X = View.GetPosition().x;
            if (X >= TailX)
            {
                TailX = X;
                Tail = View;
            }
        }
        return Tail;
    }

    private static void AddToColumn(Dictionary<int, List<DialogueNodeView>> Columns, int Layer, DialogueNodeView View)
    {
        if (!Columns.TryGetValue(Layer, out List<DialogueNodeView> Column))
        {
            Column = new List<DialogueNodeView>();
            Columns[Layer] = Column;
        }
        Column.Add(View);
    }

    private static Port FirstFreeOutput(DialogueNodeView View)
    {
        foreach (Port Output in View.OutputPorts)
        {
            if (!HasConnection(Output))
            {
                return Output;
            }
        }
        return null;
    }

    private static bool HasConnection(Port Output)
    {
        foreach (Edge Connection in Output.connections)
        {
            return true;
        }
        return false;
    }

    private DialogueNodeView FindByGuid(string Guid)
    {
        foreach (DialogueNodeView View in NodeViews)
        {
            if (View.Data.Guid == Guid)
            {
                return View;
            }
        }
        return null;
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange Change)
    {
        bool Structural = false;

        if (Change.elementsToRemove != null && CurrentAsset != null)
        {
            foreach (GraphElement Element in Change.elementsToRemove)
            {
                if (Element is DialogueNodeView View)
                {
                    CurrentAsset.RemoveNode(View.Data);
                    NodeViews.Remove(View);
                    Structural = true;
                }
                else if (Element is Edge)
                {
                    Structural = true;
                }
            }
        }

        if (Change.edgesToCreate != null && Change.edgesToCreate.Count > 0)
        {
            Structural = true;
        }

        if (Structural && CurrentAsset != null)
        {
            EditorUtility.SetDirty(CurrentAsset);
            ScheduleLayout();
        }

        return Change;
    }

    private void OnKeyDown(KeyDownEvent Event)
    {
        if (Event.keyCode == KeyCode.Delete || Event.keyCode == KeyCode.Backspace)
        {
            DeleteSelectedNodes();
            Event.StopPropagation();
        }
    }

    private void DeleteSelectedNodes()
    {
        if (CurrentAsset == null)
        {
            return;
        }

        List<DialogueNodeView> ToDelete = new List<DialogueNodeView>();
        foreach (ISelectable Item in selection)
        {
            if (Item is DialogueNodeView View)
            {
                ToDelete.Add(View);
            }
        }

        if (ToDelete.Count == 0)
        {
            return;
        }

        foreach (DialogueNodeView View in ToDelete)
        {
            RemoveNodeAndEdges(View);
        }

        EditorUtility.SetDirty(CurrentAsset);
        ScheduleLayout();
    }

    private void RemoveNodeAndEdges(DialogueNodeView View)
    {
        foreach (Edge Connection in edges.ToList())
        {
            bool Touches = (Connection.output != null && Connection.output.node == View)
                        || (Connection.input != null && Connection.input.node == View);
            if (!Touches)
            {
                continue;
            }

            Connection.input?.Disconnect(Connection);
            Connection.output?.Disconnect(Connection);
            RemoveElement(Connection);
        }

        CurrentAsset.RemoveNode(View.Data);
        NodeViews.Remove(View);
        RemoveElement(View);
    }

    private void NotifySelection()
    {
        DialogueNodeView Selected = null;
        foreach (ISelectable Item in selection)
        {
            if (Item is DialogueNodeView View)
            {
                Selected = View;
                break;
            }
        }
        NodeSelectionChanged?.Invoke(Selected);
    }

    private static string NodeDisplayName(Type NodeType)
    {
        DialogueNode Temp = Activator.CreateInstance(NodeType) as DialogueNode;
        return Temp != null ? Temp.DisplayName : NodeType.Name;
    }

    private static int IndexOfOutput(DialogueNodeView View, Port Output)
    {
        for (int Index = 0; Index < View.OutputPorts.Count; Index++)
        {
            if (View.OutputPorts[Index] == Output)
            {
                return Index;
            }
        }
        return -1;
    }

    private static string ResolveEntryGuid(DialogueData Dialogue)
    {
        foreach (DialogueNode Node in Dialogue.Nodes)
        {
            if (Node is StartNode)
            {
                return Node.Guid;
            }
        }

        if (Dialogue.Nodes.Count > 0)
        {
            return Dialogue.Nodes[0].Guid;
        }
        return string.Empty;
    }

    private static List<Type> BuildNodeTypes()
    {
        List<Type> Types = new List<Type>();
        foreach (Type Candidate in TypeCache.GetTypesDerivedFrom<DialogueNode>())
        {
            if (!Candidate.IsAbstract)
            {
                Types.Add(Candidate);
            }
        }
        return Types;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private const float ColumnSpacing = 340.0f;
    private const float RowSpacing    = 200.0f;
    private const float OriginX       = 80.0f;
    private const float OriginY       = 80.0f;

    private static readonly List<Type> NodeTypes = BuildNodeTypes();

    private readonly List<DialogueNodeView> NodeViews = new List<DialogueNodeView>();

    private DialogueData CurrentAsset;
}

// Left-mouse drag panning for the dialogue graph. Replaces ContentDragger so a plain click on a node
// still selects it: panning only begins when the press lands on the empty canvas (not a node, port
// or edge). The view is moved through GraphView.UpdateViewTransform.
public class DialogueGraphPanner : MouseManipulator
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public DialogueGraphPanner()
    {
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
    }

    ////////////////////////////////////////////////////////////
    /// Protected                                            ///
    ////////////////////////////////////////////////////////////

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        target.RegisterCallback<MouseUpEvent>(OnMouseUp);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void OnMouseDown(MouseDownEvent Event)
    {
        if (Panning)
        {
            return;
        }
        if (IsOnGraphElement(Event.target as VisualElement))
        {
            return;
        }
        if (!CanStartManipulation(Event))
        {
            return;
        }

        Panning = true;
        target.CaptureMouse();
    }

    private void OnMouseMove(MouseMoveEvent Event)
    {
        if (!Panning)
        {
            return;
        }

        GraphView View = target as GraphView;
        if (View == null)
        {
            return;
        }

        // GraphView's view transform is still read/written through ITransform here — that is the
        // pair UpdateViewTransform expects, despite the UI Toolkit-wide deprecation of these members.
#pragma warning disable 618
        Vector3 NewPosition = View.viewTransform.position + new Vector3(Event.mouseDelta.x, Event.mouseDelta.y, 0.0f);
        View.UpdateViewTransform(NewPosition, View.viewTransform.scale);
#pragma warning restore 618
        Event.StopPropagation();
    }

    private void OnMouseUp(MouseUpEvent Event)
    {
        if (!Panning)
        {
            return;
        }
        if (!CanStopManipulation(Event))
        {
            return;
        }

        Panning = false;
        target.ReleaseMouse();
        Event.StopPropagation();
    }

    private static bool IsOnGraphElement(VisualElement Element)
    {
        while (Element != null)
        {
            if (Element is Node || Element is Port || Element is Edge)
            {
                return true;
            }
            Element = Element.parent;
        }
        return false;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private bool Panning;
}
