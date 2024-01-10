using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public static int CurrentNodeID = -1;

    // up to 4
    private List<Connection> connections;

    private int nodeId;
    private int doorCount;
    private RoomType type;
    private Vector2 position;

    private Utils.ORIENTATION orientation = Utils.ORIENTATION.NONE;

    public int DoorCount { get => doorCount; }
    public List<Connection> Connections { get => connections; }
    public int NodeId { get => nodeId; }
    public Vector2 Position { get => position; }
    public Utils.ORIENTATION Orientation { get => orientation; set => orientation = value; }

    public Node(int doorCount, RoomType type = RoomType.BASE)
    {
        this.doorCount = doorCount;
        this.type = type;
        this.nodeId = ++CurrentNodeID;

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

    public void SetPosition(Vector2 position)
    {
        this.position = position;
    }
}
