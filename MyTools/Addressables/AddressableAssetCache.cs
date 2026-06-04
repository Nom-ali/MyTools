using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public sealed class AddressableAssetCache<TObject> where TObject : UnityEngine.Object
{
    private readonly Dictionary<int, AsyncOperationHandle<TObject>> _loadedHandles = new();
    private readonly Dictionary<int, Task<TObject>> _loadingTasks = new();

    public bool IsLoaded(int index)
    {
        return _loadedHandles.TryGetValue(index, out var handle) && handle.IsValid();
    }

    public bool TryGetLoaded(int index, out TObject asset)
    {
        if (_loadedHandles.TryGetValue(index, out var handle) && handle.IsValid())
        {
            asset = handle.Result;
            return true;
        }

        asset = null;
        return false;
    }

    public async Task<TObject> LoadAsync<TReference>(
        IReadOnlyList<TReference> references,
        int index,
        string category)
        where TReference : AssetReference
    {
        ValidateReferences(references, category);
        ValidateIndex(index, references.Count, category);

        if (_loadedHandles.TryGetValue(index, out var loadedHandle) && loadedHandle.IsValid())
            return loadedHandle.Result;

        if (_loadingTasks.TryGetValue(index, out var existingTask))
            return await existingTask;

        TReference reference = references[index];
        ValidateReference(reference, index, category);

        Task<TObject> loadTask = LoadInternalAsync(reference, index, category);
        _loadingTasks[index] = loadTask;

        try
        {
            return await loadTask;
        }
        finally
        {
            _loadingTasks.Remove(index);
        }
    }

    public async Task<IReadOnlyList<TObject>> LoadManyAsync<TReference>(
        IReadOnlyList<TReference> references,
        IEnumerable<int> indices,
        string category)
        where TReference : AssetReference
    {
        if (indices == null)
            throw new ArgumentNullException(nameof(indices));

        int[] uniqueIndices = indices.Distinct().ToArray();
        if (uniqueIndices.Length == 0)
            return Array.Empty<TObject>();

        TObject[] assets = await Task.WhenAll(
            uniqueIndices.Select(i => LoadAsync(references, i, category)));

        return assets;
    }

    public bool Unload(int index)
    {
        if (!_loadedHandles.TryGetValue(index, out var handle))
            return false;

        if (handle.IsValid())
            Addressables.Release(handle);

        _loadedHandles.Remove(index);
        return true;
    }

    public int UnloadMany(IEnumerable<int> indices)
    {
        if (indices == null)
            throw new ArgumentNullException(nameof(indices));

        int count = 0;

        foreach (int index in indices.Distinct())
        {
            if (Unload(index))
                count++;
        }

        return count;
    }

    public void UnloadAll()
    {
        foreach (var pair in _loadedHandles)
        {
            if (pair.Value.IsValid())
                Addressables.Release(pair.Value);
        }

        _loadedHandles.Clear();
    }

    private async Task<TObject> LoadInternalAsync(
        AssetReference reference,
        int index,
        string category)
    {
        AsyncOperationHandle<TObject> handle = Addressables.LoadAssetAsync<TObject>(reference);
        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
        {
            if (handle.IsValid())
                Addressables.Release(handle);

            throw new Exception($"Failed to load {category} at index {index}.");
        }

        _loadedHandles[index] = handle;
        return handle.Result;
    }

    private static void ValidateReferences<TReference>(
        IReadOnlyList<TReference> references,
        string category)
        where TReference : AssetReference
    {
        if (references == null)
            throw new InvalidOperationException($"{category} reference list is null.");
    }

    private static void ValidateIndex(int index, int count, string category)
    {
        if (index < 0 || index >= count)
            throw new IndexOutOfRangeException(
                $"{category} index {index} is out of range. Valid range: 0 to {count - 1}.");
    }

    private static void ValidateReference(
        AssetReference reference,
        int index,
        string category)
    {
        if (reference == null || !reference.RuntimeKeyIsValid())
        {
            throw new InvalidOperationException(
                $"{category} reference at index {index} is null or invalid.");
        }
    }
}