using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphGenerator : MonoBehaviour
{
    [SerializeField] private int maxRoomNumber;

    void Start()
    {
        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        Node startNode = new Node(1);

        GenerateRoom(startNode, 1);
    }

    private void GenerateRoom(Node parentNode, int numberOfDoors)
    {
        string childs = "";

        while (numberOfDoors > parentNode.Connections.Count)
        {
            Node nextNode = new Node(Random.Range(1, 4));
            childs += $"{nextNode.NodeId}, ";
            parentNode.Connect(nextNode);
            GenerateRoom(nextNode, Random.Range(1, 4));
        }
        
        if (childs == "") childs = parentNode.Connections[0].NodeA.NodeId.ToString();
        Debug.Log($"Node {parentNode.NodeId} connected to : " + childs);
    }
}
