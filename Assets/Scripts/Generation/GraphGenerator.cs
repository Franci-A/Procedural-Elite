using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

public class GraphGenerator : MonoBehaviour
{
    [Header("Procedural Values")]
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
    [SerializeField] private Vector2 gridSize;
    [SerializeField] private RoomsSO roomsList;
    [SerializeField] LineRenderer lineRendererPrefab;
    private Node finalStartNode;

    [Header("Security & Debug")]
    [SerializeField] private int loopBreakIterationCount = 20000;
    [SerializeField] private int randomSeed;
    [SerializeField] private bool spawnPlaceholderRoomPrefab;
    [SerializeField, ShowIf(nameof(spawnPlaceholderRoomPrefab))] GameObject placeholderRoomPrefab;

    List<Vector2> positions = new List<Vector2>();
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
        for (int i = transform.childCount - 1; i >= 0; --i)
            Destroy(transform.GetChild(i).gameObject);
        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        int loopCount = 0;
        while (true)
        {
            if (loopCount >= loopBreakIterationCount)
            {
                Debug.LogError($"STOPPING GENERATION. TOO MANY ITERATIONS. {loopCount} ITERATIONS !");
                break;
            }
            loopCount++;

            try
            {
                ApplyRandomSeed();
                GenerateGoldenPath();
                GeneratePrefabs();
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
        finalStartNode = startNode;
        nextPosition = Vector2.zero;
        PlaceNode(startNode, nextPosition);
        nextPosition = GetNextAvailablePosition(startNode);

        int availableSidePaths = sidePathsNumber;
        int sidePathBranchRoom = GetNextSidePathBranchRoom(goldenPathRoomCount, sidePathsNumber - availableSidePaths);
        bool lockNextDoor = false;
        int secretRoomIndex = Random.Range(0, goldenPathRoomCount / 2);

        Node lastNode = startNode;
        for (int i = 0; i < goldenPathRoomCount; ++i)
        {
            bool shouldCreateSidePath = sidePathsNumber > 0 && availableSidePaths > 0 && i >= sidePathBranchRoom;
            bool shouldCreateSecret = i >= secretRoomIndex;

            Node node = new Node(lastNode, shouldCreateSidePath ? 3 : 2, RoomType.GOLDEN_PATH);

            var lastConnection = lastNode.Connect(node);
            lastConnection.SetLocked(lockNextDoor);
            lockNextDoor = shouldCreateSidePath;

            // Lock door to next Golden Path room
            PlaceNode(node, nextPosition);
            nextPosition = GetNextAvailablePosition(node);

            if (shouldCreateSidePath)
            {
                availableSidePaths--;
                GenerateSidePath(node);
                sidePathBranchRoom = GetNextSidePathBranchRoom(goldenPathRoomCount, sidePathsNumber - availableSidePaths);
            }

            if (shouldCreateSecret)
            {
                GenerateSecretRoom(node);
                secretRoomIndex = goldenPathRoomCount;
            }

            lastNode = node;
        }

        Node endNode = new Node(lastNode, 1, RoomType.END);
        var endConnection = lastNode.Connect(endNode);
        PlaceNode(endNode, nextPosition);
        nextPosition = GetNextAvailablePosition(endNode);

        DebugConnectedNodes(startNode);
        Debug.Log($"Generated {totalRoomCount} rooms");
    }

    private int GetNextSidePathBranchRoom(int totalPathRoomCount, int currentSection)
    {
        if (sidePathsNumber <= 0) return -1;
        
        int sectionRoomCount = totalPathRoomCount / sidePathsNumber;
        int min = Mathf.Clamp(sectionRoomCount * currentSection, 1, totalPathRoomCount - 1);
        int max = Mathf.Min(sectionRoomCount * (currentSection + 1), totalPathRoomCount - 1);
        return Random.Range(min, max + 1);
    }

    private void GenerateSidePath(Node startNode)
    {
        int maxSideRoomCount = Random.Range(sidePathRoomRange.x, sidePathRoomRange.y + 1);

        Node node = new Node(startNode, Random.Range(Mathf.Min(2, maxSideRoomCount), Mathf.Min(4, maxSideRoomCount) + 1), RoomType.SIDE_PATH);
        var lastConnection = startNode.Connect(node);
        PlaceNode(node, nextPosition);
        nextPosition = GetNextAvailablePosition(node);

        int roomCount = 0;
        int doorCount = node.DoorCount - 1;

        GenerateRoomRecurring(node, nextPosition, sidePathRoomRange.x, maxSideRoomCount, ref roomCount, ref doorCount);

        nextPosition = GetNextAvailablePosition(startNode);
        //Debug.Log($"Side Path generated Node {startNode.NodeId} with {roomCount} rooms.");
    }

    private void GenerateRoomRecurring(Node parentNode, Vector2 position, int minRoomCount, int maxRoomCount, ref int roomCount, ref int doorCount)
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
            else if (roomCount < minRoomCount)
            {
                minRandomRange = 2;
                maxRandomRange = 4;
            }

            int nextDoorCount = Random.Range(minRandomRange, maxRandomRange);
            if (doorCount + nextDoorCount >= maxRoomCount)
                nextDoorCount = maxRoomCount - doorCount;

            // Create a new node at this position
            // Connected to parentNode
            Node nextNode = new Node(parentNode, nextDoorCount, RoomType.SIDE_PATH);
            parentNode.Connect(nextNode);
            PlaceNode(nextNode, position);

            // Generate needed rooms from the new node,
            // sending the Next Available Position around this new node
            GenerateRoomRecurring(nextNode, GetNextAvailablePosition(nextNode), minRoomCount, maxRoomCount, ref roomCount, ref doorCount);

            // Stop if all the connections are complete
            if (parentNode.DoorCount <= parentNode.Connections.Count)
                return;

            // Update the Next Available Position around the parent Node
            position = GetNextAvailablePosition(parentNode);
        }
    }

    private void GenerateSecretRoom(Node parentNode)
    {
        if (parentNode.DoorCount >= 4)
            throw new System.Exception("Node already has every Connection occupied. Can't generate secret room here !");

        var position = GetNextAvailablePosition(parentNode);
        var secretNode = new Node(parentNode, 1, RoomType.SECRET);
        var connection = parentNode.Connect(secretNode);
        connection.SetSecret(true);
        connection.SetLocked(true);

        PlaceNode(secretNode, position);
        nextPosition = GetNextAvailablePosition(parentNode);
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
        //Debug.Log($"Node {parentNode.NodeId} connected to : " + connectedNodes);
    }

    void Restart()
    {
        positions.Clear();
        Node.ResetID();
        randomSeed++;
    }

    private void PlaceNode(Node node, Vector2 position)
    {
        node.SetPosition(position);
        positions.Add(position);

        totalRoomCount++;
    }

    private void LinkNodes(Connection connection)
    {
        LineRenderer line = Instantiate(lineRendererPrefab, transform.position, Quaternion.identity, transform);
        line.name = $"  Link {connection.From.NodeId} -> {connection.To.NodeId}";

        Vector3 pointA = connection.From.Position * gridSize;
        Vector3 pointB = connection.To.Position * gridSize;

        Vector3[] positionArray = { pointA, pointB };
        line.SetPositions(positionArray);

        if (Vector2.Distance(pointA, pointB) != gridSize.x && Vector2.Distance(pointA, pointB) != gridSize.y)
        {
            line.startColor *= Color.red;
            line.endColor *= Color.red;
        }

        if (connection.IsLocked)
        {
            line.startColor *= Color.black;
            line.endColor *= Color.black;
        }


        listLine.Add(line.gameObject);
    }

    private GameObject InstantiateRoomPlaceholder(Node node)
    {
        var go = Instantiate(placeholderRoomPrefab, node.Position * gridSize, Quaternion.identity, transform);
        go.name = $"Node {node.NodeId}";
        go.GetComponent<PlaceholderRoomHandler>().Initialise(node);
        return go;
    }

    private Vector2 GetNextAvailablePosition(Node node)
    {
        List<Utils.ORIENTATION> occupiedDirections = new List<Utils.ORIENTATION>();
        if (node.Parent != null)
            occupiedDirections.Add(Utils.DirToOrientation(node.Parent.Position - node.Position));

        Utils.ORIENTATION nextDirection;
        Vector2 pos = node.Position;
        Vector2 initialPosition = pos;

        // Try 4 times to place around
        do
        {
            nextDirection = Utils.GetRandomOrientation(occupiedDirections.ToArray());
            pos += Utils.OrientationToDir(nextDirection);

            if (!positions.Contains(pos))
                break;

            occupiedDirections.Add(nextDirection);
            pos = initialPosition;

            if (occupiedDirections.Count >= 4)
                throw new System.Exception("Place Already Occupied");
        } while (true);

        return pos;
    }

    private void GeneratePrefabs()
    {
        List<Node> openList = new List<Node>();
        List<Node> clostList = new List<Node>();

        openList.Add(finalStartNode);

        while (openList.Count > 0)
        {
            Node node = openList[0];
            clostList.Add(node);

            List<Utils.ORIENTATION> nodeOrientation = new List<Utils.ORIENTATION>();

            foreach (var connection in node.Connections)
            {
                if (!clostList.Contains(connection.To))
                {
                    openList.Add(connection.To);
                    if(spawnPlaceholderRoomPrefab)LinkNodes(connection);
                }
                nodeOrientation.Add(connection.GetOrientation(node));
            }

            if (spawnPlaceholderRoomPrefab)
            {
                InstantiateRoomPlaceholder(node);
            }
            else
            {
                GameObject newRoom = roomsList.GetRoom(nodeOrientation, node.Type);

                Room room = null;
                if (newRoom != null)
                {
                    // je retire gridsize / 2 car l'origine des rooms est en 0,0
                    room = Instantiate(newRoom, node.Position * gridSize - (gridSize / 2), Quaternion.identity, transform).GetComponent<Room>();
                    room.Position = new Vector2Int((int)node.Position.x, (int)node.Position.y);
                    if (node.NodeId == 0)
                    {
                        room.isStartRoom = true;
                    }
                    else room.isStartRoom = false;

                    foreach (var connection in node.Connections)
                    {
                        if (connection.IsLocked)
                        {
                            room.GetDoor(connection.GetOrientation(node), room.gameObject.transform.position).SetState(Door.STATE.CLOSED);
                        }
                    }
                }
                else InstantiateRoomPlaceholder(node);


            }
                openList.RemoveAt(0);
        }
    }
}
