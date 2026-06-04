using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public sealed class AudioClipAddressableLoader : MonoBehaviour
{
    [SerializeField] private List<AssetReferenceAudioClip> references = new();

    private readonly AddressableAssetCache<AudioClip> _cache = new();

    public int Count => references.Count;

    public Task<AudioClip> LoadAsync(int index)
    {
        return _cache.LoadAsync(references, index, "AudioClip");
    }

    public Task<IReadOnlyList<AudioClip>> LoadManyAsync(IEnumerable<int> indices)
    {
        return _cache.LoadManyAsync(references, indices, "AudioClip");
    }

    public bool TryGetLoaded(int index, out AudioClip clip)
    {
        return _cache.TryGetLoaded(index, out clip);
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

    private void OnDestroy()
    {
        UnloadAll();
    }
}