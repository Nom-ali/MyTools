using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class AddressableManager : MonoBehaviour
{
    [SerializeField] private LevelsData addressableReference;

    private static Dictionary<string, object> handleDictionary = new Dictionary<string, object>();


    Data selectedData;

    //todo make addressable sure
    //todo
    public static async Task<T> LoadAsset<T>(AssetReference assetReference) where T : Object
    {
        if (IsAssetLoaded<T>(assetReference.AssetGUID))
        {
            AsyncOperationHandle<T> result = (AsyncOperationHandle<T>)handleDictionary[assetReference.AssetGUID];
            return result.Result;
        }

        AsyncOperationHandle<T> asyncOperation = assetReference.LoadAssetAsync<T>();
        await asyncOperation.Task;

        if (asyncOperation.Status == AsyncOperationStatus.Succeeded)
        {
            handleDictionary[assetReference.AssetGUID] = asyncOperation;
            Debug.Log($"Total keys in handleDictionary: Added, {assetReference.AssetGUID}, Count: {handleDictionary.Count}");
            return asyncOperation.Result;
        }

        return null;
    }


    public async Task<Sprite> LoadWhiteSprite()
    {
        if (IsAssetLoaded<Sprite>(selectedData.WhiteSprite.AssetGUID))
        {
            return ((AsyncOperationHandle<Sprite>)handleDictionary[selectedData.WhiteSprite.AssetGUID]).Result;
        }

        AsyncOperationHandle<Sprite> asyncOperation = selectedData.WhiteSprite.LoadAssetAsync<Sprite>();
        await asyncOperation.Task;
        handleDictionary[selectedData.WhiteSprite.AssetGUID] = asyncOperation;

        return asyncOperation.Result;
    }

    public async Task<GameObject> LoadPhoneCase(int caseID)
    {
        selectedData = addressableReference.GetReference(caseID);

        if (IsAssetLoaded<GameObject>(selectedData.CasePrefab.AssetGUID))
        {
            return (GameObject)handleDictionary[selectedData.CasePrefab.AssetGUID];
        }

        AsyncOperationHandle<GameObject> asyncOperation = selectedData.CasePrefab.InstantiateAsync();
        await asyncOperation.Task;
        GameObject instanceObject = asyncOperation.Result;
        handleDictionary[selectedData.CasePrefab.AssetGUID] = asyncOperation;

        return instanceObject;
    }

    public async Task<SpriteRenderer> LoadSticker(Transform parent)
    {
        string stickerKey = addressableReference.StickerPrefab.AssetGUID;
        AsyncOperationHandle<GameObject> stickerPrefab;

        // Check if the asset is already loaded
        if (IsAssetLoaded<GameObject>(stickerKey))
        {
            stickerPrefab = (AsyncOperationHandle<GameObject>)handleDictionary[stickerKey];
        }
        else
        {
            // Load the asset asynchronously if not loaded
            stickerPrefab = addressableReference.StickerPrefab.LoadAssetAsync();
            await stickerPrefab.Task;

            // Store the asset handle for future use
            handleDictionary[stickerKey] = stickerPrefab;
        }

        // Ensure the asset was loaded successfully
        if (stickerPrefab.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("Failed to load sticker prefab.");
            return null; // Or handle this case appropriately
        }

        // Instantiate the prefab and get the SpriteRenderer
        GameObject temp = Instantiate(stickerPrefab.Result, parent);
        SpriteRenderer spriteRenderer = temp.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("Prefab does not have a SpriteRenderer component.");
            return null; // Or handle this case appropriately
        }

        return spriteRenderer;
    }


    public async Task<SpriteRenderer> LoadChain(int chainID, Transform parent)
    {
        AssetReferenceGameObject assetReferenceGameObject = addressableReference.GetChainReference(chainID);

        string chainKey = assetReferenceGameObject.AssetGUID;
        if (IsAssetLoaded<GameObject>(chainKey))
        {
            AsyncOperationHandle<GameObject> chainPrefab = (AsyncOperationHandle<GameObject>)handleDictionary[chainKey];
            SpriteRenderer _spriteRenderer = chainPrefab.Result.GetComponent<SpriteRenderer>();
            return _spriteRenderer;
        }

        AsyncOperationHandle<GameObject> asyncOperation = assetReferenceGameObject.InstantiateAsync(parent);
        await asyncOperation.Task;
        SpriteRenderer spriteRenderer = asyncOperation.Result.GetComponent<SpriteRenderer>();
        handleDictionary[chainKey] = asyncOperation;
        return spriteRenderer;
    }

    public async Task<GameObject> LoadColoredCase(ColorScheme colorScheme, int caseIndex, Transform parent)
    {
        GameObject instanceObject = null;
        AsyncOperationHandle<Sprite> asyncOperation1;

        // Get reference for the selected sprite asset
        AssetReferenceSprite referenceSprite = await GetSelectedCaseData(colorScheme, caseIndex);

        // Check if the asset is already loaded
        if (IsAssetLoaded<Sprite>(referenceSprite.AssetGUID))
        {
            // If asset is loaded, retrieve it from the dictionary
            asyncOperation1 = (AsyncOperationHandle<Sprite>)handleDictionary[referenceSprite.AssetGUID];
        }
        else
        {
            // Otherwise, load the asset
            asyncOperation1 = referenceSprite.LoadAssetAsync<Sprite>();
            await asyncOperation1.Task;
            handleDictionary[referenceSprite.AssetGUID] = asyncOperation1;
            Debug.Log($"Total keys in handleDictionary: Added, {referenceSprite.AssetGUID}, Count: {handleDictionary.Count}");
        }

        // Load the ColoredCase prefab asynchronously
        AsyncOperationHandle<GameObject> asyncOperation = selectedData.ColoredCasePrefab.InstantiateAsync(parent);
        await asyncOperation.Task;
        instanceObject = asyncOperation.Result;

        // Apply the sprite to the GameObject
        instanceObject.GetComponent<ChangeSprites>().ChangeSprite(asyncOperation1.Result);

        return instanceObject;
    }


    private Task<AssetReferenceSprite> GetSelectedCaseData(ColorScheme type, int selectedCaseID)
    {
        AssetReferenceSprite referenceSprite1 = null;

        switch (type)
        {
            case ColorScheme.Simple:
                referenceSprite1 = selectedData.SimpleSprites[selectedCaseID];
                break;

            case ColorScheme.Gradient:
                referenceSprite1 = selectedData.GradientSprites[selectedCaseID];
                break;

            case ColorScheme.Glitter:
                referenceSprite1 = selectedData.GlitterSprites[selectedCaseID];
                break;
        }

        return Task.FromResult(referenceSprite1);
    }


    static Dictionary<string, AsyncOperationHandle> loadedAssetCache = new();
    static Dictionary<string, List<Object>> loadedLabeledAsset = new();

    public static async Task<List<T>> LoadLabelAssets<T>(AssetLabelReference label)
    {
        string assetType = label.labelString;
        List<T> assets = new List<T>();

        Debug.Log("Asset type: " + label.labelString);
        //Check if assets of the required type are already loaded
        if (loadedAssetCache.ContainsKey(assetType) && loadedLabeledAsset.ContainsKey(assetType))
        {
            return loadedLabeledAsset[assetType].Cast<T>().ToList();
        }

        // Load assets asynchronously from Addressables
        AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(label, (item) =>
        {
            if (item != null)
                assets.Add(item); // Add the item to the assets list
        });

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            // Cache the loaded assets for future use
            loadedAssetCache[assetType] = handle;
            loadedLabeledAsset[assetType] = assets.Cast<Object>().ToList();
            Debug.Log($"Loaded assets: {assets.Count} of type {assetType}");
            Debug.Log("Total keys in loadedAssetCache: " + loadedAssetCache.Count);
        }
        else
        {
            Debug.LogError($"Failed to load assets with label: {label}");
        }

        return assets; // Return the loaded assets of type T
    }

    public static void ReleaseLabeledAsset<T>(AssetLabelReference label) where T : Object
    {
        string assetType = label.labelString;

        Debug.Log("Releasing asset type: " + label.labelString);

        // Check if assets of the required type are loaded
        if (loadedAssetCache.ContainsKey(assetType))
        {
            Addressables.Release(loadedAssetCache[assetType]);
            loadedAssetCache.Remove(assetType);

            loadedLabeledAsset[assetType].Clear();
            loadedLabeledAsset.Remove(assetType);

            Debug.Log("Total keys in loadedAssetCache: " + loadedAssetCache.Count);
            Debug.Log($"Cleared cached assets for type: {assetType}");
        }
        else
        {
            Debug.LogWarning($"No cached assets found for type: {assetType}");
        }
    }

    public static bool IsAssetLoaded<T>(string assetKey) where T : Object
    {
        // Check if the asset is already in the dictionary
        Debug.Log("Total keys in handleDictionary: IsAssetLoaded, " + assetKey + "___" + handleDictionary.Count);
        if (handleDictionary.ContainsKey(assetKey))
        {
            // Try to cast the stored object to the expected AsyncOperationHandle<T>
            AsyncOperationHandle<T> handle =  (AsyncOperationHandle<T>)handleDictionary[assetKey];

            if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
            {
                // Asset is loaded and valid
                return true;
            }
        }

        // If not in the dictionary or not loaded successfully
        return false;
    }

    public static void ReleaseAsset<T>(string assetKey) where T : Object
    {
        // Check if the asset is in the dictionary
        if (handleDictionary.ContainsKey(assetKey))
        {
            AsyncOperationHandle<T> handle = (AsyncOperationHandle<T>)handleDictionary[assetKey];
            Addressables.Release(handle);

            // Release the asset
            Debug.Log($"Asset {assetKey} released.");

            // Remove from the dictionary to prevent memory leaks
            handleDictionary.Remove(assetKey);
            Debug.Log($"Total keys in handleDictionary: Removed, {assetKey}, Count: {handleDictionary.Count}");
        }
        else
        {
            Debug.LogWarning($"Asset {assetKey} not found in the dictionary.");
        }
    }

    public static void ReleaseAllAssets()
    {
        var keysToRelease = new List<string>(handleDictionary.Keys);

        foreach (var Item in handleDictionary)
        {
            //Item.Value
            ReleaseAsset<Object>(Item.Key);
        }
    }

    public static void ReleaseDataFromDictionary()
    {
        // Iterate through each entry in the dictionary
        foreach (var item in handleDictionary)
        {
            string key = item.Key;
            object value = item.Value;

            // Check the type of the value and release it accordingly
            if (value is AsyncOperationHandle<GameObject> gameObjectHandle)
            {
                // Release the GameObject resource
                Addressables.Release(gameObjectHandle);
                Debug.Log($"Released GameObject: {key}");
            }
            else if (value is AsyncOperationHandle<Sprite> spriteHandle)
            {
                // Release the Sprite resource
                Addressables.Release(spriteHandle);
                Debug.Log($"Released Sprite: {key}");
            }
            else
            {
                Debug.LogWarning($"Unknown type encountered for key: {key}, type: {value.GetType()}");
            }
        }

        // After releasing the resources, clear the dictionary
        handleDictionary.Clear();
        Debug.Log("Cleared all handles from dictionary.");
    }
}
