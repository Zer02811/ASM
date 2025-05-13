using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObjectPool : MonoBehaviour
{
    public EnemyAI prefab; // Prefab của EnemyAI cần pool
    public int poolSize = 10;
    private List<EnemyAI> pool;

    public static EnemyObjectPool Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            pool = new List<EnemyAI>();
            PopulatePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void PopulatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            EnemyAI obj = Instantiate(prefab, transform);
            obj.gameObject.SetActive(false);
            pool.Add(obj);
        }
    }

    public EnemyAI GetPooledObject()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].gameObject.activeInHierarchy)
            {
                return pool[i];
            }
        }
        Debug.LogWarning("Enemy Pool is exhausted, expanding pool.");
        EnemyAI obj = Instantiate(prefab, transform);
        obj.gameObject.SetActive(false);
        pool.Add(obj);
        return obj;
    }

    public void ReturnObjectToPool(EnemyAI obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform);
    }

    // Removed HandleEnemyLifecycle as it's not needed for pooling in this context.
}