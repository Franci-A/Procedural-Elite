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
    [SerializeField, MinMaxSlider(0, 50)] private Vector2Int sidePathRoomRange;
    [SerializeField] private int sidePathsNumber;

    [Header("Visual")]
    [SerializeField] GameObject roomPrefab;
    [SerializeField] LineRenderer lineRendererPrefab;

    [Header("Security & Debug")]
    [SerializeField] private int loopBreakIterationCount = 20000;
    [SerializeField] private int randomSeed;

    Dictionary<Vector2, GameObject> positions = new Dictionary<Vector2, GameObject>();
    List<GameObject> listLine = new List<GameObject>();
    Vector2 currentPosition;

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
        int goldenPathRoomCount = Random.Range(goldenPathRoomRange.x, goldenPathRoomRange.y);
        int totalRoomCount = goldenPathRoomCount;

        Node startNode = new Node(1, RoomType.START);
        currentPosition = Vector2.zero;
        CreateNode(startNode, currentPosition);

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

            CreateNode(node, currentPosition);
            LinkNodes(lastConnection);

            if (shouldCreateSidePath)
            {
                availableSidePaths--;

                int roomCount = 0;
                int doorCount = 1;

                int maxSideRoomCount = Random.Range(sidePathRoomRange.x, sidePathRoomRange.y);
                GenerateSidePath(node, maxSideRoomCount + node.DoorCount - 1, ref roomCount, ref doorCount);

                Debug.Log($"Side Path {sidePathsNumber - availableSidePaths} generated {roomCount} rooms. At Node {node.NodeId}");

                // Lock door to next Golden Path room

                totalRoomCount += maxSideRoomCount;
            }

            lastNode = node;
        }

        Node endNode = new Node(lastNode, 1, RoomType.END);
        var endConnection = lastNode.Connect(endNode);
        CreateNode(endNode, currentPosition);
        LinkNodes(endConnection);

        DebugConnectedNodes(startNode);
        Debug.Log($"Generated {totalRoomCount} rooms");
    }

    private void GenerateSidePath(Node parentNode, int maxRoomCount, ref int roomCount, ref int doorCount)
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

            CreateNode(nextNode, currentPosition);
            LinkNodes(connection);

            GenerateSidePath(nextNode, maxRoomCount, ref roomCount, ref doorCount);
        }

        currentPosition = parentNode.Position;
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
        Utils.ORIENTATION lastOrientation = node.Parent == null ?
            Utils.GetRandomOrientation() :
            Utils.DirToOrientation(node.Parent.Position - node.Position);

        // Try 4 times to place around
        int orientationCheckCount = 0;
        Vector2 initialPosition = position;
        do
        {
            if (lastOrientation == Utils.ORIENTATION.NONE)
                position += Utils.OrientationToDir(Utils.GetRandomOrientation());
            else
                position += Utils.OrientationToDir(Utils.GetRandomOrientation(Utils.OppositeOrientation(lastOrientation)));
            
            if (!positions.ContainsKey(position))
                break;
            position = initialPosition;

            orientationCheckCount++;
            if(orientationCheckCount >= 4)
                throw new System.Exception("Place Already Occupied");
        } while (true);

        var go = Instantiate(roomPrefab, position, Quaternion.identity);
        go.name = $"Node {node.NodeId}";
        go.GetComponent<PlaceholderRoomHandler>().Initialise(node.Type);

        positions.Add(position, go);
        node.SetPosition(position);
        currentPosition = position;
    }

    private void LinkNodes(Connection connection)
    {
        LineRenderer line = Instantiate(lineRendererPrefab, transform.position, Quaternion.identity);
        Vector3 pointA = connection.From.Position;
        Vector3 pointB = connection.To.Position;

        Vector3[] positionArray = { pointA, pointB };
        line.SetPositions(positionArray);

        listLine.Add(line.gameObject);
    }
}
