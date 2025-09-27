
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class EnemyPool
{
    public string enemyType; //Name to identify the enemy type.
    public GameObject enemyPrefab; //Preab used to instantiate enemies.
    public int enemyPoolsize = 10; //Number of instances preloaded.
    public Queue<GameObject> poolQueue = new Queue<GameObject>(); //Queue to hold enemy instances.
}

[System.Serializable]
public class BulletPool
{
    public GameObject bulletPrefab;
    public int poolSize = 20;
    public Queue<GameObject> poolQueue = new Queue<GameObject>(); //Queue to hold bullet instances.
}
public class EnemyObjectPool : MonoBehaviour
{/*
    public static EnemyObjectPool instance; //For global access.


    public List<EnemyPool> enemyPools;


    public BulletPool bulletPool;




    private void Awake() //For singletons.
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeEnemyPools();
        InitializeBulletPool();

    }


    private void InitializeEnemyPools() //Pre-instantiates enemies and store them in a queue to prevent runtime instantiation.
    {
        for (int i = 0; i < enemyPools.Count; i++)
        {
            EnemyPool pool = enemyPools[i];
            pool.poolQueue.Clear();

            for (int j = 0; j < pool.enemyPoolsize; j++)
            {
                GameObject enemy = Instantiate(pool.enemyPrefab);
                enemy.SetActive(false);
                pool.poolQueue.Enqueue(enemy);
            }
        }
    }

    private EnemyPool FindEnemyPool(string enemyType)  //Find enemy pool of a given type.
    {
        for (int i = 0; i < enemyPools.Count; i++)
        {
            EnemyPool pool = enemyPools[i];
            if (pool.enemyType == enemyType)
            {
                return pool;
            }
        }
        return null;
    }

    public GameObject GetEnemyFromPool(string enemyType) //Get enemy from the pool.
    {
        EnemyPool pool = FindEnemyPool(enemyType);
        if (pool == null)
        {
            Debug.LogError("Enemy pool for type '" + enemyType + "' not found!");
            return null;
        }

        if (pool.poolQueue.Count > 0)
        {
            GameObject enemy = pool.poolQueue.Dequeue();
            if (enemy.GetComponent<Enemy>() == null)
            {
                Debug.LogWarning("Pooled enemy '" + enemy.name + "' missing Enemy script.");
            }
            enemy.SetActive(true);
            return enemy;
        }
        else
        {
            Debug.LogWarning("Enemy pool '" + enemyType + "' exceeded! Instantiating new enemy.");
            return Instantiate(pool.enemyPrefab);
        }
    }

    public void ReturnEnemyToPool(string enemyType, GameObject enemy) //Return enemy to its respective pool.
    {
        EnemyPool pool = FindEnemyPool(enemyType);
        if (pool == null)
        {
            Debug.LogWarning("Enemy pool for '" + enemyType + "' not found when returning. Destroying object.");
            Destroy(enemy);
            return;
        }

        enemy.SetActive(false); //Deactivate it before returning.
        pool.poolQueue.Enqueue(enemy); //Readded it into the pool.
    }
    /*  public int GetActiveEnemyCount() //Count howmany pool enemies are currently active. This is specifically for wave spawner..
      {
          int count = 0;
          // Check all enemy pools
          foreach (EnemyPool pool in enemyPools)
          {
              // Check each enemy in the pool
              foreach (GameObject enemy in pool.poolQueue)
              {
                  if (enemy != null && enemy.activeInHierarchy)
                  {
                      count++;
                  }
              }
          }
          return count;
      }*/
    /*
    private void InitializeBulletPool()  //Set up bullet pool.
    {
        bulletPool.poolQueue.Clear();
        for (int i = 0; i < bulletPool.poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPool.bulletPrefab);
            bullet.SetActive(false);
            bulletPool.poolQueue.Enqueue(bullet);
        }
    }

    public GameObject GetBulletFromPool() //Retrieve a bullet from pool.
    {
        if (bulletPool.poolQueue.Count > 0)
        {
            GameObject bullet = bulletPool.poolQueue.Dequeue();
            if (bullet.GetComponent<Bullet>() == null)
            {
                Debug.LogWarning("Pooled bullet missing Bullet script.");
            }
            bullet.SetActive(true);
            return bullet;
        }
        else
        {
            Debug.LogWarning("Bullet pool exceeded! Instantiating new bullet.");
            return Instantiate(bulletPool.bulletPrefab);
        }
    }

    public void ReturnBulletToPool(GameObject bullet) //Return bullet to pool after use.
    {
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;  // Reset velocity to avoid ghost movement from bullet.


        bullet.SetActive(false);
        bulletPool.poolQueue.Enqueue(bullet); //Readded to pool.
    }


    public void DisableAllEnemies()
    {
        for (int i = 0; i < enemyPools.Count; i++)
        {
            EnemyPool pool = enemyPools[i];
            GameObject[] pooledEnemies = pool.poolQueue.ToArray();
            for (int j = 0; j < pooledEnemies.Length; j++)
            {
                GameObject enemy = pooledEnemies[j];
                if (enemy.activeInHierarchy)
                {
                    enemy.SetActive(false);
                }
            }
        }
    }

    public void DisableAllBullets() //Disable bullets in the pool.
    {
        GameObject[] pooledBullets = bulletPool.poolQueue.ToArray();
        for (int i = 0; i < pooledBullets.Length; i++)
        {
            GameObject bullet = pooledBullets[i];
            if (bullet.activeInHierarchy)
            {
                bullet.SetActive(false);
            }
        }
    }*/


}
