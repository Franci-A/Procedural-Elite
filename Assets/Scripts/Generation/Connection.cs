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
}
