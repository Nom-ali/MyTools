using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public sealed class SpriteAddressableLoader : MonoBehaviour
{
    [SerializeField] private List<AssetReferenceSprite> references = new();

    private readonly AddressableAssetCache<Sprite> _cache = new();

    public int Count => references.Count;

    public Task<Sprite> LoadAsync(int index)
    {
        return _cache.LoadAsync(references, index, "Sprite");
    }

    public Task<IReadOnlyList<Sprite>> LoadManyAsync(IEnumerable<int> indices)
    {
        return _cache.LoadManyAsync(references, indices, "Sprite");
    }

    public bool TryGetLoaded(int index, out Sprite sprite)
    {
        return _cache.TryGetLoaded(index, out sprite);
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