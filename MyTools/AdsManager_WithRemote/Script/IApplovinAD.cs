using System.Collections;
using UnityEngine;
using System;
using MyTools.SaveManager;

public class IApplovinAD : MonoBehaviour
{
    #region Variables
    [SerializeField] private Platform m_Platform = Platform.Android;

    private bool IsAd_Removed => SaveManager.Prefs.GetBool(SharedVariables.RemoveAds, false) == true;

    [Header("----- Applovin -------------------")]
    public string MaxSdkKey = "ENTER_MAX_SDK_KEY_HERE";
    public MaxSdkBase.BannerPosition Applovin_BannerPostion;
    public MaxSdkBase.AdViewPosition Applovin_BigBannerPosition;
    [Space(2)]
    [SerializeField] ApplovinIds ApplovinAndroidLiveID;
    [SerializeField] ApplovinIds ApplovinIosLiveID;

    [SerializeField] private bool ShowBannerOnLoad = false;
    /// <summary>
    /// Testing AD IDs
    /// </summary>
    
    bool isApplovin_BannerShowing;
    int rewardedInterstitialRetryAttempt;
    int interstitialRetryAttempt;
    int rewardedRetryAttempt;
    bool isMRecShowing;
    bool isApplovin_Initialized;



    /// Private variables
    /// Don't change these if you don't know
    private string Applovin_BannerAdsID, Applovin_InterstitialAdsID, Applovin_RewardedAdsID, Applovin_MRecAdUnitId;
    private Action RewardAction;

    bool isBannerLoaded;
    bool isMrecBannerLoaded;


    [SerializeField] private AdsStatus BannerStatus = AdsStatus.None;
    public AdsStatus CurrentBannerStatus { get { return BannerStatus; } set { BannerStatus = value; } }
    #endregion

    public IEnumerator Initialize_AppLovin(AdSettings adSettings, Platform platform, bool showBanneronLoad)
    {
        MaxSdkKey = adSettings.MaxSDKKey;
        ApplovinAndroidLiveID = adSettings.ApplovinAndroidID;
        ApplovinIosLiveID = adSettings.ApplovinIosID;
        Applovin_BannerPostion = adSettings.ApplovinBannerPos;
        Applovin_BigBannerPosition = adSettings.ApplovinBigBannerPos;
        
        m_Platform = platform;
        ShowBannerOnLoad = showBanneronLoad;

        yield return Set_Ads_IDs_Before_Initializing();

        yield return InitializeApplovin();
    }

    public IEnumerator InitializeApplovin()
    {
        MaxSdk.SetHasUserConsent(true);

        Debug.Log("SDK: APPLOVIN, Initializing...");
        MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
        {
            // AppLovin SDK is initialized, configure and start loading ads.
            isApplovin_Initialized = true;
            Debug.Log("MAX Applovin SDK Initialized");


            if (IsAd_Removed == false)
            {
                // Banner Request
                InitializeBannerAds();

                // Med Banner Request
                InitializeMRecAds();

                // interstitial Request
                InitializeInterstitialAds();
            }

            // Rewarded Request
            InitializeRewardedAds();
        };

        // MaxSdk.SetSdkKey(MaxSdkKey);
        MaxSdk.InitializeSdk();

        yield return null;
    }

    IEnumerator Set_Ads_IDs_Before_Initializing()
    {
        // Initializing Applovin IDs
        if (m_Platform.Equals(Platform.Android))
        {
            //Applovin
            Applovin_BannerAdsID = ApplovinAndroidLiveID.BannerAdUnitId.Trim();
            Applovin_InterstitialAdsID = ApplovinAndroidLiveID.InterstitialAdUnitId.Trim();
            Applovin_RewardedAdsID = ApplovinAndroidLiveID.RewardedAdUnitId.Trim();
            Applovin_MRecAdUnitId = ApplovinAndroidLiveID.MRecAdUnitId.Trim();
        }
        else if (m_Platform.Equals(Platform.IOS))
        {
            //Applovin
            Applovin_BannerAdsID = ApplovinIosLiveID.BannerAdUnitId.Trim();
            Applovin_InterstitialAdsID = ApplovinIosLiveID.InterstitialAdUnitId.Trim();
            Applovin_RewardedAdsID = ApplovinIosLiveID.RewardedAdUnitId.Trim();
            Applovin_MRecAdUnitId = ApplovinIosLiveID.MRecAdUnitId.Trim();
        }           
        yield return null;
    }


    /***************************************************************************************************************************************
                                                            Applovin Section
    //**************************************************************************************************************************************/

    #region Applovin ADS

