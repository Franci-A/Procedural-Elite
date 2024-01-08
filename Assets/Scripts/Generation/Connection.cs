public class Connection
{
    private bool isLocked;
    private Node nodeA;
    private Node nodeB;

    public bool IsLocked { get => isLocked; }
    public Node NodeA { get => nodeA; }
    public Node NodeB { get => nodeB; }

    public Connection(Node nodeA, Node nodeB)
    {
        this.nodeA = nodeA;
        this.nodeB = nodeB;
        isLocked = false;
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }
}
