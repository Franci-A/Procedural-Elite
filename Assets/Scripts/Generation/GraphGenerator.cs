using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Orientation = Utils.ORIENTATION;

public class GraphGenerator : MonoBehaviour
{
    [SerializeField] private int maxRoomNumber;

    // Start is called before the first frame update
    void Start()
    {
        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        Node startNode = new Node(1);
        Node previousNode = startNode;

        for (int i = 0; i < maxRoomNumber - 1; i++)
        {
            previousNode = GenerateRoom(previousNode);
        }

        Node endNode = new Node(1);
        previousNode.Connect(endNode);

        Node currentNode = startNode;
        
        while (true)
        {
            Debug.Log($"Node {currentNode.NodeId} connected to : ");
            foreach(var item in currentNode.Connections)
            {
                Debug.Log($"{item.NodeA.NodeId}, {item.NodeB.NodeId}");
                currentNode = item.NodeB;
            }

            if (currentNode.Connections.Count == 1)
            {
                Debug.Log($"Node {currentNode.NodeId} is the last one");
                break;
            }
        }
    }

    private Node GenerateRoom(Node parentNode)
    {
        Node nextNode = new Node(2);

        parentNode.Connect(nextNode);

        return nextNode;
    }
}