    #region Banner Ad Methods
    private void InitializeBannerAds()
    {
        if (IsAd_Removed == true)
            return;

        // Attach Callbacks
        MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdFailedEvent;
        MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;

        // Banners are automatically sized to 320x50 on phones and 728x90 on tablets.
        // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments.
        MaxSdk.CreateBanner(Applovin_BannerAdsID, Applovin_BannerPostion);

        // Set background or background color for banners to be fully functional.
        MaxSdk.SetBannerBackgroundColor(Applovin_BannerAdsID, Color.black);
    }

    public void ShowApplovin_Banner()
    {
        isApplovin_BannerShowing = true;
        MaxSdk.ShowBanner(Applovin_BannerAdsID);
    }

    public void HideApplovin_Banner()
    {
        isApplovin_BannerShowing = false;
        MaxSdk.HideBanner(Applovin_BannerAdsID);
    }

    public void DestroyBanner()
    {
        isApplovin_BannerShowing = false;
        MaxSdk.DestroyBanner(Applovin_BannerAdsID);
    }

    private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Banner ad is ready to be shown.
        // If you have already called MaxSdk.ShowBanner(BannerAdUnitId) it will automatically be shown on the next ad refresh.
        BannerStatus = AdsStatus.Ready;
        isBannerLoaded = true;
        Debug.Log("Applovin: Banner ad loaded");
        
