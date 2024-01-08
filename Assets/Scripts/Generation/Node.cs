using System.Collections.Generic;

public class Node
{
    public static int CurrentNodeID = -1;

    // up to 4
    private List<Connection> connections;

    private int nodeId;
    private int doorCount;
    private RoomType type;

    public int DoorCount { get => doorCount; }
    public List<Connection> Connections { get => connections; }
    public int NodeId { get => nodeId; }

    public Node(int doorCount, RoomType type = RoomType.BASE)
    {
        this.doorCount = doorCount;
        this.type = type;
        this.nodeId = ++CurrentNodeID;

        connections = new List<Connection>();
    }

    public void Connect(Node node)
    {
        Connection connection = new Connection(this, node);
        
        AddConnection(connection);
        node.AddConnection(connection);
    }

    public void AddConnection(Connection connection)
    {
        connections.Add(connection);
    }
}
