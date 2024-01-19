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

    [SerializeField] private List<BossPlug> bossPlugList;
    private int spawnedAmount;
    [SerializeField] GameObject shield;

    private void Start()
    {
        for (int i = 0; i < bossPlugList.Count; i++)
        {
            bossPlugList[i].onDestroy.AddListener(PlugDestroyed);
        }
        attack.canBeHit = false;
        shield.SetActive(true);
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
            yield return new WaitForSeconds(.5f);
           Instantiate<Enemy>(enemyList[Random.Range(0, enemyList.Count)], transform.position, Quaternion.identity).onDied.AddListener(SpawnedEnemyDied);
        }

        SetState(BossState.ATTACK);
    }

    private void SpawnedEnemyDied()
    {
        spawnedAmount--;
        if(spawnedAmount <= 0)
        {
            if (!attack.canBeHit)
                return;
            spawnedAmount = 0;
            StartCoroutine(SpawnCooldown());
        }
    }

    private IEnumerator SpawnCooldown()
    {
        yield return new WaitForSeconds(spawnInterval);
        SetState(BossState.SPAWNING);
    }

    private void PlugDestroyed(BossPlug obj)
    {
        bossPlugList.Remove(obj);

        if(bossPlugList.Count <= 0)
        {
            attack.canBeHit = true;
            shield.SetActive(false);
            StopAllCoroutines();
            SetState(BossState.ATTACK);
        }
    }
}

enum BossState
{
    NONE,
    CHASE,
    ATTACK,
    SPAWNING,
}