        if(ShowBannerOnLoad)
            AdsManager.Instance.ShowBanner();
    }

    private void OnBannerAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        BannerStatus = AdsStatus.NotReady;
        // Banner ad failed to load. MAX will automatically try loading a new ad internally.
        isBannerLoaded = false;
        Debug.Log("Applovin: Banner ad failed to load with error code: " + errorInfo.Code);
    }

    private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Applovin: Banner ad clicked");
    }

    private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Banner ad revenue paid. Use this callback to track user revenue.
        Debug.Log("Applovin: Banner ad revenue paid");

        // Ad revenue
        double revenue = adInfo.Revenue;

        // Miscellaneous data
        string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD"!
        string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
        string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
        string placement = adInfo.Placement; // The placement this ad's postbacks are tied to
    }

    #endregion

    #region Interstitial Ad Methods
    private void InitializeInterstitialAds()
    {
        if (IsAd_Removed == true)
            return;

        // Attach callbacks
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailedToDisplayEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialRevenuePaidEvent;

        // Load the first interstitial
        LoadInterstitial();
    }

    void LoadInterstitial()
    {
        //interstitialStatusText.text = "Loading...";
        MaxSdk.LoadInterstitial(Applovin_InterstitialAdsID);
    }

    public void ShowApplovin_Interstitial()
    {
        if (MaxSdk.IsInterstitialReady(Applovin_InterstitialAdsID))
        {
            // interstitialStatusText.text = "Showing";
            MaxSdk.ShowInterstitial(Applovin_InterstitialAdsID);
        }
        else
        {
            Debug.Log("Applovin: Interstitial is not Ready");
        }
    }

    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'
        // interstitialStatusText.text = "Loaded";
        Debug.Log("Applovin: Interstitial loaded");

        // Reset retry attempt
        interstitialRetryAttempt = 0;
    }

    private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Interstitial ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
        interstitialRetryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempt));

        //interstitialStatusText.text = "Load failed: " + errorInfo.Code + "\nRetrying in " + retryDelay + "s...";
        Debug.Log("Applovin: Interstitial failed to load with error code: " + errorInfo.Code);

        Invoke("LoadInterstitial", (float)retryDelay);
    }

    private void InterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad failed to display. We recommend loading the next ad
        Debug.Log("Applovin: Interstitial failed to display with error code: " + errorInfo.Code);
        LoadInterstitial();
    }

    private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is hidden. Pre-load the next ad
        Debug.Log("Applovin: Interstitial dismissed");
        LoadInterstitial();
    }

    private void OnInterstitialRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad revenue paid. Use this callback to track user revenue.
        Debug.Log("Applovin: Interstitial revenue paid");

        // Ad revenue
        double revenue = adInfo.Revenue;

        // Miscellaneous data
        string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD"!
        string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
        string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
        string placement = adInfo.Placement; // The placement this ad's postbacks are tied to

    }

    #endregion

    #region Rewarded Ad Methods
    private void InitializeRewardedAds()
    {
        // Attach callbacks
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;

        // Load the first RewardedAd
        LoadRewardedAd();
    }

    private void LoadRewardedAd()
    {
        // rewardedStatusText.text = "Loading...";
        MaxSdk.LoadRewardedAd(Applovin_RewardedAdsID);
    }

    public void ShowApplovin_RewardedAd(Action action)
    {
        
        if (MaxSdk.IsRewardedAdReady(Applovin_RewardedAdsID))
        {
            // rewardedStatusText.text = "Showing";
            RewardAction = action;
            MaxSdk.ShowRewardedAd(Applovin_RewardedAdsID);
        }
        else
        {
            Debug.LogError("Applovin: Rewarded Ad is not Ready");
        }
    }

    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(rewardedAdUnitId) will now return 'true'
        //  rewardedStatusText.text = "Loaded";
        Debug.Log("Applovin: Rewarded ad loaded");
        // Reset retry attempt
        rewardedRetryAttempt = 0;
    }

    private void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Rewarded ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
        rewardedRetryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, rewardedRetryAttempt));

        // rewardedStatusText.text = "Load failed: " + errorInfo.Code + "\nRetrying in " + retryDelay + "s...";
        Debug.Log("Applovin: Rewarded ad failed to load with error code: " + errorInfo.Code);

        Invoke("LoadRewardedAd", (float)retryDelay);
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad failed to display. We recommend loading the next ad
        Debug.Log("Applovin: Rewarded ad failed to display with error code: " + errorInfo.Code);
        LoadRewardedAd();
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Applovin: Rewarded ad displayed");
    }

    private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Applovin: Rewarded ad clicked");
    }

    private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is hidden. Pre-load the next ad
        Debug.Log("Applovin: Rewarded ad dismissed");
        LoadRewardedAd();
    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad was displayed and user should receive the reward
        if (RewardAction != null)
            RewardAction?.Invoke();
        else Debug.LogError(" Applovin: Rewarded Action missing");
        Debug.Log("Applovin: Rewarded ad received reward");
    }

    private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad revenue paid. Use this callback to track user revenue.
        Debug.Log("Applovin: Rewarded ad revenue paid");


        // Ad revenue
        double revenue = adInfo.Revenue;

        // Miscellaneous data
        string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD"!
        string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
        string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
        string placement = adInfo.Placement; // The placement this ad's postbacks are tied to
    }

    #endregion

    #region MREC Ad Methods
    private void InitializeMRecAds()
    {
        if (IsAd_Removed == true)
            return;

        // Attach Callbacks
        MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecAdLoadedEvent;
        MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecAdFailedEvent;
        MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecAdClickedEvent;
        MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnMRecAdRevenuePaidEvent;

        // MRECs are automatically sized to 300x250.
        MaxSdk.CreateMRec(Applovin_MRecAdUnitId, Applovin_BigBannerPosition);
    }

    public void Show_ApplovinMRecBanner()
    {
        isMRecShowing = true;
        MaxSdk.ShowMRec(Applovin_MRecAdUnitId);
    }

    public void HideApplovin_MRecBanner()
    {
        isMRecShowing = false;
        MaxSdk.HideMRec(Applovin_MRecAdUnitId);
    }

    public void DestroyMRecBanner()
    {
        isMRecShowing = false;
        MaxSdk.DestroyMRec(Applovin_MRecAdUnitId);
    }

    private void OnMRecAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // MRec ad is ready to be shown.
        isMrecBannerLoaded = true;
        // If you have already called MaxSdk.ShowMRec(MRecAdUnitId) it will automatically be shown on the next MRec refresh.
        Debug.Log("Applovin: MRec ad loaded");
    }

    private void OnMRecAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        isMrecBannerLoaded = false;
        // MRec ad failed to load. MAX will automatically try loading a new ad internally.
        Debug.Log("Applovin: MRec ad failed to load with error code: " + errorInfo.Code);
    }

    private void OnMRecAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Applovin: MRec ad clicked");
    }

    private void OnMRecAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // MRec ad revenue paid. Use this callback to track user revenue.
        Debug.Log("Applovin: MRec ad revenue paid");

        // Ad revenue
        double revenue = adInfo.Revenue;

        // Miscellaneous data
        string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD"!
        string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
        string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
        string placement = adInfo.Placement; // The placement this ad's postbacks are tied to

    }

    #endregion

    #endregion Applovin ADS


     /***************************************************************************************************************************************
                                                             Applovin AD Check Methods
     ****************************************************************************************************************************************/
   
    #region AD Avalibality Check
    public bool IsBannerReady() // method for checking if banner ad is ready
    {
        return isBannerLoaded;
    }
    public bool IsMedBannerReady()// method for checking if Big banner ad is ready
    {
        return isMrecBannerLoaded;
    }
    public bool IsInterstitialReady()// method for checking if Interstitial ad is ready
    {
        return MaxSdk.IsInterstitialReady(Applovin_InterstitialAdsID);
    }
    public bool IsRewardedAdReady()// method for checking if RewardedAd ad is ready
    {
        return MaxSdk.IsRewardedAdReady(Applovin_RewardedAdsID);
    }
    public bool IsAppOpenAdReady()// method for checking if AppOpen ad is ready
    {
        return false;
    }

    #endregion
}