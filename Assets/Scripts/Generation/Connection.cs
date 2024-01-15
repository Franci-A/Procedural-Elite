using UnityEngine;

public class Connection
{
    private Node from;
    private Node to;
    private bool isLocked;
    private bool isSecret;

    public Node From { get => from; }
    public Node To { get => to; }
    public bool IsLocked { get => isLocked; }
    public bool IsSecret { get => isSecret; }

    public Connection(Node nodeA, Node nodeB)
    {
        this.from = nodeA;
        this.to = nodeB;
        isLocked = false;
        isSecret = false;
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }

    public void SetSecret(bool secret)
    {
        isSecret = secret;
    }

    public Utils.ORIENTATION GetOrientation(Node node)
    {
        if (node == to)
        {
            return Utils.DirToOrientation(from.Position - to.Position);
        }
        else if (node != from)
        {
            Debug.LogError($" Current node is id: {node.NodeId} but you try to connect node {from.NodeId}(from) and node {to.NodeId}(to)");
            throw new System.Exception("Can't get orientation for this node....");
        }

        return Utils.DirToOrientation(to.Position - from.Position);
    }
}
