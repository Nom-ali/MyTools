using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class ObjectPool
{
    public Dictionary<string, Queue<GameObject>> poolDictionary; // Dictionary for quick access to pools
    
    [SerializeField] private PoolEnqueueType PoolEnqueueType = PoolEnqueueType.None;
    [SerializeField] private string KeyName = "Key";
    [SerializeField] private bool ShuffleQueues = false;

    [SerializeField] private Transform Container = null;
    [SerializeField] private int AutoSpawnCount = 0; // Number of objects to auto-spawn at start, if needed

    [Space]
    [SerializeField] private List<Pool> pools; // List of pools

    internal string Key => KeyName;

    #region Creating Pool
    internal IEnumerator CreatePool(Transform container)
    {
        if (PoolEnqueueType == PoolEnqueueType.None)
            yield break;

        if (container)
            Container = container;
        else
        {
            GameObject temp = new GameObject("Container");
            Container = temp.transform;
        }

        if (PoolEnqueueType == PoolEnqueueType.Separate)
            yield return CreatePool_Separate();
        else if (PoolEnqueueType == PoolEnqueueType.Single)
        {
            yield return CreatePool_Single();
            if (ShuffleQueues) ShuffleQueue(KeyName);
        }
    }

    IEnumerator CreatePool_Single()
    {
        poolDictionary ??= new Dictionary<string, Queue<GameObject>>();
        poolDictionary.Clear();

        Queue<GameObject> pooled = new();

        // Initialize the pools
        foreach (Pool pool in pools)
        {
            // Create and store pooled objects

            pool.size = pool.size.GetValidBaseCount(2, 2);
            Debug.Log($"Creating pool for {pool.prefab.name} with size {pool.size}");
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Object.Instantiate(pool.prefab, Container);
                obj.name = $"{KeyName}_{pool.prefab.name}_{i}";
                obj.SetActive(false); // Disable the object initially
                pooled.Enqueue(obj); // Add to the queue
            }
            yield return null;
        }
        poolDictionary.Add(KeyName, pooled); // Add the pool to the dictionary
        Debug.Log("Dictionary Length: " + poolDictionary.Count);
        yield return null;
    }

    IEnumerator CreatePool_Separate()
    {
        poolDictionary ??= new Dictionary<string, Queue<GameObject>>();
        poolDictionary.Clear(); 

        // Initialize the pools
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new();

            // Create and store pooled objects
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Object.Instantiate(pool.prefab, Container);
                obj.SetActive(false); // Disable the object initially
                objectPool.Enqueue(obj); // Add to the queue
                yield return null;
            }
            poolDictionary.Add(pool.SeparateKey, objectPool); // Add the pool to the dictionary
        }
        Debug.Log("Dictionary Length: " + poolDictionary.Count);
        yield return null;
    }

    #endregion


    #region Shuffle
    public void ShuffleQueue(string key)
    {
        if (poolDictionary.ContainsKey(key))
            poolDictionary[Key].Shuffle();
    }
    #endregion


    #region Pool Size

    public int GetPoolSize(string key)
    {
        return poolDictionary.TryGetValue(key, out var queue) ? queue.Count : 0;
    }

    public int GetSinglePoolSize() => GetPoolSize(KeyName);
    #endregion Pool Size

    #region Dequeue
    // Method to get an object from the pool
    public GameObject SpawnFromPool(string key)
    {
        if (!poolDictionary.ContainsKey(key) || poolDictionary[key].Count == 0)
        {
            Debug.LogWarning($"Pool with tag {key} doesn't exist.");

        }

        GameObject objectToSpawn = poolDictionary[key].Dequeue(); // Get the object from the pool
        return objectToSpawn; // Return the spawned object
    }

    public (string Key, GameObject pooled) SpawnFromSinglePool(bool IsActive)
    {
        if (!poolDictionary.ContainsKey(KeyName))
        {
            Debug.LogWarning($"Pool with tag {KeyName} doesn't exist.");
            return default;
        }

        if (poolDictionary[KeyName].Count <= 0) 
        {
            Debug.LogError($"No object in this {KeyName} Pool");
            return default;
        }

        GameObject objectToSpawn = poolDictionary[KeyName].Dequeue(); // Get the object from the pool
        objectToSpawn.SetActive(IsActive); // Activate the object

        return (KeyName, objectToSpawn); // Return the spawned object
    }

    public GameObject SpawnFromPool(string tag, bool IsActive)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue(); // Get the object from the pool
        objectToSpawn.SetActive(IsActive); // Activate the object

        return objectToSpawn; // Return the spawned object
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue(); // Get the object from the pool
        objectToSpawn.SetActive(true); // Activate the object
        objectToSpawn.transform.position = position; // Set position
        objectToSpawn.transform.rotation = rotation; // Set rotation

        return objectToSpawn; // Return the spawned object
    }

    internal (string, GameObject)[] AutoSpawn()
    {
        if (AutoSpawnCount > 0 && poolDictionary[KeyName].Count >= AutoSpawnCount)
        {
            (string, GameObject)[] gameObjects = new (string, GameObject)[AutoSpawnCount];

            for (int i = 0; i < AutoSpawnCount; i++)
            {
                if (poolDictionary.ContainsKey(KeyName))
                {
                    (string, GameObject) obj = SpawnFromSinglePool(false);
                    gameObjects[i] = obj;
                }
                else
                {
                    Debug.LogWarning($"Pool with tag {KeyName} doesn't exist.");
                }
            }
            return gameObjects;
        }

        return null;
    }

    #endregion Dequeue

    #region Enqueue
    public void ReturnObject(string key, GameObject returnedObj)
    {
        if (!poolDictionary.ContainsKey(key))
        {
            Debug.LogWarning($"Pool with tag {key} doesn't exist.");
            return;
        }
        int beforeCount = poolDictionary[key].Count; // Get the count before adding
        returnedObj.SetActive(false); // Deactivate the object before returning to pool
        poolDictionary[key].Enqueue(returnedObj); // Add it back to the pool
        Debug.Log(SpawnFromPool(key).name + " returned to pool: " + key + " | Before Count: " + beforeCount + " | After Count: " + poolDictionary[key].Count);
    }
    #endregion Enqueue

}


[System.Serializable]
public class Pool
{
    public string SeparateKey = "Key"; // Identifier for the pool
    public GameObject prefab = null; // Prefab to pool
    public int size = 5; // Number of objects to pool
    public int MaxSize = 20; // Maximum size of the pool, if needed
}

[System.Serializable]
public enum PoolEnqueueType
{
    None, Single, Separate
}

