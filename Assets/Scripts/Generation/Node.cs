using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public static int CurrentNodeID = -1;
    public static void ResetID() => CurrentNodeID = -1;

    // up to 4
    private List<Connection> connections;

    private int nodeId;
    private int doorCount;
    private RoomType type;
    private Vector2 position;
    private Node parent;

    public int DoorCount { get => doorCount; }
    public List<Connection> Connections { get => connections; }
    public int NodeId { get => nodeId; }
    public Vector2 Position { get => position; }
    public Node Parent { get => parent; }
    public RoomType Type { get => type; }

    public Node(Node parent, int doorCount, RoomType type = RoomType.BASE)
    {
        this.doorCount = doorCount;
        this.type = type;
        this.nodeId = ++CurrentNodeID;
        this.parent = parent;

        connections = new List<Connection>();
    }

    public Node(int doorCount, RoomType type = RoomType.BASE)
    {
        this.doorCount = doorCount;
        this.type = type;
        this.nodeId = ++CurrentNodeID;
        this.parent = null;

        connections = new List<Connection>();
    }

    public Connection Connect(Node node)
    {
        Connection connection = new Connection(this, node);
        
        AddConnection(connection);
        node.AddConnection(connection);

        return connection;
    }

    public void AddConnection(Connection connection)
    {
        connections.Add(connection);
    }

    public void AddConnectionWithDoorCount(Connection connection)
    {
        AddConnection(connection);
        doorCount++;
    }

    public void SetPosition(Vector2 position)
    {
        this.position = position;
    }
}
