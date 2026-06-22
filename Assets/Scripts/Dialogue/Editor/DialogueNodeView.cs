using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

// Visual representation of one DialogueNode on the canvas. Backed by the serialized node data it
// edits in place. Builds its ports from the node (input port — named by the node — unless it refuses
// input, plus one named output port per output slot) and shows a one-line content preview in its
// body. RebuildOutputPorts re-syncs the output ports after the node's shape changes; RefreshContent
// re-reads the body preview after the node's fields change.
public class DialogueNodeView : Node
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public DialogueNode        Data        => NodeData;
    public Port                InputPort   => Input;       // null when the node accepts no input
    public IReadOnlyList<Port> OutputPorts => Outputs;

    public DialogueNodeView(DialogueNode Node)
    {
        NodeData = Node;
        title = Node.DisplayName;
        SetPosition(new Rect(Node.EditorPosition, DefaultSize));

        if (Node.AcceptsInput)
        {
            Input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            Input.portName = Node.InputLabel;
            inputContainer.Add(Input);
        }

        BuildOutputPorts();

        Summary = new Label();
        Summary.style.whiteSpace = WhiteSpace.Normal;
        Summary.style.marginTop = 4.0f;
        Summary.style.marginBottom = 4.0f;
        Summary.style.marginLeft = 6.0f;
        Summary.style.marginRight = 6.0f;
        Summary.style.maxWidth = 190.0f;
        extensionContainer.Add(Summary);

        RefreshContent();
        RefreshExpandedState();
        RefreshPorts();

        // Positions are owned by the graph's automatic layout — the node is not hand-movable.
        capabilities &= ~Capabilities.Movable;
    }

    public void RebuildOutputPorts()
    {
        foreach (Port Existing in Outputs)
        {
            outputContainer.Remove(Existing);
        }
        Outputs.Clear();

        BuildOutputPorts();

        RefreshExpandedState();
        RefreshPorts();
    }

    public void RefreshContent()
    {
        Summary.text = NodeData.Summary;
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void BuildOutputPorts()
    {
        for (int Index = 0; Index < NodeData.OutputCount; Index++)
        {
            Port Output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            Output.portName = NodeData.GetOutputLabel(Index);
            outputContainer.Add(Output);
            Outputs.Add(Output);
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private static readonly Vector2 DefaultSize = new Vector2(220.0f, 120.0f);

    private readonly DialogueNode NodeData;
    private readonly List<Port>   Outputs = new List<Port>();

    private Port  Input;
    private Label Summary;
}
