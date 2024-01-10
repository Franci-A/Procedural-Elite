using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphGenerator : MonoBehaviour
{
    /// <summary>
    /// Number of Rooms used for the Golden Path
    /// </summary>
    [SerializeField, MinMaxSlider(0, 50)] private Vector2Int goldenPathRoomRange;
    /// <summary>
    /// Number of Rooms used for a Side Path
    /// </summary>
    [SerializeField, MinMaxSlider(1, 50)] private Vector2Int sidePathRoomRange;
    [SerializeField] private int sidePathsNumber;

    [Header("Visual")]
    [SerializeField] GameObject roomPrefab;
    [SerializeField] LineRenderer lineRendererPrefab;

    [Header("Security & Debug")]
    [SerializeField] private int loopBreakIterationCount = 20000;
    [SerializeField] private int randomSeed;

    Dictionary<Vector2, GameObject> positions = new Dictionary<Vector2, GameObject>();
    List<GameObject> listLine = new List<GameObject>();
    Vector2 nextPosition;
    private int totalRoomCount;

    private bool IsApplicationRunning { get => Application.isPlaying; }

    void Start()
    {
        GenerateDungeon();
    }

    private void ApplyRandomSeed()
    {
        Random.seed = randomSeed;
    }

    [Button("Regenerate Dungeon"), ShowIf(nameof(IsApplicationRunning))]
    private void RegenerateDungeon()
    {
        Restart();
        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        int loopCount = 0;
        while (true)
        {
            if (loopCount > loopBreakIterationCount)
            {
                Debug.LogError($"STOPPING GENERATION. TOO MANY ITERATIONS !");
                break;
            }
            loopCount++;

            try
            {
                ApplyRandomSeed();
                GenerateGoldenPath();
                Debug.Log($"Success ! ({loopCount} iterations Only ^^)");
                break;
            }
            catch (System.Exception e)
            {
                //Debug.LogError($"{e.Message}.");
                Restart();
            }
        }
        Node.ResetID();
    }

    private void GenerateGoldenPath()
    {
        totalRoomCount = 0;
        int goldenPathRoomCount = Random.Range(goldenPathRoomRange.x, goldenPathRoomRange.y + 1);

        Node startNode = new Node(1, RoomType.START);
        nextPosition = Vector2.zero;
        CreateNode(startNode, nextPosition);

        int availableSidePaths = sidePathsNumber;

        Node lastNode = startNode;
        for (int i = 0; i < goldenPathRoomCount; ++i)
        {
            bool shouldCreateSidePath =
                (goldenPathRoomCount - i <= availableSidePaths) // if there are as much rooms to place as side paths to generate = BRANCH
                || availableSidePaths > 0 && Random.Range(0, 2) == 0; // if still side path to generate + Luck

            Node node = shouldCreateSidePath ?
                new Node(lastNode, 3, RoomType.GOLDEN_PATH) :
                new Node(lastNode, 2, RoomType.GOLDEN_PATH);

            var lastConnection = lastNode.Connect(node);

            CreateNode(node, nextPosition);
            LinkNodes(lastConnection);

            if (shouldCreateSidePath)
            {
                availableSidePaths--;
                GenerateSidePath(node);

                // Lock door to next Golden Path room
            }

            lastNode = node;
        }

        Node endNode = new Node(lastNode, 1, RoomType.END);
        var endConnection = lastNode.Connect(endNode);
        CreateNode(endNode, nextPosition);
        LinkNodes(endConnection);

        DebugConnectedNodes(startNode);
        Debug.Log($"Generated {totalRoomCount} rooms");
    }

    private void GenerateSidePath(Node startNode)
    {
        int maxSideRoomCount = Random.Range(sidePathRoomRange.x, sidePathRoomRange.y + 1);

        // /!\ Take into account sidePathRoomRange.minimum room amount /!\
        Node node = new Node(startNode, Random.Range(Mathf.Min(2, maxSideRoomCount), Mathf.Min(4, maxSideRoomCount) + 1), RoomType.SIDE_PATH);
        var lastConnection = startNode.Connect(node);
        CreateNode(node, nextPosition);
        LinkNodes(lastConnection);

        int roomCount = 0;
        int doorCount = node.DoorCount - 1;

        GenerateRoomRecurring(node, maxSideRoomCount, ref roomCount, ref doorCount);

        nextPosition = GetNextAvailablePosition(startNode);
        Debug.Log($"Side Path generated Node {startNode.NodeId} with {roomCount} rooms.");
    }

    private void GenerateRoomRecurring(Node parentNode, int maxRoomCount, ref int roomCount, ref int doorCount)
    {
        roomCount++;
        doorCount += parentNode.DoorCount - 1;

        while (parentNode.DoorCount > parentNode.Connections.Count)
        {
            int minRandomRange = 1;
            int maxRandomRange = 5;

            if (doorCount >= maxRoomCount - 1)
            {
                minRandomRange = 1;
                maxRandomRange = 2;
            }
            else if (roomCount < sidePathRoomRange.x)
            {
                minRandomRange = 2;
                maxRandomRange = 4;
            }

            int nextDoorCount = Random.Range(minRandomRange, maxRandomRange);
            if (doorCount + nextDoorCount >= maxRoomCount)
                nextDoorCount = maxRoomCount - doorCount;

            Node nextNode = new Node(parentNode, nextDoorCount, RoomType.SIDE_PATH);

            var connection = parentNode.Connect(nextNode);

            CreateNode(nextNode, nextPosition);
            LinkNodes(connection);

            GenerateRoomRecurring(nextNode, maxRoomCount, ref roomCount, ref doorCount);
        }

        nextPosition = GetNextAvailablePosition(parentNode.Parent);
    }

    private void DebugConnectedNodes(Node parentNode)
    {
        string connectedNodes = "";
        foreach (var door in parentNode.Connections)
        {
            if (door.To.NodeId == parentNode.NodeId)
                continue;
            connectedNodes += $"{door.To.NodeId}, ";
            DebugConnectedNodes(door.To);
        }

        if (connectedNodes == "") connectedNodes = "NONE, Dead-End.";
        Debug.Log($"Node {parentNode.NodeId} connected to : " + connectedNodes);
    }

    void Restart()
    {
        foreach (var item in positions)
        {
            Destroy(item.Value);
        }

        foreach (var item in listLine)
        {
            Destroy(item);
        }

        listLine.Clear();
        positions.Clear();
        Node.ResetID();
        randomSeed++;
    }

    private void CreateNode(Node node, Vector2 position)
    {
        node.SetPosition(position);
        var go = InstantiateRoomPlaceholder(node);
        positions.Add(position, go);

        totalRoomCount++;

        if (node.Parent == null)
        {
            nextPosition = position + Utils.OrientationToDir(Utils.GetRandomOrientation());
            return;
        }

        nextPosition = GetNextAvailablePosition(node);
    }

    private void LinkNodes(Connection connection)
    {
        LineRenderer line = Instantiate(lineRendererPrefab, transform.position, Quaternion.identity);
        Vector3 pointA = connection.From.Position;
        Vector3 pointB = connection.To.Position;

        Vector3[] positionArray = { pointA, pointB };
        line.SetPositions(positionArray);

        if (Vector2.Distance(pointA, pointB) > 1f)
        {
            line.startColor *= Color.red;
            line.endColor *= Color.red;
        }

        listLine.Add(line.gameObject);
    }

    private GameObject InstantiateRoomPlaceholder(Node node)
    {
        var go = Instantiate(roomPrefab, node.Position, Quaternion.identity);
        go.name = $"Node {node.NodeId}";
        go.GetComponent<PlaceholderRoomHandler>().Initialise(node);
        return go;
    }

    private Vector2 GetNextAvailablePosition(Node node)
    {
        if (node.Parent == null)
            throw new System.Exception("Bad parameter. Node has no Parent");

        Utils.ORIENTATION lastOrientation = Utils.DirToOrientation(node.Parent.Position - node.Position);

        // Try 4 times to place around
        int orientationCheckCount = 0;
        Vector2 nextPosition = node.Position;
        Vector2 initialPosition = nextPosition;
        do
        {
            nextPosition += Utils.OrientationToDir(Utils.GetRandomOrientation(lastOrientation));

            if (!positions.ContainsKey(nextPosition))
                break;

            nextPosition = initialPosition;

            orientationCheckCount++;
            if (orientationCheckCount >= 4)
                throw new System.Exception("Place Already Occupied");
        } while (true);

        return nextPosition;
    }
}
