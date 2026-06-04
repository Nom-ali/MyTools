using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public sealed class PrefabAddressableLoader : MonoBehaviour
{
    [SerializeField] private string Name;
    [SerializeField] private AssetReferenceGameObject[] references = null;

    private readonly AddressableAssetCache<GameObject> _cache = new();
    private readonly HashSet<GameObject> _spawnedInstances = new();

    public int Count => references.Length;

    public Task<GameObject> LoadAsync(int index)
    {
        return _cache.LoadAsync(references, index, "Prefab");
    }

    public Task<IReadOnlyList<GameObject>> LoadManyAsync(IEnumerable<int> indices)
    {
        return _cache.LoadManyAsync(references, indices, "Prefab");
    }

    public bool TryGetLoaded(int index, out GameObject prefab)
    {
        return _cache.TryGetLoaded(index, out prefab);
    }

    public bool Unload(int index)
    {
        return _cache.Unload(index);
    }

    public int UnloadMany(IEnumerable<int> indices)
    {
        return _cache.UnloadMany(indices);
    }

    public void UnloadAll()
    {
        _cache.UnloadAll();
    }

    public async Task<GameObject> InstantiateAsync(
        int index,
        Vector3 position,
        Quaternion rotation,
        Transform parent = null)
    {
        ValidateIndex(index);

        AssetReferenceGameObject reference = references[index];
        ValidateReference(reference, index);

        var handle = Addressables.InstantiateAsync(reference, position, rotation, parent);
        await handle.Task;

        if (handle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded ||
            handle.Result == null)
        {
            throw new Exception($"Failed to instantiate prefab at index {index}.");
        }

        _spawnedInstances.Add(handle.Result);
        return handle.Result;
    }

    public bool ReleaseInstance(GameObject instance)
    {
        if (instance == null)
            return false;

        _spawnedInstances.Remove(instance);
        return Addressables.ReleaseInstance(instance);
    }

    public void ReleaseAllInstances()
    {
        foreach (GameObject instance in _spawnedInstances)
        {
            if (instance != null)
                Addressables.ReleaseInstance(instance);
        }

        _spawnedInstances.Clear();
    }

    private void OnDestroy()
    {
        ReleaseAllInstances();
        UnloadAll();
    }

    private void ValidateIndex(int index)
    {
        if (index < 0 || index >= references.Length)
        {
            throw new IndexOutOfRangeException(
                $"Prefab index {index} is out of range. Valid range: 0 to {references.Length - 1}.");
        }
    }

    private static void ValidateReference(AssetReferenceGameObject reference, int index)
    {
        if (reference == null || !reference.RuntimeKeyIsValid())
        {
            throw new InvalidOperationException(
                $"Prefab reference at index {index} is null or invalid.");
        }
    }
}