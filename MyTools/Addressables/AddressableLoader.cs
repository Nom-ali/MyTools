using UnityEngine;

public sealed class AddressableLoader : MonoBehaviour
{
    [SerializeField] internal PrefabAddressableLoader levelLoader;


    private void OnDestroy()
    {
        levelLoader.UnloadAll();
    }
}