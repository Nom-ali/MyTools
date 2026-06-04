#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AdSettings))]
public class AdSettingsEditor : Editor
{
    private const string GoogleMobileAdsSettingsResourceName = "GoogleMobileAdsSettings";

    public override void OnInspectorGUI()
    {
        // Draw the default AdSettings inspector first
        DrawDefaultInspector();

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Auto Set App IDs"))
        {
            AutoSetAdMobAppIds((AdSettings)target);
        }
    }

    private static void AutoSetAdMobAppIds(AdSettings adSettings)
    {
        if (adSettings == null)
        {
            Debug.LogError("AdSettings is null.");
            return;
        }

        // Editor-only settings asset from the AdMob plugin
        var googleAdsSettings =
            Resources.Load<GoogleMobileAds.Editor.GoogleMobileAdsSettings>(GoogleMobileAdsSettingsResourceName);

        if (!googleAdsSettings)
        {
            Debug.LogError($"File Not Found: {GoogleMobileAdsSettingsResourceName} (must be inside a Resources folder).");
            return;
        }

        // Validate using your extension method (returns trimmed string or empty)
        string androidAppId = adSettings.AdmobAndroidID.AppID;
        if(string.IsNullOrEmpty(androidAppId))
            Debug.LogError("AdMob Android App ID is invalid/empty in AdSettings.");
        else 
            androidAppId = adSettings.AdmobAndroidID.AppID.IsThisValid(IdFormat.AdmobAppId);

        string iosAppId = adSettings.AdmobIosID.AppID;
        if (string.IsNullOrEmpty(iosAppId)) 
            Debug.LogError("AdMob iOS App ID is invalid/empty in AdSettings.");
        else
            iosAppId = adSettings.AdmobIosID.AppID.IsThisValid(IdFormat.AdmobAppId);

        googleAdsSettings.GoogleMobileAdsAndroidAppId = androidAppId;
        Debug.Log($"AdMob Android App ID set to: {androidAppId}");

        googleAdsSettings.GoogleMobileAdsIOSAppId = iosAppId;
        Debug.Log($"AdMob iOS App ID set to: {iosAppId}");

        googleAdsSettings.UserTrackingUsageDescription = adSettings.UserTrackingUsageDescription;
        if (string.IsNullOrEmpty(adSettings.UserTrackingUsageDescription))
            Debug.LogError("Admob: String: 'UserTrackingUsageDescription' is empty");
        
        // Persist to disk
        EditorUtility.SetDirty(googleAdsSettings);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("GoogleMobileAdsSettings saved successfully.");
    }
}
#endif