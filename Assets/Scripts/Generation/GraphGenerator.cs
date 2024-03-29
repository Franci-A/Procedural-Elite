using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

public class GraphGenerator : MonoBehaviour
{
    [SerializeField, Expandable, Label("Procedural Values")] private ProceduralData data;

    [Header("Visual")]
    [SerializeField] private Vector2 gridSize;
    [SerializeField] private RoomsSO roomsList;
    [SerializeField] LineRenderer lineRendererPrefab;
    private Node finalStartNode;

    [Header("Security & Debug")]
    [SerializeField] private int loopBreakIterationCount = 20000;
    [SerializeField] private bool generateOnStart;
    [SerializeField] private bool useSeed;
    [SerializeField, ShowIf(nameof(useSeed))] private int randomSeed;
    [SerializeField] private bool spawnPlaceholderRoomPrefab;
    [SerializeField, ShowIf(nameof(spawnPlaceholderRoomPrefab))] GameObject placeholderRoomPrefab;
    [SerializeField] private bool debugCatchExceptionsInConsole;

    List<Vector2> positions = new List<Vector2>();
    List<GameObject> listLine = new List<GameObject>();
    Vector2 nextPosition;
    private int totalRoomCount;

    private bool IsApplicationRunning { get => Application.isPlaying; }
    private string InversePlayerTag
    {
        get
        {
            return Player.Instance.GetSelectedWeaponType switch
            {
                WeaponType.MELEE => ROOM_TAG_DISTANCE,
                WeaponType.DISTANCE => ROOM_TAG_MELEE,
                _ => ROOM_TAG_MELEE
            };
        }
    }

    const string ROOM_TAG_MELEE = "Room_Melee";
    const string ROOM_TAG_DISTANCE = "Room_Distance";

