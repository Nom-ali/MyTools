using UnityEngine;
using System.Collections.Generic;

public class SingletonManager : MonoBehaviour
{
    // A dictionary to hold references to singleton instances
    private static Dictionary<System.Type, MonoBehaviour> singletons = new Dictionary<System.Type, MonoBehaviour>();

    // Register a MonoBehaviour-derived class as a singleton
    public static void RegisterSingleton<T>(T instance) where T : MonoBehaviour
    {
        // Check if the instance is null
        if (instance == null)
        {
            Debug.LogError("Attempted to register a null instance as a singleton.");
            return;
        }

        // If the singleton is already registered, log a warning and return
        if (singletons.ContainsKey(typeof(T)))
        {
            Debug.LogWarning($"{typeof(T)} is already registered as a singleton.");
            return;
        }

        // Register the singleton instance in the dictionary
        singletons[typeof(T)] = instance;

        // Optionally, you can ensure the singleton is a child of this manager for scene persistence
        instance.transform.SetParent(SingletonManager.instance.transform);

        Debug.Log($"{typeof(T)} successfully registered as a singleton, Count: {singletons.Count}");
    }

    // Unregister a MonoBehaviour-derived class from the singletons dictionary
    public static void UnregisterSingleton<T>(bool destroyGameObject = false) where T : MonoBehaviour
    {
        // Check if the singleton exists
        if (singletons.ContainsKey(typeof(T)))
        {
            // Log the unregistration

            // Optionally, destroy the GameObject if specified
            if (destroyGameObject)
            {
                GameObject singletonObject = singletons[typeof(T)].gameObject;
                Destroy(singletonObject);
                Debug.Log($"{typeof(T)}'s GameObject destroyed.");
            }

            // Remove the singleton from the dictionary
            singletons.Remove(typeof(T));
            Debug.Log($"{typeof(T)} successfully unregistered as a singleton, Count: {singletons.Count}");
        }
        else
        {
            Debug.LogWarning($"{typeof(T)} not registered as a singleton, cannot unregister.");
        }
    }



    // Retrieve the singleton instance by type
    public static T GetSingleton<T>() where T : MonoBehaviour
    {
        // Check if the singleton exists
        if (singletons.ContainsKey(typeof(T)))
        {
            return singletons[typeof(T)] as T;
        }

        // If the singleton doesn't exist, log an error and return null
        Debug.LogError($"{typeof(T)} singleton not found!");
        return null;
    }

    // Optionally, clear all registered singletons
    public static void ClearSingletons()
    {
        singletons.Clear();
    }

    // SingletonManager instance for easier access
    private static SingletonManager instance;

    private void Awake()
    {
        // Ensure only one SingletonManager exists and persist it across scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
