using Firebase.Extensions;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    [SerializeField] private FirebaseSDK SelectedFirebaseSDK = FirebaseSDK.RemoteConfig | FirebaseSDK.Analytics;

    [Space]
    [SerializeField] private CustomAnalytics customAnalytics;
    [SerializeField] private FirebaseRemoteConfigManager remoteConfig;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        if(SelectedFirebaseSDK == FirebaseSDK.None)
        {
            Debug.LogError("Firebase: No SDK selected. Please select at least one SDK.");
            return;
        }

        await Setup();
        await Custom_Start();
    }

    private async Task Setup()
    {
        if (SelectedFirebaseSDK.HasFlag(FirebaseSDK.Analytics) && customAnalytics == null)
        {
            customAnalytics = gameObject.AddComponent<CustomAnalytics>();
        }
        if (SelectedFirebaseSDK.HasFlag(FirebaseSDK.RemoteConfig) && remoteConfig == null)
        {
            remoteConfig = gameObject.AddComponent<FirebaseRemoteConfigManager>();
        }
        await Task.CompletedTask;
    }

    private async Task Custom_Start()
    {
        await Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                if (SelectedFirebaseSDK.HasFlag(FirebaseSDK.Analytics))
                {
                    customAnalytics.Initialize(dependencyStatus);
                }

                if (SelectedFirebaseSDK.HasFlag(FirebaseSDK.RemoteConfig))
                {
                    remoteConfig.InitializeFirebase(dependencyStatus);
                }

                // Set a flag here to indicate whether Firebase is ready to use by your app.
                Debug.Log("Firebase: is ready to Use!!!");
            }
            else
            {
                Debug.LogError($"Firebase: Could not resolve all Firebase dependencies: {dependencyStatus}");
                // Firebase Unity SDK is not safe to use here.
            }
        });

        await Task.CompletedTask;
    }


    public T GetRemoteConfigValue<T>(string key, T defaultValue)
    {
        if (remoteConfig == null)
        {
            Debug.LogError("Firebase: RemoteConfigManager is not initialized:\nReturning default value: Key: {key}, VAlue: {defaultValue}");
            return defaultValue;
        }
        try
        {
            var value = remoteConfig.GetConfigValue<T>(key, defaultValue);
            return value;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Firebase: Error getting config value for key '{key}': {ex.Message}");
            return defaultValue;
        }
    }

    public void LogEvents(params string[] messages)
    {
        if(customAnalytics == null)
        {
            Debug.LogError("Firebase: CustomAnalytics is not initialized.");
            return;
        }
        if (messages == null || messages.Length == 0)
        {
            Debug.LogError("Firebase: No messages to log.");
            return;
        }

        customAnalytics.LogEvent(messages);
    }
}

[System.Serializable, System.Flags]
public enum FirebaseSDK
{
    None = 0,
    Analytics = 1 << 0,     // 1
    RemoteConfig = 1 << 1   // 2
}