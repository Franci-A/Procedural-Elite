using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    public GameObject objectToSpawn;
    [SerializeField] private bool spawnOnStart = false;

    private void Start()
    {
        if (spawnOnStart)
            OnSpawnObject();
    }

    public void OnSpawnObject()
    {
        Instantiate(objectToSpawn, transform.position, Quaternion.identity);
    }
}
