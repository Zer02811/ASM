using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    public float spawnRate = 1f;
    public int maxEnemies = 10;
    public float spawnRadius = 5f;
    public EnemyObjectPool enemyPool;

    private int activeEnemiesCount = 0;

    void Start()
    {
        if (enemyPool == null)
        {
            enabled = false;
            return;
        }
        StartCoroutine(SpawnEnemiesCoroutine());
    }

    IEnumerator SpawnEnemiesCoroutine()
    {
        while (true)
        {
            if (activeEnemiesCount < maxEnemies)
            {
                SpawnEnemy();
            }
            yield return new WaitForSeconds(2f); // Chờ 2 giây trước khi sinh quái vật tiếp theo
        }
    }

    void SpawnEnemy()
    {
        Vector3 spawnPosition;

        if (spawnPoints.Length > 0)
        {
            Transform selectedSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            spawnPosition = selectedSpawnPoint.position;
        }
        else
        {
            spawnPosition = transform.position + Random.insideUnitSphere * spawnRadius;
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(spawnPosition, out navHit, spawnRadius, NavMesh.AllAreas))
            {
                spawnPosition = navHit.position;
            }
            else
            {
                return;
            }
        }

        EnemyAI enemy = enemyPool.GetPooledObject();
        if (enemy != null)
        {
            enemy.transform.position = spawnPosition;
            enemy.gameObject.SetActive(true);
            activeEnemiesCount++;

            // reset health
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.currentHealth = enemyHealth.maxHealth;
                enemyHealth.IsDead = false; // Very important: Reset the dead flag using public property
            }
            UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = true;
            }

            CharacterController characterController = enemy.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = true;
            }
            Collider collider = enemy.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = true;
            }
        }
    }

    public void OnEnemyReturnedToPool()
    {
        activeEnemiesCount--;
    }

    public void ReturnEnemyToPool(EnemyAI enemy)
    {
        enemyPool.ReturnObjectToPool(enemy);
        activeEnemiesCount--;
    }
}