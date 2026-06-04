using MyTools.SaveManager;
using System;
using System.Collections;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance;

    #region Variables
    [Header("---------- SDK ----------------")]
    [SerializeField] private AdSDK adSDK = AdSDK.AdMob | AdSDK.AppLovin | AdSDK.UnityAds; // Default to all SDKs enabled
  
    [Header("---------- Test Mode ----------")]
    [SerializeField] private Platform m_Platform = Platform.Android;

    [Header("---------- Debugger ----------")]
    [SerializeField]
    bool ShowDebugLogs = false;

    [Header("---------- Fram Limit ----------")]
    [SerializeField]
    bool SetFrames = true;
    [SerializeField] private int Frames = 60;

    [Header("---------- AD Ids --------------")]
    [SerializeField] private AdSettings adSettings; // Reference to the ScriptableObject for Ad configuration

    [Header("------- Remote Default --------")]
    [SerializeField] private bool DEFAULT_ENABLE_ADS = true;
    [SerializeField] private bool DEFAULT_ENABLE_TEST_ADS = false;
   
    [Header("---------- Setting -------------")]
    [SerializeField] private Priority[] AdPriorityOrder = new Priority[3] { Priority.Admob, Priority.Applovin, Priority.Unity};
    [Space]
    [SerializeField] private bool ShowLoadingPanel = false;
   
    [Space]
    [SerializeField] private bool NeverSleepMode = true;
    [SerializeField] private bool ShowBannerOnLoad = false;
    [SerializeField] private bool ShowAppOpenAdOnLoad = false;
    [SerializeField] private bool ShowAppOpenInBackground = false;

    // dont destroy these bool
    private IAdmobAD admobAd = null;
    private IApplovinAD applovinAd = null;
    private IUnityAD unityAd = null;

    private bool admobBannerShowing;
    private bool ApplovinBannerShowing;

    private bool admobMedBannerShowing;
    private bool ApplovinMedBannerShowing;
    private bool isFirstTimeOpen = true; // Starts as true to indicate the app is opening for the first time
    private bool isADLoading;


    private bool ENABLE_ADS = true;
    private bool ENABLE_TEST_ADS = false;

    private bool IsAd_Removed => SaveManager.Prefs.GetBool(SharedVariables.RemoveAds, false) == true;

    #endregion

    #region Awake
    // Creating Instance
    private void Awake()
    {
        try
        {
            ENABLE_ADS = FirebaseManager.Instance.GetRemoteConfigValue(RemoteConfigKeys.ENABLE_ADS, DEFAULT_ENABLE_ADS);
            ENABLE_TEST_ADS = FirebaseManager.Instance.GetRemoteConfigValue(RemoteConfigKeys.ENABLE_TEST_ADS, DEFAULT_ENABLE_TEST_ADS);
        }
        catch
        {
            Debug.LogError("FirebaseManager: Remote Config not found.");
            ENABLE_ADS = DEFAULT_ENABLE_ADS;
            ENABLE_TEST_ADS = DEFAULT_ENABLE_TEST_ADS;
        }

        if (SetFrames)
            Application.targetFrameRate = Frames;

        Debug.unityLogger.logEnabled = ShowDebugLogs;

        if (NeverSleepMode)
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject); // Ensures the old Instance is not duplicated
        }
    }

    #endregion

    #region Start\Initializing
    private void Start()
    {
        StartCoroutine(InitializeSelectedSDKs());
    }

    /// <summary>
    /// Initializes the selected SDKs based on the user's choice in adSDK flags and IDs
    /// </summary>
    private IEnumerator InitializeSelectedSDKs()
    {
        if (ENABLE_ADS == false)
        {
            Debug.Log("Remote Config: ADS are disabled.");
            yield break;
        }

        // Check if AdPriorityOrder is empty
        if (AdPriorityOrder == null || AdPriorityOrder.Length == 0)
        {
            Debug.LogError("Error: No ad priority values assigned in AdPriorityOrder. Please configure the ad priority settings.");
            yield break; // Exit the coroutine early if there's no priority set
        }

        bool? anySDKInitialized = null; // Flag to check if any SDK was initialized

        if(adSettings == null)
            adSettings = Resources.Load<AdSettings>("AdSettings"); // Load the AdSettings ScriptableObject

        yield return new WaitUntil(() => adSettings != null); // Wait until the AdSettings is loaded

        // Check and initialize AdMob
        if ((adSDK & AdSDK.AdMob) != 0)
        {
            // Create GameObjects for each ad manager
            if (admobAd == null)
            {
                GameObject admobGO = new("AdmobAd");
                admobGO.transform.SetParent(transform);
                admobAd = admobGO.AddComponent<IAdmobAD>();  // Assuming AdmobAd is the class that implements IAdmobAD
            }

            if (admobAd != null)
            {
                Debug.Log("Initializing AdMob ID...");
                yield return admobAd.Initialize_Admob(adSettings, m_Platform, ShowBannerOnLoad, ShowAppOpenAdOnLoad, ENABLE_TEST_ADS);


                anySDKInitialized = true; // Set flag to true if AdMob is initialized
            }
            else
            {
                Debug.LogWarning("AdMob SDK not assigned.");
            }
        }

        // Check and initialize AppLovin
        if ((adSDK & AdSDK.AppLovin) != 0)
        {
            if (applovinAd == null)
            {
                GameObject applovinGO = new("ApplovinAd");
                applovinGO.transform.SetParent(transform);
                applovinAd = applovinGO.AddComponent<IApplovinAD>();  // Assuming ApplovinAd is the class that implements IApplovinAD
            }

            if (applovinAd != null)
            {
                Debug.Log("Initializing AppLovin ID...");
                yield return applovinAd.Initialize_AppLovin(adSettings, m_Platform, ShowBannerOnLoad);
                anySDKInitialized = true; // Set flag to true if AppLovin is initialized
            }
            else
            {
                Debug.LogWarning("AppLovin SDK not assigned.");
            }
        }

        // Check and initialize Unity Ads
        if ((adSDK & AdSDK.UnityAds) != 0)
        {

            if (unityAd == null)
            {
                GameObject unityGO = new("UnityAd");
                unityGO.transform.SetParent(transform);
                unityAd = unityGO.AddComponent<IUnityAD>();  // Assuming UnityAd is the class that implements IUnityAD
            }

            if (unityAd != null)
            {
                Debug.Log("Initializing Unity Ads ID...");
                yield return unityAd.InitializeUnity_ID(adSettings, m_Platform, ENABLE_TEST_ADS);

                //Debug.Log("Initializing Unity Ads...");
                //yield return unityAd.InitializeUnity();
                anySDKInitialized = true; // Set flag to true if Unity Ads is initialized
            }
            else
            {
                Debug.LogWarning("Unity Ads SDK not assigned.");
            }
        }

        // Check if no SDKs were initialized
        if (anySDKInitialized == null || !(bool)anySDKInitialized)
        {
            Debug.LogError("Error: No Ad SDK has been initialized. Please check your settings and SDK assignments.");
        }
    }
    #endregion Start\Initializing

    /***************************************************************************************************************************************
                                                           Show Methods
   //**************************************************************************************************************************************/

    #region Show Methods
    /// <summary>
    /// Show Banner ads method.
    /// This is based on selected Platform and priority
    /// </summary>
    /// 

    #region Banner
    public void ShowBanner()
    {
        StartCoroutine(ShowBannerAds());
    }

    private IEnumerator ShowBannerAds()
    {
        if (ENABLE_ADS == false)
        {
            Debug.Log("Remote Config: ADS are disabled.");
            if (admobAd && admobAd.IsBannerReady)
                admobAd.DestroyBanner(); // Hide AdMob banner if it is ready

            if (applovinAd && applovinAd.IsBannerReady())
                applovinAd.DestroyBanner(); // Hide AppLovin banner if it is ready

            yield break;
        }

        if (IsAd_Removed)
        {
            Debug.LogError("Ads are removed");
            yield break; // Ads are disabled, exit the method
        }

        foreach (Priority sdkPriority in AdPriorityOrder)
        {
            if (sdkPriority == Priority.Admob && adSDK.HasFlag(AdSDK.AdMob) && !admobBannerShowing && !ApplovinBannerShowing)
            {
                yield return new WaitUntil(() => admobAd != null && admobAd.CurrentBannerStatus != AdsStatus.None);

                if (admobAd != null && admobAd.AdmobInitialized && admobAd.CurrentBannerStatus == AdsStatus.Ready) // Check if AdMob banner is ready
                {
                    Debug.Log("Showing AdMob banner.");

                    // Show AdMob banner
                    admobAd.ShowAdmobBanner();
                    admobBannerShowing = true;

                    yield break;
                }
                else
                    continue;
            }
            else if (sdkPriority == Priority.Applovin && adSDK.HasFlag(AdSDK.AppLovin) && !ApplovinBannerShowing && !admobBannerShowing)
            {
                yield return new WaitUntil(() => applovinAd != null && applovinAd.CurrentBannerStatus != AdsStatus.None);

                if (applovinAd != null && applovinAd.CurrentBannerStatus == AdsStatus.Ready) // Check if AppLovin banner is ready
                {
                    Debug.Log("Showing AppLovin banner.");

                    // Show AppLovin banner
                    applovinAd.ShowApplovin_Banner();
                    ApplovinBannerShowing = true;

                    yield break;
                }
                else
                    continue;
            }
            else
                Debug.LogError("No banner ad is ready to be shown.");
        }
    }

    #endregion Banner

    #region Big Banner
    public void ShowBigBannerAds()
    {
        if (ENABLE_ADS == false)
        {
            Debug.Log("Remote Config: ADS are disabled.");
            if (admobAd && admobAd.IsMedBannerReady)
                admobAd.DestroyMedBanner(); // Hide AdMob banner if it is ready

            if (applovinAd && applovinAd.IsMedBannerReady())
                applovinAd.DestroyMRecBanner(); // Hide AppLovin banner if it is ready

            return;
        }

        if (IsAd_Removed)
        {
            Debug.LogError("Ads are removed");
            return; // Ads are disabled, exit the method
        }

        foreach (Priority sdkPriority in AdPriorityOrder)
        {
            if (sdkPriority == Priority.Admob && adSDK.HasFlag(AdSDK.AdMob))
            {
                if (admobAd != null && admobAd.AdmobInitialized && admobAd.IsMedBannerReady) // Check if AdMob Med banner is ready
                {
                    Debug.Log("Showing AdMob Med banner.");
                    admobMedBannerShowing = true;
                    admobAd.ShowMedAdmobBanner(); // Show AdMob banner
                    return;
                }
                else
                    continue;
            }
            else if (sdkPriority == Priority.Applovin && adSDK.HasFlag(AdSDK.AppLovin))
            {
                if (applovinAd != null && applovinAd.IsMedBannerReady()) // Check if AppLovin Rec banner is ready
                {
                    Debug.Log("Showing AppLovin Med banner.");
                    ApplovinMedBannerShowing = true;
                    applovinAd.Show_ApplovinMRecBanner(); // Show AppLovin banner
                    return;
                }
                else
                    continue;
            }
            else
                Debug.LogError("No Med banner ad is ready to be shown.");
        }

    }
    #endregion Big Banner

    #region Destroy Banner
    /// <summary>
    /// hide banner
    /// </summary>
    public void DestroyBanner()
    {
        // Check and destroy AdMob banner if it is showing
        if (admobBannerShowing)
        {
            admobAd?.DestroyBanner(); // Call the method to destroy the AdMob banner
            admobBannerShowing = false; // Update the state to indicate banner is no longer showing
        }

        // Check and destroy AppLovin banner if it is showing
        if (ApplovinBannerShowing)
        {
            applovinAd?.HideApplovin_Banner();
            ApplovinBannerShowing = false;
        }
    }

    public void DestroyBigBanner()
    {
        // Check and destroy AdMob Med banner if it is showing
        if (admobMedBannerShowing)
        {
            admobAd?.DestroyMedBanner(); // Call the method to destroy the AdMob Med banner
            admobMedBannerShowing = false; // Update the state to indicate Med banner is no longer showing
        }

        // Check and destroy AppLovin banner if it is showing
        if (ApplovinMedBannerShowing)
        {
            applovinAd?.HideApplovin_MRecBanner(); // Call the method to destroy the AppLovin Med banner
            ApplovinMedBannerShowing = false; // Update the state
        }
    }
    #endregion Destroy Banner

    #region Inter
    public void ShowInterAds()
    {
        StartCoroutine(ShowInterAds_(null));
    }

    public void ShowInterAds(Action action)
    {
        StartCoroutine(ShowInterAds_(action));
    }

    /// <summary>
    /// Show Interstitial ads method.
    /// This is based on selected Platform and priority
    /// </summary>
    private IEnumerator ShowInterAds_(Action action)
    {
        if (ENABLE_ADS == false)
        {
            Debug.Log("Remote Config: ADS are disabled.");
            if (admobAd && admobAd.IsInterstitialReady())
                admobAd.DestroyInterAds(); // Destroy AdMob banner if it is ready

            yield break;
        }

        if (IsAd_Removed)
        {
            Debug.LogError("Ads are removed");
            yield break; // Ads are disabled, exit the method
        }

        //Show Loading Ads Panel
        if (ShowLoadingPanel)
        {
            yield return UIManager.Instance.ShowLoadingPopup(); 
        }

        foreach (Priority sdkPriority in AdPriorityOrder)
        {
            if (sdkPriority == Priority.Admob && adSDK.HasFlag(AdSDK.AdMob))
            {
                if (admobAd != null && admobAd.AdmobInitialized && admobAd.IsInterstitialReady()) // Check if AdMob interstitial is ready
                {
                    Debug.Log("Showing AdMob interstitial.");

                    isADLoading = true;

                    admobAd.ShowAdmobInterAds(); // Show AdMob interstitial
                    StartCoroutine(Reset_AppOpen_ADLoading(2));
                    action?.Invoke(); // Trigger callback after showing ad
                    yield break;
                }
                else
                    continue;
            }
            else if (sdkPriority == Priority.Applovin && adSDK.HasFlag(AdSDK.AppLovin))
            {
                if (applovinAd != null && applovinAd.IsInterstitialReady()) // Check if AppLovin interstitial is ready
                {
                    Debug.Log("Showing AppLovin interstitial.");

                    isADLoading = true;

                    applovinAd.ShowApplovin_Interstitial(); // Show AppLovin interstitial
                    StartCoroutine(Reset_AppOpen_ADLoading(2));
                    action?.Invoke(); // Trigger callback after showing ad
                    yield break;
                }
                else
                    continue;
            }
            else if (sdkPriority == Priority.Unity && adSDK.HasFlag(AdSDK.UnityAds))
            {
                if (unityAd != null && unityAd.IsInterstitialReady()) // Check if Unity interstitial is ready
                {
                    Debug.Log("Showing Unity interstitial.");

                    isADLoading = true;

                    unityAd.ShowUnityInter(); // Show Unity interstitial
                    StartCoroutine(Reset_AppOpen_ADLoading(2));
                    action?.Invoke(); // Trigger callback after showing ad
                    yield break;
                }
                else
                {
                    continue;
                }
            }
            else
                Debug.LogError("No interstitial ad is ready to be shown.");
        }

        ////Any action/event you want to perform after ads failed to show
        action?.Invoke();
    }
    #endregion Inter

    #region Rewarded 
    public void ShowRewardedAds()
    {
        ShowRewardedAds(null, null);

    }

    public void ShowRewardedAds(Action rewardedAction, Action rewardNotReady = null)
    {
        StartCoroutine(ShowRewardedAds_(rewardedAction, rewardNotReady));
    }

    /// <summary>
    /// Show rewarded ads method.
    /// This is based on selected Platform and priority
    /// </summary>
    /// <param name="rewardedAction"> You can assign reward method as a parameter and it will auto assign after ads completion</param>
    private IEnumerator ShowRewardedAds_(Action rewardedAction, Action RewardNotReady = null)
    {
        if (ENABLE_ADS == false)
        {
            Debug.Log("Remote Config: ADS are disabled.");
            if (admobAd && admobAd.IsRewardedAdReady())
                admobAd.DestroyRewardedAds(); // Hide AdMob banner if it is ready

            yield break;
        }

        //Show Loading Ads Panel
        if (ShowLoadingPanel)
        {
            yield return UIManager.Instance.ShowLoadingPopup();
        }

        foreach (Priority sdkPriority in AdPriorityOrder)
        {
            if (sdkPriority == Priority.Admob && adSDK.HasFlag(AdSDK.AdMob))
            {
                if (admobAd != null && admobAd.AdmobInitialized && admobAd.IsRewardedAdReady()) // Check if AdMob Rewarded is ready
                {
                    Debug.Log("Showing AdMob Rewarded.");

                    isADLoading = true;

                    admobAd.ShowAdmobRewardedAds(rewardedAction);
                    StartCoroutine(Reset_AppOpen_ADLoading(2));
                    yield break;
                }
                else
                    continue;
            }
            else if (sdkPriority == Priority.Applovin && adSDK.HasFlag(AdSDK.AppLovin))
            {
                if (applovinAd != null && applovinAd.IsRewardedAdReady()) // Check if AppLovin Rewarded is ready
                {
                    Debug.Log("Showing AppLovin Rewarded.");

                    isADLoading = true;

                    applovinAd.ShowApplovin_RewardedAd(rewardedAction);
                    StartCoroutine(Reset_AppOpen_ADLoading(2));
                    yield break;
                }
                else
                    continue;
            }
            else if (sdkPriority == Priority.Unity && (adSDK & AdSDK.UnityAds) != 0)
            {
                if (unityAd != null && unityAd.IsRewardedAdReady()) // Check if Unity Rewarded is ready
                {
                    Debug.Log("Showing Unity Rewarded.");

                    isADLoading = true;

                    unityAd.ShowUnityRewarded(rewardedAction);
                    StartCoroutine(Reset_AppOpen_ADLoading(2));
                    yield break;
                }
                else
                    continue;
            }
            else
                Debug.LogError("No rewarded ad is ready to be shown.");
        }

        RewardNotReady?.Invoke();
    }
    #endregion Rewarded

    #region AppOpen
    IEnumerator Reset_AppOpen_ADLoading(float wait)
    {
        yield return new WaitForSeconds(wait);
        isADLoading = false;
    }

    public bool CanShowADOpenAD()
    {
        return admobAd && admobAd.IsAppOpenAdReady;
    }
    public void ShowAppOpen()
    {
        if (ENABLE_ADS == false)
        {
            Debug.Log("Remote Config: ADS are disabled.");
            if (admobAd && admobAd.IsAppOpenAdReady)
                admobAd.DestroyAppOpenAd(); // Hide AdMob banner if it is ready

            return;
        }

        //if (IsAd_Removed || DontLoadAds)
        //{
        //    // Ads are disabled, exit the method
        //    Debug.LogError("Ads are removed");
        //    return; 
        //}

        if (admobAd && admobAd.AdmobInitialized && admobAd.IsAppOpenAdReady)
        {
            admobAd.ShowAppOpenAd();
        }
    }
    #endregion AppOpen

    #region Application Paused
    private void OnApplicationPause(bool pauseStatus)
    {
        if (IsAd_Removed || !ShowAppOpenInBackground)
        {
            Debug.LogError("No App Open");
            return; // Ads are disabled, exit the method
        }

        // If this is the first time the app has opened, skip showing the AppOpen ad
        if (isFirstTimeOpen)
        {
            // Debug.Log("Skipping App Open ad because it's the first time the app has opened.");
            isFirstTimeOpen = false; // Mark that the first time has passed
            return;
        }

        if (!pauseStatus && !isADLoading && admobAd && admobAd.AdmobInitialized && admobAd.IsAppOpenAdReady)
        {
            // App has resumed from the background
            Debug.Log("App resumed from the background. Attempting to show App Open Ad.");
            admobAd.ShowAppOpenAd();
        }
    }
    #endregion Application Paused

    #endregion Show Methods
}

    /***************************************************************************************************************************************
                                                             Extras
    //**************************************************************************************************************************************/

#region Extras
[Serializable, Flags]
public enum AdSDK
{
    None = 0,
    AdMob = 1 << 0,
    AppLovin = 1 << 1,
    UnityAds = 1 << 2,
}

[Serializable]
public enum Platform
{
    Android, IOS
}

[System.Serializable]
public enum Priority
{
    Admob,
    Unity,
    Applovin,
}

[Serializable]
public enum AdsStatus
{
    None,
    Ready,
    NotReady,
}
#endregion