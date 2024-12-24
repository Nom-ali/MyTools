using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using System.Collections.Generic;

public class AddressableManager : MonoBehaviour
{
    [SerializeField] private LevelsData addressableReference;

    private static Dictionary<string, object> handleDictionary = new Dictionary<string, object>();


    Data selectedData;

    public static async Task<T> LoadAsset<T>(AssetReference assetReference) where T : Object
    {
        if (IsAssetLoaded<T>(assetReference.ToString()))
        {
            return (T)handleDictionary[assetReference.ToString()];
        }

        AsyncOperationHandle<T> asyncOperation = assetReference.LoadAssetAsync<T>();
        await asyncOperation.Task;

        if (asyncOperation.Status == AsyncOperationStatus.Succeeded)
        {
            handleDictionary[assetReference.ToString()] = asyncOperation.Result;
            return asyncOperation.Result;
        }

        return null;
    }


    public async Task<Sprite> LoadWhiteSprite()
    {
        if (IsAssetLoaded<Sprite>(selectedData.WhiteSprite.ToString()))
        {
            return (Sprite)handleDictionary[selectedData.WhiteSprite.ToString()];
        }

        AsyncOperationHandle<Sprite> asyncOperation = selectedData.WhiteSprite.LoadAssetAsync<Sprite>();
        await asyncOperation.Task;

        return asyncOperation.Result;
    }

    public async Task<GameObject> LoadPhoneCase(int caseID)
    {
        selectedData = addressableReference.GetReference(caseID);

        if (IsAssetLoaded<GameObject>(selectedData.CasePrefab.ToString()))
        {
            return (GameObject)handleDictionary[selectedData.CasePrefab.ToString()];
        }

        AsyncOperationHandle<GameObject> asyncOperation = selectedData.CasePrefab.InstantiateAsync();
        await asyncOperation.Task;
        GameObject instanceObject = asyncOperation.Result;

        return instanceObject;
    }
    public async Task<SpriteRenderer> LoadSticker(Transform parent)
    {
        string stickerKey = addressableReference.StickerPrefab.ToString();
        if (IsAssetLoaded<GameObject>(stickerKey))
        {
            GameObject stickerPrefab = (GameObject)handleDictionary[stickerKey];
            SpriteRenderer _spriteRenderer = stickerPrefab.GetComponent<SpriteRenderer>();
            return _spriteRenderer;
        }

        AsyncOperationHandle<GameObject> asyncOperation = addressableReference.StickerPrefab.InstantiateAsync(parent);
        await asyncOperation.Task;
        SpriteRenderer spriteRenderer = asyncOperation.Result.GetComponent<SpriteRenderer>();

        return spriteRenderer;
    }

    public async Task<SpriteRenderer> LoadChain(int chainID, Transform parent)
    {
        AssetReferenceGameObject assetReferenceGameObject = addressableReference.GetChainReference(chainID);

        string chainKey = assetReferenceGameObject.ToString();
        if (IsAssetLoaded<GameObject>(chainKey))
        {
            GameObject chainPrefab = (GameObject)handleDictionary[chainKey];
            SpriteRenderer _spriteRenderer = chainPrefab.GetComponent<SpriteRenderer>();
            return _spriteRenderer;
        }

        AsyncOperationHandle<GameObject> asyncOperation = assetReferenceGameObject.InstantiateAsync(parent);
        await asyncOperation.Task;
        SpriteRenderer spriteRenderer = asyncOperation.Result.GetComponent<SpriteRenderer>();

        return spriteRenderer;
    }

    public async Task<GameObject> LoadColoredCase(ColorScheme colorScheme, int caseIndex, Transform parent)
    {
        GameObject instanceObject = null;
        AsyncOperationHandle<Sprite> asyncOperation1;

        // Get reference for the selected sprite asset
        AssetReferenceSprite referenceSprite = await GetSelectedCaseData(colorScheme, caseIndex);

        // Check if the asset is already loaded
        if (IsAssetLoaded<Sprite>(referenceSprite.ToString()))
        {
            // If asset is loaded, retrieve it from the dictionary
            asyncOperation1 = (AsyncOperationHandle<Sprite>)handleDictionary[referenceSprite.ToString()];
        }
        else
        {
            // Otherwise, load the asset
            asyncOperation1 = referenceSprite.LoadAssetAsync<Sprite>();
            await asyncOperation1.Task;
            handleDictionary[referenceSprite.ToString()] = asyncOperation1;
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
    
     public static async Task<List<T>> LoadAssets<T>(AssetLabelReference label) where T : Object
    {
        List<T> assets = new List<T>();

        if (IsAssetLoaded<T>(label.labelString))
        {
            var handle = (AsyncOperationHandle<IList<T>>)handleDictionary[label.labelString];
            assets.AddRange(handle.Result);
        }
        else
        {
            AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(label, null);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                assets.AddRange(handle.Result);
                handleDictionary[label.labelString] = handle;
                Debug.Log("Loaded assets: " + assets.Count);
            }
            else
            {
                Debug.LogError("Failed to load assets with label: " + label);
            }
        }

        return assets;
    }

    public static bool IsAssetLoaded<T>(string assetKey) where T : Object
    {
        // Check if the asset is already in the dictionary
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

            // Release the asset
            Addressables.Release(handle);
            Debug.Log($"Asset {assetKey} released.");

            // Remove from the dictionary to prevent memory leaks
            handleDictionary.Remove(assetKey);
        }
        else
        {
            Debug.LogWarning($"Asset {assetKey} not found in the dictionary.");
        }
    }

    public static void ReleaseAllAssets()
    {
        var keysToRelease = new List<string>(handleDictionary.Keys);

        foreach (var key in keysToRelease)
        {
            ReleaseAsset<Object>(key);
        }
    }
}
