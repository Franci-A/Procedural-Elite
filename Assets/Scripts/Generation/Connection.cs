using System.Diagnostics;
using UnityEngine;


public class Connection
{
    private bool isLocked;
    private Node from;
    private Node to;

    public bool IsLocked { get => isLocked; }
    public Node From { get => from; }
    public Node To { get => to; }

    public Connection(Node nodeA, Node nodeB)
    {
        this.from = nodeA;
        this.to = nodeB;
        isLocked = false;
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }

    public Utils.ORIENTATION GetOrientation(Node node)
    {
        if (node == to)
        {
            return Utils.DirToOrientation(from.Position - to.Position);
        }
        else if (node != from)
        {
            UnityEngine.Debug.LogError($" Current node is id: {node.NodeId} but you try to connect node {from.NodeId}(from) and node {to.NodeId}(to)");
            throw new System.Exception("Can't get orientation for this node....");
        }

        return Utils.DirToOrientation(to.Position - from.Position);
    }
}
