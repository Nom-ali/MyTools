using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using UnityEngine;

public class FirebaseRemoteConfigManager : MonoBehaviour
{
    [SerializeField] private uint fetchIntervalInSeconds = 3600; // Default to 1 hour
    [SerializeField] private bool fetchOnAwake = true;

    private bool isFirebaseInitialized = false;
    private DateTime lastFetchTime;
  
    internal async void InitializeFirebase(DependencyStatus dependencyStatus)
    {
        try
        {
            if (dependencyStatus == DependencyStatus.Available)
            {
                SetDefaultValues();
                isFirebaseInitialized = true;

                if (fetchOnAwake)
                {
                    await FetchRemoteConfigAsync();
                }
            }
            else
            {
                Debug.LogError($"Remote: Could not resolve Firebase dependencies: {dependencyStatus}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Remote: initialization failed: {ex.Message}");
        }
    }

    private void SetDefaultValues()
    {
        var defaults = new Dictionary<string, object>
        {
            { RemoteConfigKeys.ENABLE_ADS, true },
            { RemoteConfigKeys.ENABLE_TEST_ADS, false },
        };

        FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Remote: Failed to set default values: {task.Exception}");
            }
        });
    }

    public async Task<bool> FetchRemoteConfigAsync()
    {
        if (!isFirebaseInitialized)
        {
            Debug.LogWarning("Remote: Firebase is not initialized yet. Cannot fetch remote config.");
            return false;
        }

        // Check if we need to respect fetch cooldown
        TimeSpan timeSinceLastFetch = DateTime.UtcNow - lastFetchTime;
        if (timeSinceLastFetch.TotalSeconds < fetchIntervalInSeconds)
        {
            Debug.Log($"Remote: Skipping fetch due to cooldown. Next fetch available in {fetchIntervalInSeconds - timeSinceLastFetch.TotalSeconds} seconds");
            // Use cached values instead
            //ApplyRemoteConfig();
            return true;
        }

        try
        {
            // Use a reasonable timeout value (3 seconds) instead of TimeSpan.Zero
            await FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.FromSeconds(3));
            lastFetchTime = DateTime.UtcNow;

            bool activated = await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();

            if (activated)
            {
                Debug.Log("Remote: config fetched and activated successfully");
            }
            else
            {
                Debug.Log("Remote: config was fetched but not activated (no changes found)");
            }

            //ApplyRemoteConfig();
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Remote: config fetch or activation failed: {ex.Message}");
            return false;
        }
    }

    // Non-async version for compatibility with existing code
    public void FetchRemoteConfig()
    {
        _ = FetchRemoteConfigAsync();
    }

    private void ApplyRemoteConfig()
    {
        try
        {
            // Get config values safely
            bool ENABLE_ADS = (bool)FirebaseRemoteConfig.DefaultInstance.GetValue(RemoteConfigKeys.ENABLE_ADS).BooleanValue;
            bool ENABLE_TEST_ADS = (bool)FirebaseRemoteConfig.DefaultInstance.GetValue(RemoteConfigKeys.ENABLE_TEST_ADS).BooleanValue;

            Debug.Log($"Applying config values - ENABLE_ADS: {ENABLE_ADS}, ENABLE_TEST_ADS: {ENABLE_TEST_ADS}");

            // Apply these values in your game logic with null checks

        }
        catch (Exception ex)
        {
            Debug.LogError($"Error applying remote config values: {ex.Message}");
        }
    }

    // Helper method to get a config value with a fallback
    internal T GetConfigValue<T>(string key, T defaultValue)
    {
        if (!isFirebaseInitialized)
        {
            Debug.Log($"Remote: Firebase not initialized. Returning default value for key: {key}");
            return defaultValue;
        }

        try
        {
            ConfigValue value = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
            if (typeof(T) == typeof(bool))
            {
                T result = (T)(object)value.BooleanValue;
                Debug.Log($"Remote: Key: {key}, Value: {result}");
                return result;
            }
            else if (typeof(T) == typeof(int) || typeof(T) == typeof(long))
            {
                T result = (T)(object)(int)value.LongValue;
                Debug.Log($"Remote: Key: {key}, Value: {result}");
                return result;
            }
            else if (typeof(T) == typeof(float) || typeof(T) == typeof(double))
            {
                T result = (T)(object)(float)value.DoubleValue;
                Debug.Log($"Remote: Key: {key}, Value: {result}");
                return result;
            }
            else if (typeof(T) == typeof(string))
            {
                T result = (T)(object)value.StringValue;
                Debug.Log($"Remote: Key: {key}, Value: {result}");
                return result;
            }

            Debug.Log($"Remote: Key: {key} has unsupported type. Returning default value.");
            return defaultValue;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Remote: Error retrieving value for key: {key}, Returning default value. Exception: {ex.Message}");
            return defaultValue;
        }
    }

    // Optional: Add a method to get the last fetch time
    public DateTime GetLastFetchTime()
    {
        return lastFetchTime;
    }

    // Optional: Add method to force an immediate fetch regardless of cooldown
    public async Task<bool> ForceFetchRemoteConfigAsync()
    {
        if (!isFirebaseInitialized)
        {
            Debug.LogWarning("Remote: Firebase is not initialized yet. Cannot fetch remote config.");
            return false;
        }

        try
        {
            await FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.FromSeconds(3));
            lastFetchTime = DateTime.UtcNow;

            bool activated = await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
            Debug.Log(activated ?
                "Remote: config force fetched and activated" :
                "Remote: config force fetched but not activated (no changes found)");

            //ApplyRemoteConfig();
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Remote: Force fetch of remote config failed: {ex.Message}");
            return false;
        }
    }
}