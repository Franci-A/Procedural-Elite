using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHandler : MonoBehaviour
{
    [SerializeField] private Enemy attack;
    private BossState state;
    [SerializeField] private List<Enemy> enemyList;
    [SerializeField] private int minSpawnAmount;
    [SerializeField] private int maxSpawnAmount;
    [SerializeField] private float spawnInterval = 3f;

    private int spawnedAmount;

    private void Start()
    {
        SetState(BossState.SPAWNING);
    }

    private void SetState(BossState newState)
    {
        state = newState;
        switch (state)
        {
            case BossState.ATTACK:
                attack.SetState(Enemy.STATE.ATTACKING);
                Debug.Log("attack");
                break;
            case BossState.SPAWNING:
                attack.SetState(Enemy.STATE.IDLE);
                Debug.Log("spawning");
                StartCoroutine( SpawnEnemies());
                break;
        }
    }

    public IEnumerator SpawnEnemies()
    {
        spawnedAmount = Random.Range(maxSpawnAmount, minSpawnAmount);
        for (int i = 0; i < spawnedAmount; i++)
        {
            yield return new WaitForSeconds(.1f);
           Instantiate<Enemy>(enemyList[Random.Range(0, enemyList.Count)], transform.position, Quaternion.identity).onDied.AddListener(SpawnedEnemyDied);
        }

        SetState(BossState.ATTACK);
    }

    private void SpawnedEnemyDied()
    {
        spawnedAmount--;
        if(spawnedAmount <= 0)
        {
            spawnedAmount = 0;
            StartCoroutine(SpawnCooldown());
        }
    }

    private IEnumerator SpawnCooldown()
    {
        yield return new WaitForSeconds(spawnInterval);
        SetState(BossState.SPAWNING);
    }
}

enum BossState
{
    NONE,
    CHASE,
    ATTACK,
    SPAWNING,
}