    void Start()
    {
        if(generateOnStart) GenerateDungeon();
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

    public void GenerateDungeon()
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
                if(useSeed)
                    ApplyRandomSeed();

                if(data.generateGoldenPath)
                    GenerateGoldenPath();
                else
                    GenerateBranchingPath();
                
                GeneratePrefabs();
                Debug.Log($"Success ! ({loopCount} iterations Only ^^)");
                break;
            }
            catch (System.Exception e)
            {
                if(debugCatchExceptionsInConsole)
                    Debug.LogError(e);
                Restart();
            }
        }
        Node.ResetID();

    }

    private void GenerateBranchingPath()
    {
        totalRoomCount = 0;
        int pathRoomCount = Random.Range(data.pathRoomRange.x, data.pathRoomRange.y + 1);

        Node startNode = GetStartNode();
        nextPosition = GetNextAvailablePosition(startNode);

        GenerateRoomRecursive(startNode, nextPosition, data.pathRoomRange.x, pathRoomCount);
        
        var lastNode = GetDeepestNode(startNode);
        {
            Node currentNode = lastNode;
            while (currentNode.Parent != null)
            {
                currentNode.SetType(RoomType.GOLDEN_PATH);
                currentNode = currentNode.Parent;
            }
        }
        var endNode = GetEndNode(lastNode, GetNextAvailablePosition(lastNode));

        Debug.Log($"Generated {totalRoomCount} rooms from `roomCount` {pathRoomCount}");
    }

    private void GenerateGoldenPath()
    {
        totalRoomCount = 0;
        int goldenPathRoomCount = Random.Range(data.goldenPathRoomRange.x, data.goldenPathRoomRange.y + 1);

        Node startNode = GetStartNode();
        finalStartNode = startNode;
        nextPosition = GetNextAvailablePosition(startNode);

        int availableSidePaths = data.sidePathsNumber;
        int sidePathBranchRoom = GetNextSidePathBranchRoom(goldenPathRoomCount, data.sidePathsNumber - availableSidePaths);
        bool lockNextDoor = false;
        int secretRoomIndex = Random.Range(0, goldenPathRoomCount / 2);

        Node lastNode = startNode;
        for (int i = 0; i < goldenPathRoomCount; ++i)
        {
            bool shouldCreateSidePath = data.sidePathsNumber > 0 && availableSidePaths > 0 && i >= sidePathBranchRoom;
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
                sidePathBranchRoom = GetNextSidePathBranchRoom(goldenPathRoomCount, data.sidePathsNumber - availableSidePaths);
            }

            if (shouldCreateSecret)
            {
                GenerateSecretRoom(node);
                secretRoomIndex = goldenPathRoomCount;
            }

            lastNode = node;
        }

        Node endNode = GetEndNode(lastNode, nextPosition);
        nextPosition = GetNextAvailablePosition(endNode);

        //DebugConnectedNodes(startNode);
        Debug.Log($"Generated {totalRoomCount} rooms");
    }

    private int GetNextSidePathBranchRoom(int totalPathRoomCount, int currentSection)
    {
        if (data.sidePathsNumber <= 0) return -1;
        
        int sectionRoomCount = totalPathRoomCount / data.sidePathsNumber;
        int min = Mathf.Clamp(sectionRoomCount * currentSection, 1, totalPathRoomCount - 1);
        int max = Mathf.Min(sectionRoomCount * (currentSection + 1), totalPathRoomCount - 1);
        return Random.Range(min, max + 1);
    }

    private void GenerateSidePath(Node startNode)
    {
        int maxSideRoomCount = Random.Range(data.sidePathRoomRange.x, data.sidePathRoomRange.y + 1);

        Node node = new Node(startNode, Random.Range(Mathf.Min(2, maxSideRoomCount), Mathf.Min(4, maxSideRoomCount) + 1), RoomType.SIDE_PATH);
        startNode.Connect(node);
        PlaceNode(node, nextPosition);

        GenerateRoomRecursive(node, GetNextAvailablePosition(node), data.sidePathRoomRange.x, maxSideRoomCount);
        nextPosition = GetNextAvailablePosition(startNode);

        TagKeyRoomInBranch(node);
    }

    private void GenerateRoomRecursive(Node startNode, Vector2 position, int minRoomCount, int maxRoomCount)
    {
        if (maxRoomCount <= 0) return;

        int roomCount = 0;
        int doorCount = startNode.DoorCount - 1;
        int initialDepth = startNode.Depth;

        void GenerateNextRoomRecursive(Node parentNode, Vector2 position, int minRoomCount, int maxRoomCount)
        {
            roomCount++;
            doorCount += parentNode.DoorCount - 1;


            while (parentNode.DoorCount > parentNode.Connections.Count)
            {
                int minRandomRange = 1;
                int maxRandomRange = 5;

                if (doorCount >= maxRoomCount - 1)
                {
                    maxRandomRange = 2;
                }
                else if (doorCount < minRoomCount - 1)
                {
                    minRandomRange = 2;
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
                GenerateNextRoomRecursive(nextNode, GetNextAvailablePosition(nextNode), minRoomCount, maxRoomCount);

                // Stop if all the connections are complete
                if (parentNode.DoorCount <= parentNode.Connections.Count)
                    return;

                // Update the Next Available Position around the parent Node
                position = GetNextAvailablePosition(parentNode);
            }
        }

        GenerateNextRoomRecursive(startNode, position, minRoomCount, maxRoomCount);
    }

    private Node GetStartNode()
    {
        Node startNode = new Node(1, RoomType.START);
        finalStartNode = startNode;
        PlaceNode(startNode, Vector2.zero);

        return startNode;
    }

    private Node GetEndNode(Node lastNode, Vector2 position)
    {
        Node endNode = new Node(lastNode, 1, RoomType.END);
        lastNode.Connect(endNode);
        PlaceNode(endNode, position);

        return endNode;
    }

    private Node GetDeepestNode(Node startNode)
    {
        Node GetDeepestNodeRecursive(Node parentNode, Node deepestNode)
        {
            foreach (Connection connection in parentNode.Connections)
            {
                if (connection.To == parentNode)
                    continue;

                deepestNode = GetDeepestNodeRecursive(connection.To, deepestNode);
                if (connection.To.Depth <= deepestNode.Depth)
                    continue;

                deepestNode = connection.To;
            }
            return deepestNode;
        }

        return GetDeepestNodeRecursive(startNode, startNode);
    }

    private void GenerateSecretRoom(Node parentNode)
    {
        if (parentNode.DoorCount >= 4)
            throw new System.Exception("Node already has every Connection occupied. Can't generate secret room here !");

        var position = GetNextAvailablePosition(parentNode);
        var secretNode = new Node(parentNode, 1, RoomType.SECRET);
        var connection = parentNode.Connect(secretNode);
        connection.SetSecret(true);
        //connection.SetLocked(true);

        PlaceNode(secretNode, position);
        nextPosition = GetNextAvailablePosition(parentNode);
    }

    private void TagKeyRoomInBranch(Node startNode)
    {
        var keyNode = GetDeepestNode(startNode);
        keyNode.SetType(RoomType.KEY);
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
        positions.Clear();
        Node.ResetID();
        if(useSeed) randomSeed++;
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
        int loopCount = 0;
        List<Node> openList = new List<Node>();
        List<Node> clostList = new List<Node>();

        openList.Add(finalStartNode);

        while (openList.Count > 0 && loopCount < totalRoomCount)
        {
            loopCount++;
            Node node = openList[0];
            clostList.Add(node);

            List<Utils.ORIENTATION> nodeOrientation = new List<Utils.ORIENTATION>();

            foreach (var connection in node.Connections)
            {
                if (!clostList.Contains(connection.To))
                {
                    openList.Add(connection.To);
                    if (spawnPlaceholderRoomPrefab) LinkNodes(connection);
                }
                nodeOrientation.Add(connection.GetOrientation(node));
            }

            if (spawnPlaceholderRoomPrefab)
            {
                InstantiateRoomPlaceholder(node);
            }
            else
            {
                GameObject newRoom = roomsList.GetRoom(
                    nodeOrientation,
                    node.Type,
                    ShouldCheckRoomTag(node.Type) ? InversePlayerTag : string.Empty);

                if (newRoom != null)
                {
                    // je retire gridsize / 2 car l'origine des rooms est en 0,0
                    Room room = Instantiate(newRoom, node.Position * gridSize - (gridSize / 2), Quaternion.identity, transform).GetComponent<Room>();
                    room.Position = new Vector2Int((int)node.Position.x, (int)node.Position.y);

                    room.isStartRoom = node.NodeId == 0;

                    foreach (var connection in node.Connections)
                    {
                        if (!connection.IsLocked && !connection.IsSecret)
                            continue;

                        Door door = room.GetDoor(connection.GetOrientation(node), room.gameObject.transform.position);
                        
                        if (connection.IsLocked)
                            door.SetState(Door.STATE.CLOSED);
                        else if(connection.IsSecret) 
                            door.SetState(Door.STATE.SECRET);
                    }
                }
                else InstantiateRoomPlaceholder(node);
            }
            openList.RemoveAt(0);
        }
    }

    private bool ShouldCheckRoomTag(RoomType roomType)
    {
        return roomType == RoomType.GOLDEN_PATH
            || roomType == RoomType.SIDE_PATH;
    }
}
