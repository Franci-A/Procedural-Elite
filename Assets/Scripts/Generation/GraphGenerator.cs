using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GraphGenerator : MonoBehaviour
{
    /// <summary>
    /// Percentage of Rooms used for the Golden Path
    /// </summary>
    [SerializeField, MinMaxSlider(0, 50)] private Vector2Int goldenPathRoomRange;
    /// <summary>
    /// Percentage of Rooms used for a Side Path
    /// </summary>
    [SerializeField, MinMaxSlider(0, 50)] private Vector2Int sidePathRoomRange;
    [SerializeField] private int sidePathsNumber;

    [Header("Visual")]
    Dictionary<Vector2, GameObject> positions = new Dictionary<Vector2, GameObject>();
    List<GameObject> listLine = new List<GameObject>();

    Vector2 currentPosition;
    [SerializeField] GameObject roomPrefab;

    [SerializeField] LineRenderer lineRendererPrefab;

    void Start()
    {
        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        try
        {
            GenerateGoldenPath();
        }
        catch (System.Exception)
        {
            Restart();
        }
    }

    private void GenerateGoldenPath()
    {
        int goldenPathRoomCount = Random.Range(goldenPathRoomRange.x, goldenPathRoomRange.y);
        int totalRoomCount = goldenPathRoomCount;

        Node startNode = new Node(1, RoomType.START);
        currentPosition = Vector2.zero;
        CreateNode(startNode, currentPosition, Utils.ORIENTATION.NONE);

        int availableSidePaths = sidePathsNumber;

        Node lastNode = startNode;
        for (int i = 0; i < goldenPathRoomCount; ++i)
        {
            bool shouldCreateSidePath =
                (goldenPathRoomCount - i <= availableSidePaths) // if there are as much rooms to place as side paths to generate = BRANCH
                || availableSidePaths > 0 && Random.Range(0, 2) == 0; // if still side path to generate + Luck

            Node node = shouldCreateSidePath ?
                new Node(3) :
                new Node(2);

            lastNode.Connect(node);

            CreateNode(node, currentPosition, lastNode.Orientation);
            LinkNodes(lastNode);

            if (shouldCreateSidePath)
            {
                availableSidePaths--;

                int roomCount = 0;
                int doorCount = 1;

                int maxSideRoomCount = Random.Range(sidePathRoomRange.x, sidePathRoomRange.y);
                GenerateSidePath(node, maxSideRoomCount, ref roomCount, ref doorCount);

                Debug.Log($"Side Path {sidePathsNumber - availableSidePaths} generated {roomCount} rooms. At Node {node.NodeId}");

                // Lock door to next Golden Path room

                totalRoomCount += maxSideRoomCount;
            }

            lastNode = node;
        }

        Node endNode = new Node(1, RoomType.END);
        lastNode.Connect(endNode);

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

            Node nextNode = new Node(nextDoorCount);

            parentNode.Connect(nextNode);
            GenerateSidePath(nextNode, maxRoomCount, ref roomCount, ref doorCount);
        }
    }

    private void DebugConnectedNodes(Node parentNode)
    {
        string connectedNodes = "";
        foreach (var door in parentNode.Connections)
        {
            if (door.NodeB.NodeId == parentNode.NodeId)
                continue;
            connectedNodes += $"{door.NodeB.NodeId}, ";
            DebugConnectedNodes(door.NodeB);
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
        GenerateDungeon();
    }

    private void CreateNode(Node node, Vector2 position, Utils.ORIENTATION lastOrientation)
    {
        if (positions.ContainsKey(position)) throw new System.Exception();// needRestart = true;
        else
        {
            var go = Instantiate(roomPrefab, position, Quaternion.identity);
            positions.Add(position, go);

            Vector2 lastPosition = position;

            if (lastOrientation == Utils.ORIENTATION.NONE) currentPosition += Utils.OrientationToDir(Utils.GetRandomOrientation());
            else currentPosition += Utils.OrientationToDir(Utils.GetRandomOrientation(lastOrientation));
            else currentPosition += Utils.OrientationToDir(Utils.GetRandomOrientation(Utils.OppositeOrientation(lastOrientation)));

            node.Orientation = Utils.DirToOrientation(currentPosition - position);

        }
    }

    private void LinkNodes(Node nodeA)
    {
        /*LineRenderer line = Instantiate(lineRendererPrefab, transform.position, Quaternion.identity);
        Vector3 pointA = currentPosition;
        Vector3 pointB = currentPosition + Utils.OrientationToDir(nodeA.Orientation);
        //Vector3 pointB = currentPosition + Utils.OrientationToDir(nodeA.Orientation);

        Vector3[] positionArray = { pointA, pointB};
        line.SetPositions(positionArray);

        listLine.Add(line.gameObject);*/
    }

}
