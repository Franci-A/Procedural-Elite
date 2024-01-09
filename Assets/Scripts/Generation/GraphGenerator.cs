using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphGenerator : MonoBehaviour
{
    [SerializeField] private int minRoomNumber;
    [SerializeField] private int maxRoomNumber;

    private int doorCount;
    private int roomCount;

    void Start()
    {
        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        Node startNode = new Node(1);
        doorCount = 1;
        roomCount = 0;
        GenerateRoom(startNode);

        Debug.Log($"Generated {roomCount} rooms, with {doorCount} doors");
    }

    private void GenerateRoom(Node parentNode)
    {
        string childs = "";
        roomCount++;
        doorCount += parentNode.DoorCount - 1;

        while (parentNode.DoorCount > parentNode.Connections.Count)
        {
            int minRandomRange = 1;
            int maxRandomRange = 5;

            if (doorCount >= maxRoomNumber - 1)
            {
                minRandomRange = 1;
                maxRandomRange = 2;
            }
            else if (roomCount < minRoomNumber)
            {
                minRandomRange = 2;
                maxRandomRange = 4;
            }

            int nextDoorCount = Random.Range(minRandomRange, maxRandomRange);
            if (doorCount + nextDoorCount >= maxRoomNumber)
                nextDoorCount = maxRoomNumber - doorCount;

            Node nextNode = new Node(nextDoorCount);

            childs += $"{nextNode.NodeId}, ";

            parentNode.Connect(nextNode);
            GenerateRoom(nextNode);
        }
        
        if (childs == "") childs = parentNode.Connections[0].NodeA.NodeId.ToString();
        Debug.Log($"Node {parentNode.NodeId} connected to : " + childs);
    }
}
