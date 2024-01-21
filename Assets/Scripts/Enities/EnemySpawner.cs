using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private List<expSpawnAmount> expSpawnAmounts;
    [SerializeField] private IntVariable playerExp;
    [SerializeField] private float spawnRadius = 1;
    private int enemyToSpawn;
    private int index;
    private int enemiesAlive;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite deadImage;

    bool alreadySpawned = false;
    [Button]
    public void Init()
    {
        if (alreadySpawned)
            return;

        for (int i = expSpawnAmounts.Count -1; i >= 0; i--)
        {
            if (playerExp.Value >= expSpawnAmounts[i].expAmount)
            {
                index = i;
                break;
            }
        }
        enemyToSpawn = expSpawnAmounts[index].spawnAmount;
        SpawnEnemies();
    }


    public void SpawnEnemies()
    {
        alreadySpawned = true;
        enemiesAlive = UnityEngine.Random.Range(expSpawnAmounts[index].minSpawnPerWave, expSpawnAmounts[index].maxSpawnPerWave);
        if (enemiesAlive > enemyToSpawn)
            enemiesAlive = enemyToSpawn;

        for (int i = 0; i < enemiesAlive; i++)
        {
            float rad = UnityEngine.Random.Range(0f, spawnRadius);
            Vector3 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            Instantiate<Enemy>(enemyPrefab, transform.position + offset, Quaternion.identity).onDied.AddListener(OnEnemyDied);
        }
        enemyToSpawn -= enemiesAlive;
    }

    public void OnEnemyDied()
    {
        enemiesAlive--;
        if (enemiesAlive == 0 && enemyToSpawn > 0)
            SpawnEnemies();
        else
        {
            spriteRenderer.sprite = deadImage;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}

[Serializable]
public struct expSpawnAmount
{
    public int expAmount;
    public int spawnAmount;
    public int minSpawnPerWave;
    public int maxSpawnPerWave;
}