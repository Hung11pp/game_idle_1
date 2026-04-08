using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [Header("Pool Setup")]
    public EnemyController enemyPrefab;
    public int initialPoolSize = 60;
    public Transform poolParent;

    private List<EnemyController> pooledEnemies = new List<EnemyController>();
    private List<EnemyController> activeEnemies = new List<EnemyController>();

    private void Start()
    {
        BuildPool();
    }

    public EnemyController GetEnemy()
    {
        for (int i = 0; i < pooledEnemies.Count; i++)
        {
            if (!pooledEnemies[i].gameObject.activeInHierarchy)
            {
                pooledEnemies[i].gameObject.SetActive(true);
                return pooledEnemies[i];
            }
        }

        EnemyController newEnemy = CreateEnemy();
        if (newEnemy == null)
        {
            return null;
        }
        newEnemy.gameObject.SetActive(true);
        return newEnemy;
    }

    public void ReturnEnemy(EnemyController enemy)
    {
        if (enemy == null)
        {
            return;
        }

        activeEnemies.Remove(enemy);
        enemy.gameObject.SetActive(false);

        if (poolParent != null)
        {
            enemy.transform.SetParent(poolParent);
        }
    }

    public int GetActiveEnemyCount()
    {
        return activeEnemies.Count;
    }

    public void RegisterActiveEnemy(EnemyController enemy)
    {
        if (enemy == null)
        {
            return;
        }

        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }

    public EnemyController GetNearestActiveEnemy(Vector3 position, float maxRange)
    {
        EnemyController nearest = null;
        float bestSqrDistance = maxRange * maxRange;

        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            EnemyController enemy = activeEnemies[i];

            if (enemy == null || !enemy.IsAlive())
            {
                activeEnemies.RemoveAt(i);
                continue;
            }

            float sqrDistance = (enemy.transform.position - position).sqrMagnitude;
            if (sqrDistance <= bestSqrDistance)
            {
                bestSqrDistance = sqrDistance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    private void BuildPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateEnemy();
        }
    }

    private EnemyController CreateEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemyPool is missing an enemy prefab.");
            return null;
        }

        EnemyController newEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);

        if (poolParent != null)
        {
            newEnemy.transform.SetParent(poolParent);
        }
        else
        {
            newEnemy.transform.SetParent(transform);
        }

        newEnemy.gameObject.SetActive(false);
        pooledEnemies.Add(newEnemy);
        return newEnemy;
    }
}