using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public sealed class MaterialAddressableLoader : MonoBehaviour
{
    [SerializeField] private List<AssetReferenceMaterial> references = new();

    private readonly AddressableAssetCache<Material> _cache = new();

    public int Count => references.Count;

    public Task<Material> LoadAsync(int index)
    {
        return _cache.LoadAsync(references, index, "Material");
    }

    public Task<IReadOnlyList<Material>> LoadManyAsync(IEnumerable<int> indices)
    {
        return _cache.LoadManyAsync(references, indices, "Material");
    }

    public bool TryGetLoaded(int index, out Material material)
    {
        return _cache.TryGetLoaded(index, out material);
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